using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class Option : MonoBehaviour
{
    IOptionDO option;
    public Sprite dormant;
    public Sprite down;
    public Sprite ready;
    public Sprite active;

    public SpriteRenderer spriteRenderer;

    public ResourceExchange resourcePrefab;

    int optionNum;

    public TextMeshPro output;
    public TextMeshPro optionText;
    // Start is called before the first frame update
    public bool disableOption;

    private Camera mainCamera;

    private List<ResourceExchange> requirementsObjects = new List<ResourceExchange>();
    private List<ResourceExchange> rewardList = new List<ResourceExchange>();

    private System.Action cardCallBack;

    private bool movedBack = false;

    public bool isDormant = false;

    private static Option optionChosen = null;

    public delegate void NotifyOptionChosen();
    public static event NotifyOptionChosen OptionChosen;

    public CardDO cardDO;

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
        StartCoroutine(MarkResourcesOnTable());
    }

    public void AddOnChooseListeners(){
        OptionChosen += MoveOptionBack;
        OptionChosen += this.SpawnRewards;
    }

    public void SpawnRewards()
    {
        if(optionChosen==this){
            Table.Instance.SpawnRewards(option.GetRewards());
            optionChosen=null;
        }
        
    }

    public bool LosingOption(){
        return option.IsLose();
    }

    public void OnOptionHover(){

    }
    public IEnumerator MoveTo(Vector3 to,float speed = 10f){
        while(transform.position!=to){
            transform.position = Vector3.MoveTowards(transform.position, to,  Time.deltaTime*speed);
            yield return null;
        }
        if(cardCallBack!=null && optionNum==3 && movedBack){
            cardCallBack();
            cardCallBack = null;
        }
    }
    public void OnOptionClicked(InputAction.CallbackContext context){
        if(this.disableOption || ! context.performed || !IsMouseOnOption()) return ;
        if(isDormant || !spriteRenderer.sprite.name.Contains("Ready")) return;

        if(IsMouseOnMe()){
            disableSpriteChange();
            DisbleOption();
            optionChosen = this;

            if(option.IsLose()){
                GameState.GameLost();
            }
            else if(option.IsWin()!=""){
                GameState.GodWon = option.IsWin();
                GameState.GameWon();
            }

            spriteRenderer.sprite = down;
            if(option.RandomRequirements()!=0){
                ResourceCard.CardOnTableDestroyed +=whenCardsOnTableDestroyed;
                StartCoroutine(Hand.Instance.RemoveFromHandRandomly(option.RandomRequirements()));                
            }
            else if(Table.Instance.ResourcesOnTable().Count==0) optionRealized();
            else{
                ResourceCard.CardOnTableDestroyed +=whenCardsOnTableDestroyed;
                Table.Instance.SacrificeAll();
            }
            GameAudio.Instance.AddToQueue(cardDO.GetTitle(),optionNum);
            Deck.Instance.AddToDiscard(option.GetShuffle());
        }
        
    }
    

    void optionRealized(){
        checkForesight();
        StartCoroutine(checkForesightDone());

    }
    private IEnumerator checkForesightDone(){
        while(Deck.Instance.performingForesight){
            yield return new WaitForSeconds(0.5f);
        }
        if(!WinLoseBG.Instance.StartFadeIn())    OnOptionChosen();
    }
    

    void whenCardsOnTableDestroyed(){
        ResourceCard.CardOnTableDestroyed-=whenCardsOnTableDestroyed;
        optionRealized();
    }

    void checkForesight(){
        if(option.GetForesight().hasforesight==1){
            StartCoroutine( Deck.Instance.Foresight(option.GetForesight().candiscard==1));
        }
    }

    void DisbleOption(){
        this.disableOption = true;
    }

    void EnableOption(){
        this.disableOption = false;
    }

    public Coroutine MoveOption(System.Action cardCallBack){
        this.cardCallBack = cardCallBack;
        Vector3 to = Vector3.zero;
        if(optionNum==1){
            to = transform.position+Vector3.left*spriteRenderer.bounds.size.x;

        }
        else if(optionNum==3){
            to = transform.position+Vector3.right*spriteRenderer.bounds.size.x;
        }
        if(to!=Vector3.zero){
            return StartCoroutine(MoveTo(to));
        }
        return null;
    }

    public void MoveOptionBack(){
        Vector3 to = Vector3.zero;
        if(optionNum==1){
            to = transform.position+Vector3.right*spriteRenderer.bounds.size.x;

        }
        else if(optionNum==3){
            to = transform.position+Vector3.left*spriteRenderer.bounds.size.x;
        }
        movedBack = true;
        if(to!=Vector3.zero){
            StartCoroutine(MoveTo(to));
        }
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
        IRequirementsDO requirements = option.GetRequirements();
        if(RequirementsAreMet(requirements,resourcesOnTable)){
            spriteRenderer.sprite = ready;
        }
        else{
            spriteRenderer.sprite = active;
        }
    }

    public void WakeUp(){
        isDormant = false;
        UpdateSprite();
    }

    void UpdateSpriteFirstTime(){
        if(!(RequirementsCanBeMet(option.GetRequirements(),Table.Instance.ResourcesOnTable(),option.IsCultistEqualPrisoner()))
            || isEmptyOption()
            || option.IsLose()
            || LockedUnlessZeroOfResource()){
            isDormant=true;
            spriteRenderer.sprite = dormant;
        }
        else UpdateSprite();
    }

    bool LockedUnlessZeroOfResource(){
        Dictionary<Resource,System.Func<int>> howManyFromFunctions = option.GetRequirements().HowManyFromFunctions();
        foreach(Resource resource in howManyFromFunctions.Keys){
            if(howManyFromFunctions[resource]()==Hand.ONLY_IF_RESOURCE_IS_ZERO){
                return Table.Instance.ResourcesOnTable().Where(resourseOnTable=>resourseOnTable==resource).Count() +
                        Hand.Instance.HowManyOfResourceInHand(resource)>0;
            }
        }
        return false;
    }

    private void OnDestroy(){
        disableSpriteChange();
        OptionChosen -= MoveOptionBack;
        OptionChosen -= this.SpawnRewards;
    }

    private void disableSpriteChange(){
        Table.Instance.ResourcedOnTableChanged -= UpdateSprite;
    }


    bool RequirementsAreMet(IRequirementsDO requirements,List<Resource> resourcesOnTable){
        return SpareResources(requirements,resourcesOnTable)==0;
    }

    int SpareResources(IRequirementsDO requirements,List<Resource> resourcesToCover){
        List<Resource> allResourcesOnTable = new List<Resource>();
        allResourcesOnTable.AddRange(resourcesToCover);

        Dictionary<Resource,System.Func<int>> HowManyFrom = requirements.HowManyFromFunctions(new RequirementsOptions(){cultistequalsprisoner=option.IsCultistEqualPrisoner()});
        allResourcesOnTable.Sort();
        foreach(Resource resource in HowManyFrom.Keys){
            int howManyDemanded = specialNumbersAmounts(HowManyFrom[resource](),resource);
            for(int i = 0; i<howManyDemanded;i++){
                bool canCoverThisResource = false;
                for(int j=0;j<allResourcesOnTable.Count;j++ ){
                    Resource resourceOnTable = allResourcesOnTable[j];
                    if(resource.IsCoveredBy(resourceOnTable)){
                        allResourcesOnTable.RemoveAt(j);
                        canCoverThisResource = true;
                        break;
                    }
                }
                if(!canCoverThisResource)   return -1;
            }
        }
        return allResourcesOnTable.Count;
    }

    

    bool RequirementsCanBeMet(IRequirementsDO requirements,List<Resource> resourcesOnTable,bool prisonerEqualCultist){
        List<Resource> allResources = new List<Resource>();
        allResources.AddRange(resourcesOnTable);
        allResources.AddRange(Hand.Instance.ResourcesInHand());
        return SpareResources(requirements,allResources)>=0;
    }

    public void UpdateDO(IOptionDO option,int optionNum){
        this.option = option;
        this.optionNum = optionNum;
        UpdateSpriteFirstTime();
        Table.Instance.ResourcedOnTableChanged += UpdateSprite;

        output.text = option.GetOutputText();
        optionText.text = option.GetOptionText();
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
        bool prisonerEqualCultist = option.IsCultistEqualPrisoner();

        Dictionary<Resource,System.Func<int>> howManyFromFunctions = option.GetRequirements().HowManyFromFunctions();
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
        else if(amount==Hand.ONLY_IF_RESOURCE_IS_ZERO){
            return 0;
        }
        return amount;
    }

    private void AddRewards(){
        List<Resource> resourceList = GetOptionRewards();
        AddResourceRewards(resourceList);
    }

    private List<Resource> GetOptionRewards(){
        List<Resource> resourceList = new List<Resource>();
        Dictionary<Resource,System.Func<int>> howManyFromFunctions = option.GetRewards().HowManyFromFunctions();

        foreach(Resource resource in howManyFromFunctions.Keys){
            for(int i =0;i<howManyFromFunctions[resource]();i++){
                resourceList.Add(resource);
            }
        }
        return resourceList;
    }

    private void AddResourceRewards(List<Resource> GameObjectsresourceList){
        float distance_X = (spriteRenderer.bounds.size.x*0.85f)/(GameObjectsresourceList.Count+1);
        float distance_Y = spriteRenderer.bounds.size.y/7f;
        Vector3 instantiationPlace = transform.position 
                        + Vector3.right*((distance_X*(GameObjectsresourceList.Count-1))/2)
                        + Vector3.down*distance_Y;
        while(GameObjectsresourceList.Count>0){
            Resource r = GameObjectsresourceList[0];
            GameObjectsresourceList.RemoveAt(0);
            ResourceExchange NewResource = Instantiate(resourcePrefab,instantiationPlace,transform.rotation,this.transform);
            rewardList.Add(NewResource);
            NewResource.GetComponent<ResourceExchange>().SetSprite(r);
            instantiationPlace += Vector3.left*distance_X;
        }
    }

    private void AddResourceRequirements(List<Resource> GameObjectsresourceList){
        float distance_X = (spriteRenderer.bounds.size.x*0.85f)/(GameObjectsresourceList.Count+1);
        Vector3 instantiationPlace = transform.position + Vector3.right*((distance_X*(GameObjectsresourceList.Count-1))/2);
        while(GameObjectsresourceList.Count>0){
            Resource r = GameObjectsresourceList[0];
            GameObjectsresourceList.RemoveAt(0);
            ResourceExchange NewResource = Instantiate(resourcePrefab,instantiationPlace,transform.rotation,this.transform);
            requirementsObjects.Add(NewResource);
            if(!option.IsCultistEqualPrisoner() || (r!=Resource.Cultist && r!=Resource.Prisoner)){
                NewResource.GetComponent<ResourceExchange>().SetSprite(r);
            }
            else{
                NewResource.GetComponent<ResourceExchange>().SetSprite(Resource.PrisonerOrCultist);
            }
            
            instantiationPlace += Vector3.left*distance_X;
        }
    }

    private IEnumerator MarkResourcesOnTable(){
        while(this!=null){
            foreach(ResourceExchange resOfReq in requirementsObjects){
                resOfReq.Ungrey();
            }

            int howManyMarked = 0;
            List<Resource> resourcesOnTable = Table.Instance.ResourcesOnTable();
            resourcesOnTable.Sort();
            foreach(Resource resource in resourcesOnTable){
                foreach(ResourceExchange resOfReq in requirementsObjects){
                    if(!resOfReq.greyed && (resOfReq.myType.IsCoveredBy(resource)) ){
                        resOfReq.Grey();
                        howManyMarked+=1;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
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
        foreach(ResourceExchange obj in requirementsObjects){
            obj.spriteRenderer.sortingLayerID = layerID;
        }
        foreach(ResourceExchange obj in rewardList){
            obj.spriteRenderer.sortingLayerID = layerID;
        }    
    }

    private bool isEmptyOption(){
        return option.GetOptionText()=="" && option.GetOutputText()=="" && 
            GetOptionRewards().Count()==0 && GetOptionRequirements().Count()==0;
    }
}




[System.Serializable]
public class CultOptionDO:IOptionDO{
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
        public bool IsCultistEqualPrisoner()=>cultistequalsprisoner==1;
        public bool IsAllyEqualReputation()=>false;
        public IRequirementsDO GetRequirements()=>requirements;
        public IRewardsDO GetRewards()=>rewards;
        public string GetOptionText()=>optiontext;
        public string GetOutputText()=>outputtext;
        public int RandomRequirements()=>randomrequirements;
        public ForesightDO GetForesight()=>foresight;
        public ShuffleDO GetShuffle()=>shuffle;
        public bool IsLose()=>islose==1;
        public string IsWin()=>iswin;
}

public class FightCultOptionDO:IOptionDO{
    public string optiontext;
    public string outputtext;
    public int allyequalreputation;
    public int randomrequirements;
    public FightCultRequirementsDO requirements;
    public FightCultRewardsDO rewards;
    public ForesightDO foresight;
    public ShuffleDO shuffle;
    public string iswin;
    public int islose;
    public bool IsCultistEqualPrisoner()=> false;
    public bool IsAllyEqualReputation()=>allyequalreputation==1;
    public IRequirementsDO GetRequirements()=>requirements;
    public IRewardsDO GetRewards()=>rewards;
    public string GetOptionText()=>optiontext;
    public string GetOutputText()=>outputtext;
    public int RandomRequirements()=>randomrequirements;
    public ForesightDO GetForesight()=>foresight;
    public ShuffleDO GetShuffle()=>shuffle;
    public bool IsLose()=>islose==1;
    public string IsWin()=>iswin;
}

public interface IOptionDO{
    public bool IsCultistEqualPrisoner();
    public bool IsAllyEqualReputation();
    public IRequirementsDO GetRequirements();
    public IRewardsDO GetRewards();
    public string GetOptionText();
    public string GetOutputText();
    public int RandomRequirements();
    public ForesightDO GetForesight();
    public ShuffleDO GetShuffle();
    public bool IsLose();
    public string IsWin();
}
