using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using System.Linq;

public class Card : MonoBehaviour
{
    public bool isGreyed = false;
    public bool foresight = false;
    public SpriteRenderer spriteRenderer;
    private static IList<Sprite> spriteList;
    public CardDO currentCardDO;
    private Vector3 moveTo;
    private Vector3 rotateOn;
    private static Vector3 zero = Vector3.up*1000000;
    List<System.Action> performWhenListLoaded = new List<System.Action>();
    public GameObject optionPrefab;
    private List<GameObject> options = new List<GameObject>();

    private Camera mainCamera;

    private System.Action moveCallback;

    private void Awake(){
        mainCamera = Camera.main;
    }
    private bool disableCard = true;
    private bool movedBack = false;

    public bool faceUp = false;

    public float moveSpeed = 10f;
    
    void Start()
    {
        if(spriteList==null ||spriteList.Count==0){
            Addressables.LoadAssetsAsync<Sprite>("CardsImages", null).Completed += (handleToCheck)=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded)
                {
                    spriteList = handleToCheck.Result;
                    while(performWhenListLoaded.Count>0){
                        performWhenListLoaded[0]();
                        performWhenListLoaded.RemoveAt(0);
                    }
                }
            };
        }
        moveTo =zero;
        rotateOn =zero;
        if(!foresight)  StartCoroutine(CheckOptions());
    }

     public IEnumerator CheckOptions(){
         if(options.Any(option=>option.GetComponent<Option>().LosingOption())){
            while(!options.All(option=>option.GetComponent<Option>().isDormant)){
                yield return null;
            }
            options.Select(option=>option.GetComponent<Option>()).First(option=>option.LosingOption()).isDormant = false;
         }
     }

    public void ShowOptions(InputAction.CallbackContext context){
        if(this.foresight || this.disableCard || ! context.performed || !CardClicked()) return ;
        DisbleCard();
        CreateOptions();
        MoveUp(MoveOptions);
    }

    void MoveOptions(){
        this.options.ForEach(op=>op.GetComponent<Option>().MoveOption(()=>{
            this.MoveBack(()=>{
                while(options.Count>0){
                    GameObject op = options[0];
                    options.RemoveAt(0);
                    GameObject.Destroy(op);
                }
                if(currentCardDO.isrecurring==0) rotateOut();
                else Discard.Instance.MoveIn();
            });
        }));
    }


    bool CardClicked(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        return rayHit.collider!=null && rayHit.collider.gameObject.CompareTag("Card");
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
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        spriteRenderer.sortingLayerName = "Card";


        UpdateOptions();
    }

    private void UpdateOptions(){
        options[0].GetComponent<Option>().UpdateDO(this.currentCardDO.option1,2);
        options[1].GetComponent<Option>().UpdateDO(this.currentCardDO.option2,1);
        options[2].GetComponent<Option>().UpdateDO(this.currentCardDO.option3,3);
    }

    private void DisbleCard(){
        disableCard = true;
    }

    private void EnableCard(){
        disableCard = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!moveTo.Equals( zero)){
            transform.position = Vector3.MoveTowards(transform.position, moveTo,  Time.deltaTime*moveSpeed);
            if(transform.position.Equals(moveTo)){
                moveTo = zero;
                if(moveCallback!=null){
                    moveCallback();
                    moveCallback = null;
                }
                if(movedBack){
                    Discard.Instance.cardToDicard = this;                
                    Discard.Instance.MoveOut();
                }
            }
        }
        if(!rotateOn.Equals(zero)){
            transform.RotateAround(rotateOn, Vector3.back, 135 * Time.deltaTime);
            Vector3 outOfScreenVector = Camera.main.WorldToViewportPoint(transform.position);
            if(outOfScreenVector.y>1.5){
                GameObject.Destroy(gameObject);
            }
        }
        
    }
    public IEnumerator MoveTo(Vector3 to){
        while(transform.position!=to){
            transform.position = Vector3.MoveTowards(transform.position, to,  Time.deltaTime*30);
            yield return null;
        }
    }

    public IEnumerator RotateOutOfScreen(){
        Vector3 on = transform.position+Vector3.up *10;
        Vector3 outOfScreenVector;
        do{
            transform.RotateAround(on, Vector3.back, 135 * Time.deltaTime);
            outOfScreenVector = Camera.main.WorldToViewportPoint(transform.position);
            yield return null;
        }while(outOfScreenVector.y<=1.5);
    }

    
    public void MoveUp(System.Action callback){
        this.moveCallback = callback;
        moveTo = transform.position+Vector3.up *8;
    }

    public void FinishingMove(){
        moveSpeed = 30f;
        moveTo = Discard.Instance.transform.position;
    }

    public void MoveBack(System.Action callback){
        this.moveCallback = callback;
        moveTo = transform.position+Vector3.down *8;
        movedBack = true;
    }

    public void rotateOut(){
        this.rotateOn = transform.position+Vector3.up *10;
    }

    public void changeSprite(string suffix){
        spriteRenderer.sprite = spriteList.First(sprite=>sprite.name.Equals("Card"+suffix));
    }

    public IEnumerator FlipCard(){
        faceUp = false;
        
        Vector3 startingPosition =transform.position;
        while(currentCardDO==null){
            yield return null;
        }
        if(!faceUp)
        {
            for(float i = 0f;i<=360f;i+=15f)
            {
                transform.rotation = Quaternion.Euler(0f,i,0f);
                
                transform.position =startingPosition+ Vector3.left*((spriteRenderer.bounds.size.x)*((i<180f? i : i-180f)/180f));
                if(i==90f){
                    string cardSuffix = spriteRenderer.sprite.name.Contains("back")?  this.currentCardDO.num.ToString(): "back";
                    if(spriteList==null) performWhenListLoaded.Add(()=>changeSprite(cardSuffix));
                    else changeSprite(cardSuffix);
                    i+=180f;
                }
                yield return new WaitForSeconds(0.01f);
            }
            
        }
        faceUp= !faceUp;

        EnableCard();
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
