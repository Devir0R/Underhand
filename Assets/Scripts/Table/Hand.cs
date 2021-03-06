using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using TMPro;


public class Hand : MonoBehaviour
{
    public static float GREED_CHANCE = 0.5f;
    public static float POLICE_RAID_CHANCE = 0.5f;
    public static float DESPARATE_MEASURES_CHANCE = 0.5f;
    public static float CULT_RISES_CHANCE = 0.5f;
    public static float INNER_DEMONS_CHANCE = 0.5f;

    public TextMeshPro cardsNumber;
    private static Hand _instance;
    public static Hand Instance{ get { return _instance; } }

    public Vector3 handMiddle;

    public float gapFromOneItemToTheNextOne; //the gap I need between each card

    public ResourceCard ResourcePrefab;

    public List<ResourceCard> hand;

    public static int ONLY_IF_RESOURCE_IS_ZERO = 840;
    public static int MAJORITY_ROUNDING_UP = 420;

    public Alert MoreThan15Cards;
    public Alert FiveOrMoreSuspision;
    public Alert NoFood;
    public Alert FiveOrMoreCorruption;
    public Alert FiveOrMoreFoe;

    public void WiggleResources(IOptionDO option){
        var howManyFromFunctions = 
            option.GetRequirements()
                    .HowManyFromFunctions(new RequirementsOptions()
                    {
                        allyequalreputation=option.IsAllyEqualReputation(),
                        cultistequalsprisoner=option.IsCultistEqualPrisoner()
                    });
        foreach(Resource resource in howManyFromFunctions.Keys){
            if(howManyFromFunctions[resource]()>0){
                foreach(ResourceCard card in hand){
                    if(resource.IsCoveredBy(card.resourceType)) card.Wiggle();
                }
            }
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public IEnumerator RemoveFromHandRandomly(int numberOfCards){
        List<int> cardsIndexesToRemove = new List<int>();
        for(int j =0;j<numberOfCards;j++){
            int randomIndex = Mathf.FloorToInt(Random.value*hand.Count);
            if(randomIndex==hand.Count || cardsIndexesToRemove.Contains(randomIndex)){
                j--;
            }
            else{
                cardsIndexesToRemove.Add(randomIndex);
            }
        }
        cardsIndexesToRemove.Sort();
        List<Coroutine> cardsJumps = new List<Coroutine>();
        for(int j =cardsIndexesToRemove.Count-1;j>0;j--){
            int indexToRemove = cardsIndexesToRemove[j];
            ResourceCard toRemove = hand[indexToRemove];//there might be a bug here
            hand.RemoveAt(indexToRemove);
            cardsJumps.Add(StartCoroutine(toRemove.JumpToTable()));
        }
        foreach(Coroutine jump in cardsJumps) yield return jump;
        Table.Instance.SacrificeAll();
        FitCards();
    }

    // Start is called before the first frame update
    void Start()
    {
        gapFromOneItemToTheNextOne = ResourcePrefab.spriteRenderer.bounds.size.x*0.4f;
        if(Loader.settings.tutorial) return;
        SpawnCards();
        OrganizeHand();
        FitCards();
    }

    void SpawnCards(){
        hand = new List<ResourceCard>();
        foreach(Resource resource in ResourceInfo.OpeningHand().ToList()){
            AddOfResource(resource);
        }
    }

    void AddOfResource(Resource resource){
        ResourcePrefab.resourceType = resource;
        hand.Add(Instantiate(ResourcePrefab,handMiddle,transform.rotation));
    }

    public int HowManyOfResourceInHand(Resource resource){
        if(resource!=Resource.PrisonerOrCultist){
            return hand.Where(card=>card.resourceType==resource).Count();
        }
        else{
            return hand.Where(card=>card.resourceType==Resource.Prisoner||card.resourceType==Resource.Cultist).Count();
        }
        
    }
    

    public void FitCards()
    {
        OrganizeHand();
        
        if (hand.Count == 0) //if list is null, stop function
             return;
        float totalTwist = 50f*((hand.Count-1)/15f);
        float twistPerCard = (totalTwist*2) / hand.Count;
        float scalingFactor = 0.4f*((hand.Count-1)/15f);


        Vector3 startingPosition = handMiddle +Vector3.right*(gapFromOneItemToTheNextOne*((hand.Count-1)*0.5f));
        for(int howManyAdded =0; howManyAdded<hand.Count;howManyAdded++){
            ResourceCard card = hand[howManyAdded]; //Reference to first image in my list
            card.transform.position = startingPosition; //relocating my card to the Start Position    
            startingPosition = startingPosition + Vector3.left*gapFromOneItemToTheNextOne;

            float amountOfTwist =  twistPerCard * howManyAdded-totalTwist;
            card.transform.rotation = Quaternion.identity;
            card.transform.Rotate(0,0,amountOfTwist);


            float nudgeThisCard = totalTwist==0? 0 : Mathf.Abs( amountOfTwist)/totalTwist;
            nudgeThisCard *= scalingFactor*ResourcePrefab.spriteRenderer.bounds.size.y;
            card.transform.Translate(0,-nudgeThisCard,0);

            card.spriteRenderer.sortingOrder = hand.Count- 2*Mathf.Abs((howManyAdded-(hand.Count/2)))
                    +(howManyAdded>hand.Count/2? 1 : 0) ;
        }
    }


    void OrganizeHand(){
        this.hand = this.hand.Where(card=>card.gameObject!=null).ToList();
        this.hand.Sort((r1,r2)=>r1.resourceType.CompareTo(r2.resourceType));
        UpdateCardsNumberIndicator();
    }

    void UpdateCardsNumberIndicator(){
        cardsNumber.text = hand.Count+"";
    }


    public void RemoveCardFromHand(ResourceCard card){
        hand.Remove(card);
        FitCards();
    }
    public void AddCardToHand(ResourceCard card){
        hand.Add(card);
        FitCards();
    }

    public void AddCardToHand(Resource resource,int howMany){
        for(int i = 0;i<howMany;i++){
            AddOfResource(resource);
        }
        FitCards();
    }
    public IEnumerable<Resource> ResourcesInHand(){
        return this.hand.Select(cardObject=>cardObject.resourceType);
    }

}

static class ResourceMethods {
    public static bool isWildCard(this Resource res){
        return res==Resource.Relic || res==Resource.Holy;
    }

    public static bool IsCoveredBy(this Resource toCover, Resource covering){
        if(covering.isWildCard())    return true;
        return toCover.CanBeCoverBy().Contains(covering);
    }

    public static bool IsMultiple(this Resource resource)=>resource==Resource.PrisonerOrCultist;

    public static ICollection<Resource> CanBeCoverBy(this Resource resource){
        List<Resource> canCover = new List<Resource>();
        if(resource==Resource.PrisonerOrCultist){
            canCover.Add(Resource.Cultist);
            canCover.Add(Resource.Prisoner);
        }
        else{
            canCover.Add(resource);
        }
        return canCover;
    }
}


