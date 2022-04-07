
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Continue : MonoBehaviour
{
    public Sprite WinButton;
    public Sprite WinButtonClicked;
    public Sprite LostButton;
    public Sprite LostButtonClicked;
    public Image spriteRenderer;
    
    [SerializeField]
    public InputAction mouseClicked;

    int clickPhase = 0;

    public bool disabled = true;

    public void changeSprite(){
        if(GameState.state==State.Won){
            spriteRenderer.sprite = WinButton;
        }
        else if(GameState.state==State.Lost){
            spriteRenderer.sprite = LostButton;
        }
    }

    
    void Update(){
        if(GameState.state==State.Lost||GameState.state==State.Won){
            mouseClicked.Enable();
            mouseClicked.started += changeToActive;
            mouseClicked.performed += MiddleOfClick;
            mouseClicked.canceled += Clicked;
        }
    }
    void changeToActive(InputAction.CallbackContext context){
        if(disabled) return;
        spriteRenderer.sprite = spriteRenderer.sprite == WinButton? WinButtonClicked : LostButtonClicked;
        clickPhase = 1;
    }

    void MiddleOfClick(InputAction.CallbackContext context){
        if(clickPhase==1){
            clickPhase=2;
        }
        else{
            clickPhase = 0;
        }
    }

    public void Enable()=> disabled = false;

    private void Clicked(InputAction.CallbackContext context){
        if(clickPhase!=2){
            clickPhase = 0;
            return;
        }
        clickPhase = 0;
        mouseClicked.performed -= Clicked;
        mouseClicked.started -= MiddleOfClick;
        mouseClicked.started -= changeToActive;
        mouseClicked.Disable();
        disabled = true;
        if(GameState.state==State.Won){
            Gods.GodDefeated(GameState.GodWon);
            Gods.SaveToFile();
            
        }
        GameAudio.Instance.PlayMenuMusic();
        SceneManager.LoadScene("Main");
    }

}
