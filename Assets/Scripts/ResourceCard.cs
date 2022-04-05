using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;

public class ResourceCard : MonoBehaviour
{
    
    private static int TableRatio = 1;
    private static float HandRatio = 1.5f;
    private static int DragRatio = 2;

    private bool sacrificeInitiated = false;
    private bool rewardInitiated = false;

    private int currentAnimationIndex = 0;
    private static readonly string NormalCardString = "_9";

    public SpriteRenderer spriteRenderer;

    int frameCount = 0;
    public TextMeshPro many;


    public GameObject resourceCircle;
    public List<Sprite> sprites;

    public Resource resourceType = Resource.None;

    private bool changedColor;

    [SerializeField]
    public InputAction mouseClickedOnCard;

    private Camera mainCamera;

    private float mouseDragSpeed = 0.1f;
    private Vector3 velocity = Vector3.zero;
    public static bool dragging = false;

    
    Vector3 OnTanbleSize;

    public int howMany = 0;

    public delegate void NotifyCardOnTableDestroyed();
    public static event NotifyCardOnTableDestroyed CardOnTableDestroyed;

    public int AnimationSpeed;

    public AudioClip DropClip;

    bool disbled = false;
    protected virtual void OnCardOnTableDestroyed() //protected virtual method
    {
        //if ProcessCompleted is not null then call delegate
        CardOnTableDestroyed?.Invoke(); 
    }

    private void OnEnable(){
        mouseClickedOnCard.Enable();
        mouseClickedOnCard.performed += DragCard;
    }

    private void OnDisable(){
        mouseClickedOnCard.performed -= DragCard;
        mouseClickedOnCard.Disable();
    }

    private void Awake(){
        AnimationSpeed =  Mathf.RoundToInt((1.0f/Time.deltaTime)/12f);
        mainCamera = Camera.main;
    }
    public void DragCard(InputAction.CallbackContext context){
        if(disbled) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(ray);
        IEnumerable<int?> sortings = 
            rayHits.Select(rayHit=>rayHit.collider?.gameObject?.GetComponent<ResourceCard>()?.spriteRenderer?.sortingOrder);
        if(sortings.Max()!=spriteRenderer.sortingOrder) return ;
        spriteRenderer.sortingOrder = Hand.Instance.hand.Count+1;
        dragging = true;
        Hand.Instance.RemoveCardFromHand(this);
        transform.rotation = Quaternion.identity;
        StartCoroutine(DragUpdate(this));
    }

    public IEnumerator DragUpdate(ResourceCard draggedCard){
        float initialDistance = Vector3.Distance(draggedCard.transform.position,mainCamera.transform.position);
        while(mouseClickedOnCard.ReadValue<float>()!=0){
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            draggedCard.transform.position = Vector3.SmoothDamp(draggedCard.transform.position,ray.GetPoint(initialDistance),
                    ref velocity,mouseDragSpeed);
            yield return null;
        }

        Ray rayToTable = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(rayToTable);
        Collider2D table = rayHits.Select(hit=>hit.collider).FirstOrDefault(collider=>collider.gameObject.CompareTag("Table"));
        if(table!=null){
            GameAudio.Instance.PlayTrack(DropClip);
            table.GetComponent<Table>().AddResource(resourceType);
            GameObject.Destroy(gameObject);

        }
        else{
            Hand.Instance.AddCardToHand(this);
        }
        dragging = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(resourceType!=Resource.None){
            sprites = Loader.resourcesSpritesDictionary[resourceType].ToList();
            spriteRenderer.sprite = sprites.Find(sprite=>sprite.name.Contains("_9"));
            if(resourceCircle!=null)
                resourceCircle.GetComponent<Renderer>().material.color = ResourceInfo.Info[resourceType].circleColor;        
        }
        OnTanbleSize = transform.localScale;
    }

    public void addOneOfResource(){
        addNOfResource(1);
    }

    public void addNOfResource(int n){
        howMany+=n;
        many.text = ""+howMany;
    }

    public void removeOneOfResource(){
        removeNOfResource(1);
    }

