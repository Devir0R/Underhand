using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Table : MonoBehaviour
{
    public Vector3 MoneyPosition;

    public ResourceCard ResourceOnTablePrefab;

    public ResourceCard ResourcePrefab;

    public InputAction mouseClickedOnTableResource;
    Camera mainCamera;

    private Vector3 velocity = Vector3.zero;

    private static Table _instance;
    public static Table Instance{ get { return _instance; } }

    public delegate void NotifyResourcesOnTableChanged();
    public event NotifyResourcesOnTableChanged ResourcedOnTableChanged;

    public TableAnimation tableAnimation;

    int isLightUp = 0;

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
    private void OnEnable(){
        mouseClickedOnTableResource.Enable();
        mouseClickedOnTableResource.performed += DragCard;
    }

    public void Darken(){
        isLightUp +=1;
        tableAnimation.backgroundSpriteRenderer.material.color = Color.grey;
    }

    public void LightUp(){
        isLightUp-=1;
        if(isLightUp<=0)    tableAnimation.backgroundSpriteRenderer.material.color = Color.white;
    }

    private void OnDisable(){
        mouseClickedOnTableResource.performed -= DragCard;
        mouseClickedOnTableResource.Disable();
    }

    public List<Resource> ResourcesOnTable(){
        List<Resource> resourcesOnTable = new List<Resource>();
        System.Func<Resource,int,List<Resource>> nOfRsource = (res,num)=>{
            List<Resource> resources = new List<Resource>();
            for(int i =0;i<num;i++){
                resources.Add(res);
            }
            return resources;
        };
        foreach(Resource resource in ResourceInfo.GetAllResources()){
            if(ResourceInfo.Info[resource].resourceOnTable!=null){
                resourcesOnTable.AddRange(nOfRsource(resource,ResourceInfo.Info[resource].resourceOnTable.howMany));
            }            
        }
        return resourcesOnTable;
    }

    public void DragCard(InputAction.CallbackContext context){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D[] rayHits = Physics2D.GetRayIntersectionAll(ray);
        foreach(RaycastHit2D hit in rayHits){
            if(hit.collider.gameObject.CompareTag("CardOnTable")){
                ResourcePrefab.resourceType = hit.collider.GetComponent<ResourceCard>().resourceType;
                ResourceCard cardToDrag = Instantiate(ResourcePrefab,hit.collider.transform.position,hit.collider.transform.rotation);
                cardToDrag.spriteRenderer.sortingLayerName = "cardsInHand";
                cardToDrag.spriteRenderer.sortingOrder = Hand.Instance.hand.Count+1;
                RemoveResource(cardToDrag.resourceType);
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 size = ResourceOnTablePrefab.spriteRenderer.bounds.size;

        ResourceInfo.Info[Resource.Money].resourcePositionOnTable = MoneyPosition;
        ResourceInfo.Info[Resource.Prisoner].resourcePositionOnTable = new Vector3(MoneyPosition.x,MoneyPosition.y+(size.y*1.2f),transform.position.z);
        Vector3 PrisonerPosition = ResourceInfo.Info[Resource.Prisoner].resourcePositionOnTable;

        ResourceInfo.Info[Resource.Food].resourcePositionOnTable = new Vector3(PrisonerPosition.x-size.x,PrisonerPosition.y,transform.position.z);
        ResourceInfo.Info[Resource.Suspision].resourcePositionOnTable = new Vector3(PrisonerPosition.x+size.x,PrisonerPosition.y,transform.position.z);
        ResourceInfo.Info[Resource.Cultist].resourcePositionOnTable = new Vector3(MoneyPosition.x+size.x,MoneyPosition.y,transform.position.z);
        ResourceInfo.Info[Resource.Relic].resourcePositionOnTable = new Vector3(MoneyPosition.x-size.x,MoneyPosition.y,transform.position.z);

    }

    protected virtual void OnResourcedOnTableChanged() //protected virtual method
    {
        //if ProcessCompleted is not null then call delegate
        ResourcedOnTableChanged?.Invoke(); 
    }

    public void SpawnRewards(RewardsDO rewards){
        Dictionary<Resource,System.Func<int>> howManyFromFunctions = HowManyFromFunctions(rewards);
        foreach(Resource resource in ResourceInfo.GetAllResources()){
            int howManyToAdd = howManyFromFunctions[resource]();
            if(howManyToAdd>0){
                ResourceOnTablePrefab.resourceType = resource;
                ResourceOnTablePrefab.spriteRenderer.sortingOrder = Table.Instance.tableAnimation.spriteRenderer.sortingOrder+1;
                ResourceInfo.Info[resource].resourceOnTable = Instantiate(ResourceOnTablePrefab,ResourceInfo.Info[resource].resourcePositionOnTable,transform.rotation);
                ResourceInfo.Info[resource].resourceOnTable.addNOfResource(howManyToAdd);
                ResourceInfo.Info[resource].resourceOnTable.RewardCard();
            }
        }

    }

    Dictionary<Resource,System.Func<int>> HowManyFromFunctions(RewardsDO rewards){
        return new Dictionary<Resource, System.Func<int>>(){
            {Resource.Food,()=>rewards.food},
            {Resource.Money,()=>rewards.money},
            {Resource.Cultist,()=>rewards.cultist},
            {Resource.Prisoner,()=>rewards.prisoner},
            {Resource.Suspision,()=>rewards.suspicion},            
            {Resource.Relic,()=>rewards.relic},            
        };
    } 

    public void SacrificeAll(){

        bool anyOnTable = false;
        foreach(Resource resource in ResourceInfo.GetAllResources()){
            ResourceCard currentCard = ResourceInfo.Info[resource].resourceOnTable;
            if(currentCard!=null){
                anyOnTable = true;
                break;
            }
        }
        if(anyOnTable){
            tableAnimation.StartSacrifice();
            StartTableAnimation(()=>{
                foreach(Resource resource in ResourceInfo.GetAllResources()){
                    ResourceInfo.Info[resource].resourceOnTable?.SacrificeCard();
                }
            });
        }

    }

    private void StartTableAnimation(System.Action callback){
        callback();
    }

    public void AddResource(Resource resource){
        if(ResourceInfo.Info[resource].resourceOnTable==null){
            ResourceOnTablePrefab.resourceType = resource;
            ResourceInfo.Info[resource].resourceOnTable = Instantiate(ResourceOnTablePrefab,ResourceInfo.Info[resource].resourcePositionOnTable,transform.rotation);
            ResourceInfo.Info[resource].resourceOnTable.spriteRenderer.sortingOrder = tableAnimation.spriteRenderer.sortingOrder+1;
        }
        ResourceInfo.Info[resource].resourceOnTable.addOneOfResource();
        OnResourcedOnTableChanged();
    }

    public void AddGreedResource(Resource resource){
        AddResource(resource);
        ResourceInfo.Info[resource].resourceOnTable.OnDisable();
    }

    public void RemoveResource(Resource resource){
         if(ResourceInfo.Info[resource].resourceOnTable==null){
            return;
        }
        else{
            ResourceInfo.Info[resource].resourceOnTable.removeOneOfResource();
        }
        if(ResourceInfo.Info[resource].resourceOnTable.howMany==0){
            GameObject.Destroy(ResourceInfo.Info[resource].resourceOnTable.gameObject);
            ResourceInfo.Info[resource].resourceOnTable = null;
        }
        OnResourcedOnTableChanged();

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
