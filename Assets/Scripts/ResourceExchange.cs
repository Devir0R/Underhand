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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SetSprite(Resource resource){
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
