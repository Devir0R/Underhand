using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Discard : MonoBehaviour
{
    public Sprite[] spriteArray;
    public List<CardDO> discard;
    private static Discard _instance;
    public static Discard Instance{ get { return _instance; } }
    public SpriteRenderer spriteRenderer;
    bool movedIn = false;
    bool moveingOut = false;
     
    public Card cardToDicard;

    Vector3 originalPosition;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

    }

    public void shuffleDiscard(){
        this.discard = this.discard.OrderBy(x => Random.value).ToList();
    }

    void UpdateDeckSprite(){
        float leftOfDeck = ((float)(this.discard.Count))/(Deck.INITIAL_DECK_SIZE+1);
        int spriteIndex = (int)((1-leftOfDeck)*(spriteArray.Length-1));
        spriteRenderer.sprite = spriteArray[spriteIndex];
    }

    // Start is called before the first frame update
    public void MoveIn(){
        StartCoroutine(MoveDiscardIn());
    }

    public void MoveOut(){
        if(!moveingOut){
            moveingOut = true;
            StartCoroutine(MoveDiscardOut());
        } 
    }

    private IEnumerator MoveDiscardIn(){
        if(!movedIn){
            Vector3 moveTo = transform.position + Vector3.right*(spriteRenderer.bounds.size.x*1.2f);
            //move in
            while(transform.position!=moveTo){
                transform.position = Vector3.MoveTowards(transform.position, moveTo,  Time.deltaTime*9);
                yield return null;
            }
            movedIn = true;
        }
    }

    private IEnumerator MoveDiscardOut(){
        while(cardToDicard==null || !movedIn){
            yield return null;
        }
        StartCoroutine(cardToDicard.FlipCard());
        while(!cardToDicard.faceUp){
            yield return null;
        }

        Vector3 whereToMove = transform.position;
         cardToDicard.FinishingMove();

        while(cardToDicard.transform.position!=transform.position) yield return null;

        if(cardToDicard!=null) GameObject.Destroy(cardToDicard.gameObject);
        while(transform.position!=originalPosition){
            transform.position = Vector3.MoveTowards(transform.position, originalPosition,  Time.deltaTime*9);
            yield return null;
        }
        movedIn = false;
        moveingOut = false;
    
    }




    // Update is called once per frame
    void Update()
    {
        
    }

    void Start(){
        originalPosition = transform.position;
    }
}
