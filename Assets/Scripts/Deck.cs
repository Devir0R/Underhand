using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;


public class Deck : MonoBehaviour
{
    public static  int INITIAL_DECK_SIZE=7;
    public SpriteRenderer spriteRenderer;

    public TextAsset godsJSON;
    AllCards allCards;
    public List<CardDO>  deck;
    private bool disableDeck;

    public List<CardDO> discard;
    public Sprite[] spriteArray;
    public GameObject cardPrefab;
    
    private int deckSize;
    Card theCard;
    public Gods godsInfo;

    private Camera mainCamera;

    private static Deck _instance;
    public static Deck Instance{ get { return _instance; } }
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
        return allCards.allCardsList.Find(card=>card.title==cardName);
    }

    
    
    public void AddToDiscard(ShuffleDO shuffle){
        List<CardDO> cardsToAdd = new List<CardDO>();
        if(shuffle.lowerbound!=999&&shuffle.upperbound!=999){
            for(int j=0;j<shuffle.numcards;j++){
                CardDO cardToAdd;
                int randomCardNum = randomNumber(shuffle.lowerbound,shuffle.upperbound+1);
                cardToAdd = allCards.allCardsList.Find(card=>card.num==randomCardNum);
                if(shuffle.allowsdupes==1 || !cardsToAdd.Contains(cardToAdd)){
                    cardsToAdd.Add(cardToAdd);
                }
                else j--;
            }
        }
        cardsToAdd.AddRange(shuffle.specificids.Select(id=>allCards.allCardsList.Find(card=>card.num==id)));
        
        this.discard.AddRange(cardsToAdd);
        

    }

    public CardDO GetTopCard(){
        return Deck.Instance.deck[0];
    }
    // Start is called before the first frame update
    void Start()
    {
        Addressables.LoadAssetsAsync<TextAsset>("CardsJsons", null).Completed += (handleToCheck)=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)
            {
                this.allCards = new AllCards(handleToCheck.Result.Select(json=>JsonConvert.DeserializeObject<CardDO>(json.text)));
                this.deck  = this.allCards.allCardsList
                    .Where(card=>card.isinitial==1)
                    .OrderByDescending(card=>Random.value)
                    .Take(INITIAL_DECK_SIZE).ToList();
                this.discard = new List<CardDO>();
                this.addRelicCardIfThereIsnt();
                this.insertGods();
                shuffleDeck();
                this.deckSize = this.deck.Count;
                this.disableDeck = false;
                GameState.GameStart();
                
            }
        };
    }

    void shuffleDeck(){
        this.deck = this.deck.OrderBy(x => Random.value).ToList();
    }

    public void SpawnACardClicked(InputAction.CallbackContext context){
        if(this.disableDeck || ! context.performed || !DeckClicked()) return ;

        DisableDeck();
        Vector3 cardPosition = new Vector3(-transform.position.x+3,transform.position.y,transform.position.z);
        if(theCard==null){
            theCard = Instantiate(cardPrefab,cardPosition,transform.rotation).GetComponent<Card>();
            nextCard();
        }
    }

    bool DeckClicked(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        return rayHit.collider!=null && rayHit.collider.gameObject.CompareTag("Deck");
    }

    void DisableDeck(){
        this.disableDeck = true;
    }

    
    public void EnableDeck(){
        this.disableDeck = false;
    }

    void nextCard(){
            CardDO current_card= GetAlertCard();
            if(current_card==null){
                current_card = RemoveTopCard();
            }
            

            theCard.changeCard(current_card);
            

            if(current_card.isrecurring==1) this.discard.Add(current_card);
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

    CardDO RemoveTopCard(){
        if(this.deck.Count==0){
            this.deck = this.discard.ToList();
            this.discard = new List<CardDO>();
            shuffleDeck();
        }
        CardDO current_card = this.deck[0];
        this.deck.RemoveAt(0);
        UpdateDeckSprite();
        return current_card;
    }

    void UpdateDeckSprite(){
        float leftOfDeck = ((float)(this.deck.Count))/this.deckSize;
        int spriteIndex = (int)((1-leftOfDeck)*(spriteArray.Length-1));
        spriteRenderer.sprite = spriteArray[spriteIndex];

    }

    void insertGods(){
        this.godsInfo = new Gods(godsJSON);
        foreach(GodDO god in godsInfo.getUnlockedGods()){
            if (god.specialRequirements==0){
                int startingCard = god.startingCard;
                CardDO staringCardDO = this.allCards.allCardsList.Find(card=>card.num==startingCard);
                if (staringCardDO.isTutorial==0){
                    this.deck.Add(staringCardDO);
                }
                this.deck.AddRange(god.blessings.Select(card_num=>this.allCards.allCardsList.Find(card=>card.num==card_num)));
            }
        }
    }
        

    List<CardDO> init_cards(){
        return this.allCards.allCardsList.Where(card=> card.isinitial!=0).ToList();
    }

    void addRelicCardIfThereIsnt(){
        if (! this.relicCardInDeck()){
            List<CardDO> relicCards = this.potentialRelicCards(this.init_cards().Select(init_card=>this.allCards.allCardsList.Find(c=>c.num == init_card.num)).ToList());
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
        HashSet<int> potentialRelicCards = new HashSet<int>(this.potentialRelicCards(this.allCards.allCardsList).Select(card=>card.num));
        foreach(CardDO card in this.deck){
            if (potentialRelicCards.Contains(card.num)){
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
            if(canGiveRelic(currentCard)){
                return true;
            }
            else{
                List<int> currentCardPossibleCards = this.possibleShuffleCardsNumbers(currentCard);
                foreach(int possibleCardNumber in currentCardPossibleCards){
                    CardDO possibleCard = allCards.allCardsList.Find(one_card=>one_card.num == possibleCardNumber);
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
        return possibleShuffleCards(card.option1.shuffle)
            .Concat(possibleShuffleCards(card.option1.shuffle)).
            Concat(possibleShuffleCards(card.option1.shuffle)).ToList();

    }

    List<int> possibleShuffleCards(ShuffleDO shuffle){
        List<int> possible_cards= new List<int>();
        if (shuffle.lowerbound != 999 && shuffle.upperbound != 999){
            possible_cards.AddRange(Enumerable.Range(shuffle.lowerbound,shuffle.upperbound+1-shuffle.lowerbound));
        }
        possible_cards.AddRange(shuffle.specificids);
        return possible_cards;

    }

    bool canGiveRelic(CardDO card){
        return card.option1.rewards.relic>0
            || card.option2.rewards.relic>0
            || card.option3.rewards.relic>0;
    }

    // Update is called once per frame
    void Update()
    {
    }
}

public class AllCards
{
    public List<CardDO> allCardsList;

    public AllCards(IEnumerable<CardDO> cardDOs){
        this.allCardsList = cardDOs.ToList();

    }

}

