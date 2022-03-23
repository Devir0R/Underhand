using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using System.Linq;

public class Card : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private static IList<Sprite> spriteList;
    private CardDO currentCardDO;
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

    public IEnumerator FlipCard(CardDO cardDO){
        
        System.Action changeSprite = ()=>{
            currentCardDO = cardDO;
            spriteRenderer.sprite = spriteList.First(sprite=>sprite.name.Equals("Card"+currentCardDO.num));
        };
        Vector3 pivot = transform.position + (Vector3.left*(spriteRenderer.bounds.size.x/2f));
        bool flipped = false;

        while(!flipped){
            transform.RotateAround(pivot, Vector3.up, -150 * Time.deltaTime);
            if(-transform.rotation.y>transform.rotation.w){
                flipped = true;
            }
            yield return null;
        }

        if(spriteList==null) performWhenListLoaded.Add(changeSprite);
        else changeSprite();

        transform.rotation = new Quaternion(transform.rotation.x,-transform.rotation.y,transform.rotation.z,transform.rotation.w);
        flipped = false;
        while(!flipped){
            float w_before = transform.rotation.w;
            transform.RotateAround(pivot, Vector3.up, -150 * Time.deltaTime);
            float w_after = transform.rotation.w;
            if(w_before>w_after){
                flipped = true;
            }
            yield return null;
        }
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
