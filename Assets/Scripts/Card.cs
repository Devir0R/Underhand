using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class Card : MonoBehaviour
{
    public bool isGreyed = false;
    public SpriteRenderer spriteRenderer;
    private static IList<Sprite> spriteList;
    public CardDO currentCardDO;
    public Option optionPrefab;
    private List<Option> options = new List<Option>();

    private Camera mainCamera;


    private void Awake(){
        mainCamera = Camera.main;
    }

    public bool faceUp = false;

    public float moveSpeed = 10f;

    public AudioClip flipSound;

    
    void Start()
    {
        if(spriteList==null ||spriteList.Count==0)  spriteList = Loader.cardsSprites;
        optionPrefab.transform.localScale = transform.localScale;
        StartCoroutine(CheckOptions());
    }

     public IEnumerator CheckOptions(){
         if(options.Any(option=>option.GetComponent<Option>().LosingOption())){
            while(!options.All(option=>option.GetComponent<Option>().isDormant)){
                yield return null;
            }
            options.Select(option=>option.GetComponent<Option>()).First(option=>option.LosingOption()).isDormant = false;
         }
     }

    IEnumerator MoveCardUp(){
        yield return MoveTo(transform.position+Vector3.up *spriteRenderer.bounds.size.y,10f);
        MoveOptions();
        options.ForEach(op=>op.GetComponent<Option>().AddOnChooseListeners());
    }

    void MoveOptions(){
        options.ForEach(op=>op.GetComponent<Option>().MoveOption(()=>StartCoroutine(FinishingMove())));
    }


    public IEnumerator FadeIn(){
        Color cardColor = spriteRenderer.material.color;
        float initialTransprancy = cardColor.a;
        float numberOfFrames = 10f;
        float unit = (initialTransprancy/Time.deltaTime)/numberOfFrames;
        float fadeAmount = 0;
        do{
            spriteRenderer.material.color = new Color(cardColor.r,cardColor.b,cardColor.g,fadeAmount*Time.deltaTime*unit);
            fadeAmount+=1;
            yield return new WaitForSeconds(0.05f);
        }while(fadeAmount<=numberOfFrames);
    }

    public void SwitchGreyness(){
        isGreyed = !isGreyed;
        spriteRenderer.material.color = isGreyed? Color.grey : Color.white;
    }

    private void CreateOptions(){
        optionPrefab.disableOption = true;
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        spriteRenderer.sortingLayerName = "Card";


        UpdateOptions();
    }

    private void UpdateOptions(){
        options[0].UpdateDO(this.currentCardDO.option1,1);
        options[1].UpdateDO(this.currentCardDO.option2,3);
        options[2].UpdateDO(this.currentCardDO.option3,2);
        if(options.All(op=>op.isDormant)){
            options.Where(op=>op.LosingOption()).ToList().ForEach(op=>op.WakeUp());
        }
    }

    public IEnumerator MoveTo(Vector3 to,float speed = 30f){
        while(transform.position!=to){
            transform.position = Vector3.MoveTowards(transform.position, to,  Time.deltaTime*speed);
            yield return null;
        }
    }

    public IEnumerator MoveToAndDestroy(Vector3 to,float speed = 30f){
        yield return MoveTo(to,speed);
        GameObject.Destroy(gameObject);
    }

    public IEnumerator RotateOutOfScreen(){
        GameAudio.Instance.PlayTrackSlide();
        Vector3 on = Discard.Instance.transform.position+Vector3.up *(spriteRenderer.bounds.size.y)*2f;
        Vector3 outOfScreenVector;
        do{
            transform.position = Vector3.MoveTowards(transform.position, on, 25 * Time.deltaTime);
            transform.RotateAround(transform.position, Vector3.forward, 65f * Time.deltaTime);
            outOfScreenVector = Camera.main.WorldToViewportPoint(transform.position);
            yield return null;
        }while(on!=transform.position);
    }

    public IEnumerator FinishingMove(){
        yield return MoveTo(transform.position+Vector3.down *spriteRenderer.bounds.size.y,10f);
        while(options.Count>0){
            Option op = options[0];
            options.RemoveAt(0);
            GameObject.Destroy(op.gameObject);
        }
        if(currentCardDO.isrecurring==0) {
            if(Deck.Instance.triggerInsertCardAnimation){
                yield return Discard.Instance.MoveIn();
                yield return Deck.Instance.CheckIfInsertCard(this);
            }

            yield return RotateOutOfScreen();

            if(Deck.Instance.triggerInsertCardAnimation) yield return Discard.Instance.MoveOut();
            GameObject.Destroy(gameObject);
        }
        else{
            yield return Discard.Instance.MoveIn();
            Discard.Instance.cardToDicard = this;                
            StartCoroutine(Discard.Instance.MoveDiscardOut());
        }
    }

    public void changeSprite(string suffix){
        spriteRenderer.sprite = spriteList.First(sprite=>sprite.name.Equals("Card"+suffix));
    }

    public IEnumerator FlipAndMoveUp(){
        yield return FlipCard();
        CreateOptions();
        yield return new WaitForSeconds(0.5f);
        yield return MoveCardUp();
        options.ForEach(op=>op.disableOption = false);
    }

    public IEnumerator FlipCard(){
        faceUp = false;
        
        Vector3 startingPosition =transform.position;
        while(currentCardDO==null){
            yield return null;
        }
        GameAudio.Instance.PlayTrack(flipSound);
        if(!faceUp)
        {
            for(float i = 0f;i<=360f;i+=15f)
            {
                transform.rotation = Quaternion.Euler(0f,i,0f);
                
                transform.position =startingPosition+ Vector3.left*((spriteRenderer.bounds.size.x)*((i<180f? i : i-180f)/180f));
                if(i==90f){
                    string cardSuffix = spriteRenderer.sprite.name.Contains("back")?  this.currentCardDO.num.ToString(): "back";
                    changeSprite(cardSuffix);
                    i+=180f;
                }
                yield return new WaitForSeconds(0.015f);
            }
            
        }
        faceUp= !faceUp;

    }
}


[System.Serializable]
public class CardDO{
        public string flavortext;
        public string title;
        public int isrecurring;
        public int isinitial;
        public int isTutorial;
        public OptionDO option1;
        public OptionDO option2;
        public OptionDO option3;
        public int num ;
        public int animationframes;
        public int weight;
        public int cardartdone;

}
