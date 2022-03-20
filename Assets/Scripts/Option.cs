using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class Option : MonoBehaviour
{
    OptionDO option;
    public Sprite dormant;
    public Sprite down;
    public Sprite ready;
    public Sprite active;

    public SpriteRenderer spriteRenderer;

    private Vector3 moveTo;
    private static Vector3 zero = Vector3.up*1000000;

    public GameObject resourcePrefab;

    int optionNum;

    public TextMeshPro output;
    public TextMeshPro optionText;
    // Start is called before the first frame update
    bool disableOption;

    private Camera mainCamera;

    private List<GameObject> resourceList = new List<GameObject>();
    private List<GameObject> rewardList = new List<GameObject>();

    private System.Action cardCallBack;

    private bool movedBack = false;

    private bool isDormant = false;

    private static Option optionChosen = null;

    public delegate void NotifyOptionChosen();
    public static event NotifyOptionChosen OptionChosen;
    protected virtual void OnOptionChosen() //protected virtual method
    {
        //if ProcessCompleted is not null then call delegate
        OptionChosen?.Invoke(); 
    }
    private void Awake(){
        mainCamera = Camera.main;
    }
    void Start()
    {
        OptionChosen += MoveOptionBack;
        OptionChosen += this.SpawnReawrds;
    }

    public void SpawnReawrds()
    {
        if(optionChosen==this){
            Table.Instance.SpawnReawrds(option.rewards);
            optionChosen=null;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!moveTo.Equals( zero)){
            transform.position = Vector3.MoveTowards(transform.position, moveTo,  Time.deltaTime*10);
            if(transform.position.Equals(moveTo)){
                moveTo = zero;
                if(cardCallBack!=null && optionNum==3 && movedBack){
                    cardCallBack();
                    cardCallBack = null;
                }
            }
        }
    }

    public void OnOptionHover(){

    }

    public void OnOptionClicked(InputAction.CallbackContext context){
        if(this.disableOption || ! context.performed || !IsMouseOnOption()) return ;
        if(isDormant || !spriteRenderer.sprite.name.Contains("Ready")) return;

        if(IsMouseOnMe()){
            disableSpriteChange();
            DisbleOption();
            optionChosen = this;
            
            spriteRenderer.sprite = down;
            if(Table.Instance.ResourcesOnTable().Count==0){
                OnOptionChosen();
            }
            else{
                ResourceCard.CardOnTableDestroyed +=MoveWhenCardsOnTableDestroyed;
                Table.Instance.SacrificeAll();
            }                
        }
        
    }

    void MoveWhenCardsOnTableDestroyed(){
        ResourceCard.CardOnTableDestroyed-=MoveWhenCardsOnTableDestroyed;
        OnOptionChosen();
    }

    void DisbleOption(){
        this.disableOption = true;
    }

    void EnableOption(){
        this.disableOption = false;
    }

    public void MoveOption(System.Action cardCallBack){
        this.cardCallBack = cardCallBack;
        if(optionNum==1){
            moveTo = transform.position+Vector3.left*spriteRenderer.bounds.size.x;

        }
        else if(optionNum==3){
            moveTo = transform.position+Vector3.right*spriteRenderer.bounds.size.x;
        }
    }

    public void MoveOptionBack(){
        if(optionNum==1){
            moveTo = transform.position+Vector3.right*spriteRenderer.bounds.size.x;

        }
        else if(optionNum==3){
            moveTo = transform.position+Vector3.left*spriteRenderer.bounds.size.x;
        }
        movedBack = true;
    }

    bool IsMouseOnOption(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        return rayHit.collider!=null && rayHit.collider.CompareTag("Option");
    }

    bool IsMouseOnMe(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray);

        return rayHit.collider!=null && rayHit.collider.gameObject.GetComponent<Option>()==this;
    }

    void UpdateSprite(){
        if(isDormant) return;
        
        List<Resource> resourcesOnTable = Table.Instance.ResourcesOnTable();
        RequirementsDO requirements = option.requirements;
        if(RequirementsAreMet(requirements,resourcesOnTable,option.cultistequalsprisoner==1)){
            spriteRenderer.sprite = ready;
        }
        else{
            spriteRenderer.sprite = active;
        }
    }

    void UpdateSpriteFirstTime(){
        if(!(RequirementsCanBeMet(option.requirements,Table.Instance.ResourcesOnTable(),option.cultistequalsprisoner==1))){
            isDormant=true;
            spriteRenderer.sprite = dormant;
        }
        else UpdateSprite();
    }

    private void OnDestroy(){
        disableSpriteChange();
        OptionChosen -= MoveOptionBack;
        OptionChosen -= this.SpawnReawrds;
    }

    private void disableSpriteChange(){
        Table.Instance.ResourcedOnTableChanged -= UpdateSprite;
    }

    Dictionary<Resource,System.Func<int>> HowManyFromFunctions(RequirementsDO requirements){
        return new Dictionary<Resource, System.Func<int>>(){
            {Resource.Food,()=>requirements.food},
            {Resource.Money,()=>requirements.money},
            {Resource.Cultist,()=>requirements.cultist},
            {Resource.Prisoner,()=>requirements.prisoner},
            {Resource.Suspision,()=>requirements.suspicion},
            {Resource.Relic,()=>requirements.relic},
        };
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

    bool RequirementsAreMet(RequirementsDO requirements,List<Resource> resourcesOnTable,bool prisonerEqualCultist){
        List<Resource> allResourcesOnTable = new List<Resource>();
        allResourcesOnTable.AddRange(resourcesOnTable);
        Dictionary<Resource,System.Func<int>> HowManyFrom = HowManyFromFunctions(requirements);
        int demandedRelics = 0;
        foreach(Resource resource in HowManyFrom.Keys){
            int howManyDemanded = specialNumbersAmounts(HowManyFrom[resource](),resource);
            for(int i = 0; i<howManyDemanded;i++){
                if(!allResourcesOnTable.Remove(resource) || 
                    (resource==Resource.Cultist && prisonerEqualCultist &&!allResourcesOnTable.Remove(Resource.Prisoner)) ||
                    (resource==Resource.Prisoner && prisonerEqualCultist &&!allResourcesOnTable.Remove(Resource.Cultist)))
                {
                    demandedRelics+=1;
                    if(demandedRelics>requirements.relic){
                        return false;
                    }
                }
            }
            if(allResourcesOnTable.Contains(resource)){
                return false;
            }
        }
        return demandedRelics==requirements.relic;
    }

    

    bool RequirementsCanBeMet(RequirementsDO requirements,List<Resource> resourcesOnTable,bool prisonerEqualCultist){
        List<Resource> allResources = new List<Resource>();
        allResources.AddRange(resourcesOnTable);
        Dictionary<Resource,System.Func<int>> HowManyFrom = HowManyFromFunctions(requirements);
        allResources.AddRange(Hand.Instance.ResourcesInHand());
        int demandedRelics = 0;
        foreach(Resource resource in HowManyFrom.Keys){
            int howManyDemanded = specialNumbersAmounts(HowManyFrom[resource](),resource);
            for(int i = 0; i<howManyDemanded;i++){
                if(!allResources.Remove(resource) || 
                    (resource==Resource.Cultist && prisonerEqualCultist &&!allResources.Remove(Resource.Prisoner)) ||
                    (resource==Resource.Prisoner && prisonerEqualCultist &&!allResources.Remove(Resource.Cultist))){
                    demandedRelics+=1;
                    if(demandedRelics>requirements.relic){
                        return false;
                    }
                }
            }
        }
        return demandedRelics<=requirements.relic;
    }

    public void UpdateDO(OptionDO option,int optionNum){
        this.option = option;
        this.optionNum = optionNum;
        UpdateSpriteFirstTime();
        Table.Instance.ResourcedOnTableChanged += UpdateSprite;
        moveTo = zero;

        output.text = option.outputtext;
        optionText.text = option.optiontext;
        AddRequirements();
        AddRewards();

        MoveOptionLayer();
    }

    private void AddRequirements(){
        List<Resource> resourceList = new List<Resource>();
        bool prisonerEqualCultist = option.cultistequalsprisoner==1;

        Dictionary<Resource,System.Func<int>> howManyFromFunctions = HowManyFromFunctions(option.requirements);

        foreach(Resource resource in howManyFromFunctions.Keys){
            int howManyToAdd = specialNumbersAmounts(howManyFromFunctions[resource](),resource);
            for(int i =0;i<howManyToAdd;i++){
                if(prisonerEqualCultist && (resource ==  Resource.Prisoner||resource ==  Resource.Prisoner)){
                    resourceList.Add(Resource.PrisonerOrCultist);
                }
                else resourceList.Add(resource);
            }
        }
        AddResourceRequirements(resourceList);
    }

    private int specialNumbersAmounts(int amount,Resource resource){
        if(amount==Hand.MAJORITY_ROUNDING_UP){
            int cardOfResourceInHand = Hand.Instance.HowManyOfResourceInHand(resource);
            return cardOfResourceInHand/2+cardOfResourceInHand%2;
        }
        else if(amount==Hand.MAJORITY_ROUNDING_DOWN){
            int cardOfResourceInHand = Hand.Instance.HowManyOfResourceInHand(resource);
            return cardOfResourceInHand/2;
        }
        return amount;
    }

    private void AddRewards(){
        List<Resource> resourceList = new List<Resource>();
        Dictionary<Resource,System.Func<int>> howManyFromFunctions = HowManyFromFunctions(option.rewards);

        foreach(Resource resource in howManyFromFunctions.Keys){
            for(int i =0;i<howManyFromFunctions[resource]();i++){
                resourceList.Add(resource);
            }
        }
        AddResourceRewards(resourceList);
    }

    private void AddResourceRewards(List<Resource> GameObjectsresourceList){
        const float resourceSize_X = 0.30f*2*0.7f;
        const float resourceSize_Y = 1.0f*0.7f;
        Vector3 instantiationPlace = transform.position 
                        + Vector3.right*((resourceSize_X*(GameObjectsresourceList.Count-1))/2)
                        + Vector3.down*resourceSize_Y;
        while(GameObjectsresourceList.Count>0){
            Resource r = GameObjectsresourceList[0];
            GameObjectsresourceList.RemoveAt(0);
            GameObject NewResource = Instantiate(resourcePrefab,instantiationPlace,transform.rotation,this.transform);
            rewardList.Add(NewResource);
            NewResource.GetComponent<ResourceExchange>().SetSprite(r);
            instantiationPlace += Vector3.left*resourceSize_X;
        }
    }

    private void AddResourceRequirements(List<Resource> GameObjectsresourceList){
        const float resourceSize_X = 0.35f*2*0.7f;
        Vector3 instantiationPlace = transform.position + Vector3.right*((resourceSize_X*(GameObjectsresourceList.Count-1))/2);
        while(GameObjectsresourceList.Count>0){
            Resource r = GameObjectsresourceList[0];
            GameObjectsresourceList.RemoveAt(0);
            GameObject NewResource = Instantiate(resourcePrefab,instantiationPlace,transform.rotation,this.transform);
            resourceList.Add(NewResource);
            NewResource.GetComponent<ResourceExchange>().SetSprite(r);
            instantiationPlace += Vector3.left*resourceSize_X;
        }
    }

    private void MoveOptionLayer(){
        int layerID;

        if(optionNum==1){
            layerID =SortingLayer.NameToID("bottom");
        }
        else if(optionNum==3){
            layerID =SortingLayer.NameToID("middle");
        }
        else{
            layerID =SortingLayer.NameToID("default");
        }
        spriteRenderer.sortingLayerID =layerID;
        output.sortingLayerID = layerID;
        optionText.sortingLayerID = layerID;  
        foreach(GameObject obj in resourceList){
            obj.GetComponent<ResourceExchange>().spriteRenderer.sortingLayerID = layerID;
        }
        foreach(GameObject obj in rewardList){
            obj.GetComponent<ResourceExchange>().spriteRenderer.sortingLayerID = layerID;
        }    
    }
}




[System.Serializable]
public class OptionDO{
        public string optiontext;
        public string outputtext;
        public int cultistequalsprisoner;
        public int randomrequirements;
        public RequirementsDO requirements;
        public RewardsDO rewards;
        public ForesightDO foresight;
        public ShuffleDO shuffle;
        public string iswin;
        public int islose;

}
