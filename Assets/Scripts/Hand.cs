using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


public class Hand : MonoBehaviour
{
    public TextMeshPro cardsNumber;
    private static Hand _instance;
    public static Hand Instance{ get { return _instance; } }
    private static readonly Resource[] startingHand = 
        new Resource[]{ Resource.Food,Resource.Food,Resource.Money,Resource.Money,
                            Resource.Cultist,Resource.Cultist,Resource.Prisoner,Resource.Prisoner};

    public Vector3 handMiddle;

    public float gapFromOneItemToTheNextOne; //the gap I need between each card

    public ResourceCard ResourcePrefab;

    public List<ResourceCard> hand;

    public static int MAJORITY_ROUNDING_DOWN = 840;
    public static int MAJORITY_ROUNDING_UP = 420;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gapFromOneItemToTheNextOne = ResourcePrefab.spriteRenderer.bounds.size.x*0.4f;
        SpawnCards();
        OrganizeHand();
        FitCards();
    }

    void SpawnCards(){
        hand = new List<ResourceCard>();
        foreach(Resource resource in startingHand.ToList()){
            ResourcePrefab.resourceType = resource;
            hand.Add(Instantiate(ResourcePrefab,handMiddle,transform.rotation));
        }
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
                    +(howManyAdded>hand.Count/2? 1 : 0);
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RemoveCardFromHand(ResourceCard card){
        hand.Remove(card);
        FitCards();
    }
    public void AddCardToHand(ResourceCard card){
        hand.Add(card);
        FitCards();
    }
    public IEnumerable<Resource> ResourcesInHand(){
        return this.hand.Select(cardObject=>cardObject.resourceType);
    }

}



public enum Resource{
    None,Food,Money,Suspision,Cultist,Prisoner,Relic,PrisonerOrCultist
}


