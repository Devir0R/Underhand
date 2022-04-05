using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableAnimation : MonoBehaviour
{
    public List<Sprite> idleSprites = new List<Sprite>();
    public List<Sprite> sacrificeSprites = new List<Sprite>();
    public AudioClip SacrificeClip;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer backgroundSpriteRenderer;
    bool inSacrifice = false;
    
    // Start is called before the first frame update
    void Start(){
        Vector3 tableBGPosition = backgroundSpriteRenderer.gameObject.transform.position;
        float tableHeight = backgroundSpriteRenderer.bounds.size.y;
        float tableWidth = backgroundSpriteRenderer.bounds.size.x;
        float animationHeight = spriteRenderer.bounds.size.y;
        float animationWidth = spriteRenderer.bounds.size.x;
        float heightRatio = 0.42f;
        float widthRatio = 0.8f;
        float newAnimationHeight = tableHeight*heightRatio;
        float newAnimationWidth = tableWidth*widthRatio;
        transform.position = new Vector3(tableBGPosition.x,tableBGPosition.y-tableHeight*0.1f,tableBGPosition.z);
        transform.localScale = new Vector3(newAnimationWidth/animationWidth,newAnimationHeight/animationHeight,transform.localScale.z);
    }

    void Awake(){
        StartIdleness();
    }


    public void StartSacrifice(){
        GameAudio.Instance.PlayTrack(SacrificeClip);
        StartCoroutine(Sacrifice());
    }

    public void StartIdleness(){
        StartCoroutine(Idle());
    }

    private IEnumerator Sacrifice(){
        inSacrifice = true;
        int index = 0;
        while(index<sacrificeSprites.Count){
            spriteRenderer.sprite = sacrificeSprites[index];
            index++;
            yield return new WaitForSeconds(1f/32f);
        }
        inSacrifice = false;

        StartIdleness();
    }


    public IEnumerator Idle(){
        int index  = 0;
        while(!inSacrifice){
            index = (index+1) % idleSprites.Count;
            spriteRenderer.sprite = idleSprites[index];
            yield return new WaitForSeconds(1f/32f);
        }
    }
}
