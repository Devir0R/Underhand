using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

public class Card : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private static IList<Sprite> spriteList;
    private CardDO currentCardDO;
    private int currentImageIndex = 0;
    private Vector3 moveTo;
    private Vector3 rotateOn;
    private static Vector3 zero = Vector3.up*1000000;
    List<System.Action> performWhenListLoaded = new List<System.Action>();
    private bool disableCard;
    public GameObject optionPrefab;
    private List<GameObject> options = new List<GameObject>();

    private Camera mainCamera;

    private System.Action moveCallback;

    private void Awake(){
        mainCamera = Camera.main;
    }

    private bool movedBack = false;
    
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
    }

    public void ShowOptions(InputAction.CallbackContext context){
        if(this.disableCard || ! context.performed || !CardClicked()) return ;
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
            });
        }));
    }
    bool CardClicked(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        return rayHit.collider!=null && rayHit.collider.gameObject.CompareTag("Card");
    }

    private void CreateOptions(){
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        options.Add(Instantiate(optionPrefab,transform.position,transform.rotation));
        spriteRenderer.sortingLayerName = "top";


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

    // Update is called once per frame
    void Update()
    {
        if(!moveTo.Equals( zero)){
            transform.position = Vector3.MoveTowards(transform.position, moveTo,  Time.deltaTime*10);
            if(transform.position.Equals(moveTo)){
                moveTo = zero;
                if(moveCallback!=null){
                    moveCallback();
                    moveCallback = null;
                }
                if(movedBack){
                    rotateOut();
                }
            }
        }
        if(!rotateOn.Equals(zero)){
            transform.RotateAround(rotateOn, Vector3.back, 135 * Time.deltaTime);
            Vector3 outOfScreenVector = Camera.main.WorldToViewportPoint(transform.position);
            if(outOfScreenVector.y>1.5){
                GameObject.Destroy(gameObject);
                GameObject.FindGameObjectWithTag("Deck").GetComponent<Deck>().EnableDeck();
            }
        }
    }

    public void MoveUp(System.Action callback){
        this.moveCallback = callback;
        moveTo = transform.position+Vector3.up *8;
    }

    public void MoveBack(System.Action callback){
        this.moveCallback = callback;
        moveTo = transform.position+Vector3.down *8;
        movedBack = true;
    }

    public void rotateOut(){
        this.rotateOn = transform.position+Vector3.up *10;
    }

    public void changeCard(CardDO cardDO){
        System.Action changeSprite = ()=>{
            currentCardDO = cardDO;
            for(int i =0;i<spriteList.Count;i++){
                Sprite sprite = spriteList[i];
                if(sprite.name.Equals("Card"+currentCardDO.num)){
                    currentImageIndex = i;
                    spriteRenderer.sprite = spriteList[currentImageIndex]; 
                }
            }
        };
        if(spriteList==null){
            performWhenListLoaded.Add(changeSprite);
        }
        else{
            changeSprite();
        }
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