    public void removeNOfResource(int n){
        howMany-=n;
        many.text = ""+howMany;
    }

    void UpdateResouceType(Resource resource){
        resourceType = resource;
    }

    public void OnTableSize(){
        transform.localScale = new Vector3(OnTanbleSize.x*TableRatio,OnTanbleSize.y*TableRatio,OnTanbleSize.z);
    }
    public void InHandSize(){
        transform.localScale = new Vector3(OnTanbleSize.x*HandRatio,OnTanbleSize.y*HandRatio,OnTanbleSize.z);
    }

    public void DraggedSize(){
        transform.localScale = new Vector3(OnTanbleSize.x*DragRatio,OnTanbleSize.y*DragRatio,OnTanbleSize.z);
    }

    // Update is called once per frame
    void Update()
    {
        
        frameCount = (frameCount+1)%AnimationSpeed; 
        if(rewardInitiated && frameCount==AnimationSpeed-1){
            spriteRenderer.sprite = sprites[currentAnimationIndex];
            if(sprites[currentAnimationIndex].name.Contains(NormalCardString)){
                rewardInitiated = false;
                StartCoroutine(JumpToHand());
            }
            currentAnimationIndex+=1;
        }
        else if(sacrificeInitiated && frameCount==AnimationSpeed-1){
            if(sprites.Count<=currentAnimationIndex){
                OnCardOnTableDestroyed();
                GameObject.Destroy(gameObject);
            }
            else{
                spriteRenderer.sprite = sprites[currentAnimationIndex];
                currentAnimationIndex+=1;
            }
        }
        
    }

    public IEnumerator JumpToHand(){
        GameObject.Destroy(resourceCircle.gameObject);
        transform.localScale = transform.localScale * 1.4f;
        while(transform.position!=Hand.Instance.transform.position){
            transform.position = Vector3.MoveTowards(transform.position,Hand.Instance.transform.position,Time.deltaTime*60f);
            yield return new WaitForSeconds(0.01f);
        }
        Hand.Instance.AddCardToHand(resourceType,howMany);
        Destroy(gameObject);
    }

    public void SacrificeCard(){
        currentAnimationIndex = 9;
        this.sacrificeInitiated = true;
    }

    public void RewardCard(){
        disbled = true;
        currentAnimationIndex = 0;
        spriteRenderer.sprite = sprites[0];
        this.rewardInitiated = true;

    }
}

class ResourceInfo{

    public ResourceInfo(Resource type,string imagesLabel,Color circleColor){
        this.type = type;
        this.imagesLabel=imagesLabel;
        this.circleColor = circleColor;
        this.resourceOnTable = null;
        this.resourcePositionOnTable = Vector3.zero;

    }

    public ResourceCard resourceOnTable;
    public Vector3 resourcePositionOnTable;
    public Resource type;
    public string imagesLabel;

    public Color circleColor;
    public static readonly Resource[] AllResources = new Resource[]{Resource.Food,Resource.Money,Resource.Cultist,Resource.Prisoner,Resource.Suspision,Resource.Relic};
    public static readonly Dictionary<Resource,ResourceInfo> Info =  
        new Dictionary<Resource, ResourceInfo>() {
            {
                Resource.Food, new ResourceInfo(Resource.Food,"FoodImages",new Color(126f/255,141f/255,66f/255))
            },
            {
                Resource.Money, new ResourceInfo(Resource.Money,"MoneyImages",new Color(210f/255,168f/255,67f/255))
            },
            {
                Resource.Cultist, new ResourceInfo(Resource.Cultist,"CultistImages", new Color(168f/255,122f/255,125f/255))
            },
            {
                Resource.Prisoner, new ResourceInfo(Resource.Prisoner,"PrisonerImages",new Color(156f/255,172f/255,157f/255))
            },
            {
                Resource.Relic, new ResourceInfo(Resource.Relic,"RelicImages",new Color(255f/255,255f/255,255f/255))
            },
            {
                Resource.Suspision, new ResourceInfo(Resource.Suspision,"SuspisionImages",new Color(181f/255,81f/255,51f/255))
            },
        };
}

