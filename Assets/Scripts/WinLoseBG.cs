using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinLoseBG : MonoBehaviour
{
    public Sprite BackgroundWin;
    public Sprite BackgroundLose;
    
    public Image spriteRenderer;
    public CanvasGroup group;

    public Continue ContinueButton;

    private static WinLoseBG _instance;
    public static WinLoseBG Instance{ get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

    }

    private void changeSprite(){
        if(GameState.state==State.Won){
            spriteRenderer.sprite = BackgroundWin;
        }
        else if(GameState.state==State.Lost){
            spriteRenderer.sprite = BackgroundLose;
        }
    }

    public IEnumerator FadeIn(){
        changeSprite();
        ContinueButton.changeSprite();
        float step = 1f/12f;
        while(group.alpha<1f){
            group.alpha += step;
            yield return new WaitForSeconds(0.1f);
        }
        ContinueButton.Enable();
    }

    public bool StartFadeIn(){
        if(GameState.state==State.Lost || GameState.state==State.Won){
            StartCoroutine(WinLoseBG.Instance.FadeIn());
            return true;
        }
        return false;
    }
}
