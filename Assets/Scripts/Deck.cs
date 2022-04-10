using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.InputSystem;


public class Deck : MonoBehaviour
{
    public static  int INITIAL_DECK_SIZE=7;
    public SpriteRenderer spriteRenderer;

    public TextAsset godsJSON;
    AllCards allCards;
    public List<CardDO>  deck;
    public Sprite[] spriteArray;
    public Card cardPrefab;
    
    private int deckSize;
    public Card theCard;

    private bool isSpawning = false;
    public Gods godsInfo;

    private Camera mainCamera;

    private static Deck _instance;
    public static Deck Instance{ get { return _instance; } }

    private List<Card> foresightCards = new List<Card>();

    [SerializeField]
    public InputAction forsightMouseClick;

    public bool performingForesight = false;

    public bool triggerInsertCardAnimation = false;
    public AudioClip shuffleSound;

    public ShuffleText shuffleText;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
        mainCamera = Camera.main;

    }
    public CardDO GetCard(string cardName){
        return allCards.allCardsList.Find(card=>card.GetTitle()==cardName);
    }

    
    
    public void AddToDiscard(ShuffleDO shuffle){
        List<CardDO> cardsToAdd = new List<CardDO>();
        if(shuffle.lowerbound!=999&&shuffle.upperbound!=999){
            for(int j=0;j<shuffle.numcards;j++){
                CardDO cardToAdd;
                int randomCardNum = randomNumber(shuffle.lowerbound,shuffle.upperbound+1);
                cardToAdd = allCards.allCardsList.Find(card=>card.GetNumber()==randomCardNum);
                if(shuffle.allowsdupes==1 || !cardsToAdd.Contains(cardToAdd)){
                    cardsToAdd.Add(cardToAdd);
                }
                else j--;
            }
        }
        cardsToAdd.AddRange(shuffle.specificids.Select(id=>allCards.allCardsList.Find(card=>card.GetNumber()==id)));
        
        Discard.Instance.discard.AddRange(cardsToAdd);
        triggerInsertCardAnimation = cardsToAdd.Count>0;
    }

    public IEnumerator CheckIfInsertCard(Card currentCard){
        if(triggerInsertCardAnimation){
            
            Vector3 belowCardPosition = currentCard.transform.position+Vector3.down*currentCard.spriteRenderer.bounds.size.y*1.2f;
            Card insertionCard = Instantiate(cardPrefab,belowCardPosition,currentCard.transform.rotation);
            yield return insertionCard.FadeIn();
            float stepsFromCardToDiscard = (Vector3.Distance(currentCard.transform.position,Discard.Instance.transform.position)/30f);
            float belowCardVelocity = Vector3.Distance(belowCardPosition,Discard.Instance.transform.position)/stepsFromCardToDiscard;
            StartCoroutine(insertionCard.MoveToAndDestroy(Discard.Instance.transform.position,belowCardVelocity));
        }
    }

    public CardDO GetTopCard(){
        return Deck.Instance.deck[0];
    }
    // Start is called before the first frame update
    void Start()
    {
        float worldScreenHeight = Camera.main.orthographicSize;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        float newWidth = worldScreenWidth/11f;
        transform.localScale = new Vector3(newWidth,transform.localScale.y*(newWidth/transform.localScale.x),transform.localScale.z);
        cardPrefab.transform.localScale = transform.localScale;
        transform.position = new Vector3(worldScreenWidth+spriteRenderer.bounds.size.x*0.6f,
                                         worldScreenHeight-spriteRenderer.bounds.size.y/1.8f,
                                         transform.position.z);
        
        this.allCards = new AllCards(Loader.CultCardsJsons.Select(json=>JsonConvert.DeserializeObject<CardInfo>(json.text)));
        this.deck  = this.allCards.allCardsList
            .Where(card=>card.IsInitial())
            .OrderByDescending(card=>Random.value)
            .Take(INITIAL_DECK_SIZE).ToList();
        Discard.Instance.discard = new List<CardDO>();
        this.addRelicCardIfThereIsnt();
        this.insertGods();
        shuffleDeck();
        // this.deck.Insert(0,allCards.allCardsList.Find(card=>card.GetNumber()==4));
        // this.deck.RemoveRange(0,3*deck.Count/4);
        this.deckSize = this.deck.Count;
        GameState.GameStart();
    }

    void shuffleDeck(){
        this.deck = this.deck.OrderBy(x => Random.value).ToList();
    }

    public void SpawnACard(){
        isSpawning = true;
        GameAudio.Instance.playNextOnQueue();
        StartCoroutine(MoveDeckAndSpawn());

    }

    public IEnumerator MoveTo(Vector3 to){
        while(transform.position!=to){
            transform.position = Vector3.MoveTowards(transform.position, to,  Time.deltaTime*12f);
            yield return null;
        }
    }

    public IEnumerator Foresight(bool discard){
        performingForesight = true;
        Table.Instance.Darken();
        Vector3 inScene = transform.position + Vector3.left*(spriteRenderer.bounds.size.x*1.05f);
        yield return MoveTo(inScene);
        yield return ShowForesightCards(discard);


    }

    public IEnumerator ShowForesightCards(bool candiscard){
        if(deck.Count<3){ 
            yield return shuffleDiscard();
            UpdateDeckSprite();
        }

        for(int j = 0;j<3 &&j<deck.Count;j++){
            foresightCards.Add(Instantiate(cardPrefab,transform.position,transform.rotation));
            foresightCards[j].changeSprite(deck[j].GetNumber().ToString());
            yield return foresightCards[j].MoveTo(transform.position+(Vector3.left*spriteRenderer.bounds.size.x*(j+3)));
        }
        if(!candiscard){
            forsightMouseClick.performed += WaitForClick;
        }
        else{
            forsightMouseClick.performed += WaitForClickOnCard;
        }
        forsightMouseClick.Enable();

    }

    private void WaitForClickOnCard(InputAction.CallbackContext context){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(ray);
        int cardClickedIndex = -1;
        foreach(RaycastHit2D hit in rayHits){
            for(int j=0;j<foresightCards.Count;j++){
                if(foresightCards[j]==hit.collider.gameObject.GetComponent<Card>()){
                    cardClickedIndex = j;
                    break;
                }
            }
            if(cardClickedIndex>=0) break;
        }

        if(cardClickedIndex<0){
            forsightMouseClick.performed -= WaitForClickOnCard;
            forsightMouseClick.Disable();
            StartCoroutine(RemoveForesightCards());
        }
        else{
            foresightCards.ForEach(card=>{
                if(card.isGreyed && foresightCards[cardClickedIndex]!=card)
                    card.SwitchGreyness();
            });
            foresightCards[cardClickedIndex].SwitchGreyness();
        }
    }

    private void WaitForClick(InputAction.CallbackContext context){
        forsightMouseClick.performed -= WaitForClick;
        forsightMouseClick.Disable();
        StartCoroutine(RemoveForesightCards());
    }

    public IEnumerator RemoveForesightCards(){
        for(int j = Mathf.Min(deck.Count,3)-1;j>=0;j--){
            Card cardToRemove = foresightCards[j];
            if(!foresightCards[j].isGreyed)    yield return cardToRemove.MoveTo(transform.position);
            else{
                yield return cardToRemove.RotateOutOfScreen();
                deck.RemoveAt(j);
            }
            GameObject.Destroy(cardToRemove.gameObject);
        }
        foresightCards.RemoveAll(card=>true);
        Vector3 outScene = transform.position + Vector3.right*(spriteRenderer.bounds.size.x*1.05f);
        yield return MoveTo(outScene);
        Table.Instance.LightUp();
        performingForesight = false;

    }

    public IEnumerator MoveDeckAndSpawn(){
        Vector3 originalPosition = transform.position;
        Vector3 inScene = transform.position + Vector3.left*(spriteRenderer.bounds.size.x*1.2f);
        if(this.deck.Count==0) yield return shuffleDiscard();
        yield return MoveTo(inScene);
        
        theCard = Instantiate(cardPrefab,transform.position,transform.rotation);
        nextCard();

        yield return MoveTo(originalPosition);
        isSpawning = false;
     }


    void nextCard(){
            CardDO current_card= GetAlertCard();
            if(current_card==null){
                current_card = RemoveTopCard();
            }
            theCard.currentCardDO = current_card;
            StartCoroutine(theCard.FlipAndMoveUp());
    }

    CardDO GetAlertCard(){
        if(Hand.Instance.MoreThan15Cards.isOn){
            if( Random.value>=Hand.GREED_CHANCE){
                return GetCard("Greed");
            }
        }
        else if(Hand.Instance.FiveOrMoreSuspision.isOn){
            if(Random.value>=Hand.POLICE_RAID_CHANCE){
                return GetCard("Police Raid");
            }
        }
        else if(Hand.Instance.NoFood.isOn){
            if(Random.value>=Hand.DESPARATE_MEASURES_CHANCE){
                return GetCard("Desperate Measures");
            }
        }
        return null;
    }

    IEnumerator shuffleDiscard(){
        GameAudio.Instance.PlayTrack(shuffleSound);
        shuffleText.Show();
        yield return new WaitForSeconds(shuffleSound.length-0.2f);
        shuffleText.Hide();
        Discard.Instance.shuffleDiscard();
        this.deck.AddRange(Discard.Instance.discard);
        Discard.Instance.discard.RemoveAll(card=>true);
    }

    CardDO RemoveTopCard(){
        CardDO current_card = this.deck[0];
        this.deck.RemoveAt(0);
        UpdateDeckSprite();
        return current_card;
    }

    void UpdateDeckSprite(){
        float leftOfDeck = ((float)(this.deck.Count))/this.deckSize;
        if(leftOfDeck>1) leftOfDeck = 1;
        int spriteIndex = (int)((1-leftOfDeck)*(spriteArray.Length-1));
        spriteRenderer.sprite = spriteArray[spriteIndex];

    }

    void insertGods(){
        this.godsInfo = new Gods(godsJSON);
        foreach(GodDO god in godsInfo.getUnlockedGods()){
            if (god.specialRequirements==0){
                int startingCard = god.startingCard;
                CardDO staringCardDO = this.allCards.allCardsList.Find(card=>card.GetNumber()==startingCard);
                if (!staringCardDO.IsTutorial()){
                    this.deck.Add(staringCardDO);
                }
                if(god.defeated==1) this.deck.AddRange(god.blessings.Select(card_num=>this.allCards.allCardsList.Find(card=>card.GetNumber()==card_num)));
            }
        }
    }
        

    List<CardDO> init_cards(){
        return this.allCards.allCardsList.Where(card=> card.IsInitial()).ToList();
    }

    void addRelicCardIfThereIsnt(){
        if (! this.relicCardInDeck()){
            List<CardDO> relicCards = this.potentialRelicCards(this.init_cards().Select(init_card=>this.allCards.allCardsList.Find(c=>c.GetNumber() == init_card.GetNumber())).ToList());
            CardDO randomRelicCard = relicCards[randomNumber(0,relicCards.Count)];
            int replace_with = randomNumber(0,this.deck.Count);
            this.deck[replace_with] = randomRelicCard;
        }
    }

    int randomNumber(int includeStart,int excludeFinish){
        int range = excludeFinish-includeStart;
        float randFloat;
        do{
            randFloat = Random.value;
        }while(randFloat==1);
        int res = (Mathf.FloorToInt( randFloat*range)+includeStart);
        return res;
    }

    bool relicCardInDeck(){
        HashSet<int> potentialRelicCards = new HashSet<int>(this.potentialRelicCards(this.allCards.allCardsList).Select(card=>card.GetNumber()));
        foreach(CardDO card in this.deck){
            if (potentialRelicCards.Contains(card.GetNumber())){
                return true;
            }
        }
        return false;
    }

    List<CardDO> potentialRelicCards(List<CardDO> cards){
        if (cards==null)
            cards = this.allCards.allCardsList;
        return cards.Where(card=> this.potentialRelicCard(card)).ToList();
    }

    bool potentialRelicCard(CardDO card){
        HashSet<CardDO> visited = new HashSet<CardDO>();
        Queue<CardDO> toVisit = new Queue<CardDO>();
        toVisit.Enqueue(card);
        while (toVisit.Count>0){
            CardDO currentCard = toVisit.Dequeue();
            visited.Add(currentCard);
            if(canGiveWildCard(currentCard)){
                return true;
            }
            else{
                List<int> currentCardPossibleCards = this.possibleShuffleCardsNumbers(currentCard);
                foreach(int possibleCardNumber in currentCardPossibleCards){
                    CardDO possibleCard = allCards.allCardsList.Find(one_card=>one_card.GetNumber() == possibleCardNumber);
                    if (! visited.Contains(possibleCard)){
                        visited.Add(possibleCard);
                        toVisit.Enqueue(possibleCard);
                    }
                }
            }
        }
        return false;
    }

    List<int> possibleShuffleCardsNumbers(CardDO card){
        return possibleShuffleCards(card.GetOption_1().shuffle)
            .Concat(possibleShuffleCards(card.GetOption_2().shuffle)).
            Concat(possibleShuffleCards(card.GetOption_3().shuffle)).ToList();

    }

    List<int> possibleShuffleCards(ShuffleDO shuffle){
        List<int> possible_cards= new List<int>();
        if (shuffle.lowerbound != 999 && shuffle.upperbound != 999){
            possible_cards.AddRange(Enumerable.Range(shuffle.lowerbound,shuffle.upperbound+1-shuffle.lowerbound));
        }
        possible_cards.AddRange(shuffle.specificids);
        return possible_cards;

    }

    bool canGiveWildCard(CardDO card){
        return card.GetOption_1().GetRewards().CanGiveWildCard()
            || card.GetOption_2().GetRewards().CanGiveWildCard()
            || card.GetOption_3().GetRewards().CanGiveWildCard();
    }

    // Update is called once per frame
    void Update()
    {
        if(theCard==null && !isSpawning && GameState.state==State.Ongoing){
            SpawnACard();
        }
    }
}

public class AllCards
{
    public List<CardDO> allCardsList;

    public AllCards(IEnumerable<CardDO> cardDOs){
        this.allCardsList = cardDOs.ToList();

    }

}

