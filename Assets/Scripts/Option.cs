using UnityEngine.SceneManagement;
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

    public bool isDormant = false;

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
            Table.Instance.SpawnRewards(option.rewards);
            optionChosen=null;
        }
        
    }

    public bool LosingOption(){
        return option.islose==1;
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

            if(option.islose==1){
                GameState.GameLost();
            }
            else if(option.iswin!=""){
                GameState.GodWon = option.iswin;
                GameState.GameWon();
            }

            spriteRenderer.sprite = down;
            if(option.randomrequirements!=0){
                Hand.Instance.RemoveFromHandRandomly(option.randomrequirements);
            }
            if(Table.Instance.ResourcesOnTable().Count==0){
                LoadEndOfGame();
                OnOptionChosen();
            }
            else{
                ResourceCard.CardOnTableDestroyed +=MoveWhenCardsOnTableDestroyed;
                ResourceCard.CardOnTableDestroyed += LoadEndOfGame;
                Table.Instance.SacrificeAll();
            }
            
            Deck.Instance.AddToDiscard(option.shuffle);
        }
        
    }

    void LoadEndOfGame(){
        if(GameState.state==State.Lost || GameState.state==State.Won){
            SceneManager.LoadScene("WinLose");
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
        IEnumerable<RaycastHit2D> rayHits = Physics2D.GetRayIntersectionAll(ray).Where(hit=>(hit.collider?.CompareTag("Option")).GetValueOrDefault());

        return rayHits.Count()>0;
    }

    bool IsMouseOnMe(){
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        IEnumerable<RaycastHit2D> rayHits = Physics2D.GetRayIntersectionAll(ray);
        foreach(RaycastHit2D hit in rayHits){
            if(hit.collider.CompareTag("Option")){
                if(hit.collider.gameObject.GetComponent<Option>()==this){
                    return true;
                }
                else return false;
            }
        }
        return false;
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
        if(!(RequirementsCanBeMet(option.requirements,Table.Instance.ResourcesOnTable(),option.cultistequalsprisoner==1))
            || isEmptyOption()
            || option.islose==1){
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
        int demandedRelics = requirements.relic;
        if(prisonerEqualCultist){
            allResourcesOnTable = allResourcesOnTable.Select(resource=>resource==Resource.Prisoner ? Resource.Cultist: resource).ToList();
        }
        foreach(Resource resource in HowManyFrom.Keys){
            int howManyDemanded = specialNumbersAmounts(HowManyFrom[resource](),resource);
            for(int i = 0; i<howManyDemanded;i++){
                if(!allResourcesOnTable.Remove(resource))
                {
                    demandedRelics+=1;
                    if(demandedRelics>allResourcesOnTable.Where(resource=>resource==Resource.Relic).Count()){
                        return false;
                    }
                }
            }
            if((resource!=Resource.Relic) && allResourcesOnTable.Contains(resource)){
                return false;
            }
        }
        return demandedRelics==allResourcesOnTable.Where(resource=>resource==Resource.Relic).Count();
    }

    

    bool RequirementsCanBeMet(RequirementsDO requirements,List<Resource> resourcesOnTable,bool prisonerEqualCultist){
        List<Resource> allResources = new List<Resource>();
        allResources.AddRange(resourcesOnTable);
        Dictionary<Resource,System.Func<int>> HowManyFrom = HowManyFromFunctions(requirements);
        HowManyFrom.Remove(Resource.Relic);
        allResources.AddRange(Hand.Instance.ResourcesInHand());
        int demandedRelics = requirements.relic;
        if(prisonerEqualCultist){
            allResources = allResources.Select(resource=>resource==Resource.Prisoner ? Resource.Cultist: resource).ToList();
        }
        foreach(Resource resource in HowManyFrom.Keys){
            int howManyDemanded = specialNumbersAmounts(HowManyFrom[resource](),resource);
            for(int i = 0; i<howManyDemanded;i++){
                if(!allResources.Remove(resource)){
                    demandedRelics+=1;
                    if(demandedRelics>allResources.Where(resource=>resource==Resource.Relic).Count()){
                        return false;
                    }
                }
            }
        }
        return demandedRelics<=allResources.Where(resource=>resource==Resource.Relic).Count();
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
        List<Resource> resourceList = GetOptionRequirements();
        AddResourceRequirements(resourceList);
    }

    List<Resource> GetOptionRequirements(){
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
        return resourceList;
    }

    private int specialNumbersAmounts(int amount,Resource resource){
        int cardOfResourceInHand = Hand.Instance.HowManyOfResourceInHand(resource);
        int cardOfResourceOnTable = Table.Instance.ResourcesOnTable().Where(resourceOnTable=>resourceOnTable==resource).Count();
        int total = cardOfResourceInHand+cardOfResourceOnTable;
        if(amount==Hand.MAJORITY_ROUNDING_UP){
            return total/2+total%2;
        }
        else if(amount==Hand.MAJORITY_ROUNDING_DOWN){
            return total/2;
        }
        return amount;
    }

    private void AddRewards(){
        List<Resource> resourceList = GetOptionRewards();
        AddResourceRewards(resourceList);
    }

    private List<Resource> GetOptionRewards(){
        List<Resource> resourceList = new List<Resource>();
        Dictionary<Resource,System.Func<int>> howManyFromFunctions = HowManyFromFunctions(option.rewards);

        foreach(Resource resource in howManyFromFunctions.Keys){
            for(int i =0;i<howManyFromFunctions[resource]();i++){
                resourceList.Add(resource);
            }
        }
        return resourceList;
    }

    private void AddResourceRewards(List<Resource> GameObjectsresourceList){
        const float resourceSize_X = 0.35f*2*0.66f;
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
        const float resourceSize_X = 0.35f*2*0.66f;
        Vector3 instantiationPlace = transform.position + Vector3.right*((resourceSize_X*(GameObjectsresourceList.Count-1))/2);
        while(GameObjectsresourceList.Count>0){
            Resource r = GameObjectsresourceList[0];
            GameObjectsresourceList.RemoveAt(0);
            GameObject NewResource = Instantiate(resourcePrefab,instantiationPlace,transform.rotation,this.transform);
            resourceList.Add(NewResource);
            if(option.cultistequalsprisoner==0 || (r!=Resource.Cultist && r!=Resource.Prisoner)){
                NewResource.GetComponent<ResourceExchange>().SetSprite(r);
            }
            else{
                NewResource.GetComponent<ResourceExchange>().SetSprite(Resource.PrisonerOrCultist);
            }
            
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
            layerID =SortingLayer.NameToID("top");
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

    private bool isEmptyOption(){
        return option.optiontext=="" && option.outputtext=="" && 
            GetOptionRewards().Count()==0 && GetOptionRequirements().Count()==0;
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
