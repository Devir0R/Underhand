using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceExchange : MonoBehaviour
{
    public Sprite Suspicion;
    public Sprite Relic;
    public Sprite Cultist;
    public Sprite Prisoner;
    public Sprite Money;
    public Sprite Food;
    public Sprite CultistPrisoner;

    public SpriteRenderer spriteRenderer;

    public bool greyed = false;
    public Resource myType = Resource.None;

    public void Grey(){
        spriteRenderer.material.color = Color.grey;
        greyed = true;
    }
    public void Ungrey(){
        spriteRenderer.material.color = Color.white;
        greyed = false;
    }
    
    public void SetSprite(Resource resource){
        myType = resource;
        switch(resource){
            case Resource.Cultist:
                spriteRenderer.sprite = Cultist;
            break;
            case Resource.Prisoner:
                spriteRenderer.sprite = Prisoner;
            break;
            case Resource.Relic:
                spriteRenderer.sprite = Relic;
            break;
            case Resource.Suspision:
                spriteRenderer.sprite = Suspicion;
            break;
            case Resource.Food:
                spriteRenderer.sprite = Food;
            break;
            case Resource.Money:
                spriteRenderer.sprite = Money;
            break;
            case Resource.PrisonerOrCultist:
                spriteRenderer.sprite = CultistPrisoner;
            break;
        }

    }

}
