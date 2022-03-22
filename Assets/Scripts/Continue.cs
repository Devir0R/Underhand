
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

public class Continue : MonoBehaviour
{
    public Sprite WinButton;
    public Sprite WinButtonClicked;
    public Sprite LostButton;
    public Sprite LostButtonClicked;
    public SpriteRenderer spriteRenderer;
    
    [SerializeField]
    public InputAction mouseClicked;

    int clickPhase = 0;

    public int disable = 60;

    // Start is called before the first frame update
    void Start()
    {
        if(GameState.state==State.Won){
            spriteRenderer.sprite = WinButton;
        }
        else if(GameState.state==State.Lost){
            spriteRenderer.sprite = LostButton;
        }
    }
    
    void Update(){
        disable = disable==0? 0 : disable-1;
    }

    private void OnEnable(){
        mouseClicked.Enable();
        mouseClicked.started += changeToActive;
        mouseClicked.performed += MiddleOfClick;
        mouseClicked.canceled += Clicked;
    }

    private void OnDisable(){
        mouseClicked.performed -= Clicked;
        mouseClicked.started -= MiddleOfClick;
        mouseClicked.started -= changeToActive;
        mouseClicked.Disable();
    }

    void changeToActive(InputAction.CallbackContext context){
        if(disable!=0) return;
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

    private void Clicked(InputAction.CallbackContext context){
        if(clickPhase!=2){
            clickPhase = 0;
            return;
        }
        if(GameState.state==State.Won){
            Gods.allGods.gods.Find(god=>god.name==GameState.GodWon).defeated=1;
            string godsJson = JsonConvert.SerializeObject(Gods.allGods,Formatting.Indented);
            File.WriteAllText(Application.dataPath+"/Scripts/json/gods.json",godsJson);
        }
        SceneManager.LoadScene("Game");
    }
}
