using UnityEngine;

public class WinLoseBG : MonoBehaviour
{
    public Sprite BackgroundWin;
    public Sprite BackgroundLose;

    public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if(GameState.state==State.Won){
            spriteRenderer.sprite = BackgroundWin;
        }
        else if(GameState.state==State.Lost){
            spriteRenderer.sprite = BackgroundLose;
        }
    }
}
