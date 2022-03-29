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
     
    public Card cardToDicard;

    Vector3 originalPosition;
    public Card cardPrefab;

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

    public IEnumerator MoveIn(){
        Vector3 moveTo = transform.position + Vector3.right*(spriteRenderer.bounds.size.x*1.2f);
        //move in
        while(transform.position!=moveTo){
            transform.position = Vector3.MoveTowards(transform.position, moveTo,  Time.deltaTime*9);
            yield return null;
        }
    }

    public IEnumerator MoveDiscardOut(){
        StartCoroutine(cardToDicard.FlipCard());
        while(!cardToDicard.faceUp){
            yield return null;
        }

        Vector3 whereToMove = transform.position;
        yield return Deck.Instance.CheckIfInsertCard(cardToDicard);

        yield return cardToDicard.MoveTo(transform.position,30f);
        cardToDicard.spriteRenderer.sortingLayerName = "behindScene";
        yield return MoveOut();

        GameObject.Destroy(cardToDicard.gameObject);
    }

    public IEnumerator MoveOut(){
        while(transform.position!=originalPosition){
            transform.position = Vector3.MoveTowards(transform.position, originalPosition,  Time.deltaTime*9);
            yield return null;
        }
    }




    // Update is called once per frame
    void Update()
    {
        
    }

    void Start(){
        originalPosition = transform.position;
    }
}