using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public string sceneName;


    public Vector3 RelativePosition;

    public ButtonType type = ButtonType.Round;

    // Start is called before the first frame update
    protected void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        Update();

    }

    void Update(){
        RectTransform menuRect = GameObject.FindGameObjectWithTag("GameMenu").GetComponent<RectTransform>();
        float newYPosition = (menuRect.rect.height*RelativePosition.y);
        float newXPosition = (menuRect.rect.width*RelativePosition.x);
        
        if(type!=ButtonType.Arrow)  transform.position = menuRect.transform.position + new Vector3(newXPosition,newYPosition,transform.position.z);
        Rect mySize = GetComponent<RectTransform>().rect;
        float newYScale;
        float newXScale;
        if(type==ButtonType.Round){
            float yScale = 1f/4.6f;
            float xScale  = 0.34f;
            newYScale = (menuRect.rect.height/mySize.height)*yScale;
            newXScale = (menuRect.rect.width/mySize.width)*xScale;
        }
        else if(type==ButtonType.Arrow){
            float sideSize = Mathf.Min(menuRect.rect.height/3f,menuRect.rect.width/3f);
            newYScale = (sideSize/mySize.height);
            newXScale = (sideSize/mySize.width);
        }
        else if(type==ButtonType.Setting){
            float yScale = 0.181f;
            float xScale  = 0.32f;
            newYScale = (menuRect.rect.height/mySize.height)*yScale;
            newXScale = (menuRect.rect.width/mySize.width)*xScale;
        }
        else if(type==ButtonType.RectSetting){
            float yScale = 0.14f;
            float xScale  = 0.5f;
            newYScale = (menuRect.rect.height/mySize.height)*yScale;
            newXScale = (menuRect.rect.width/mySize.width)*xScale;
        }
        else{//really should never happen
            newYScale = (menuRect.rect.height/mySize.height)*0.1f;
            newXScale = (menuRect.rect.width/mySize.width)*0.1f;
        }

        transform.localScale = new Vector3(newXScale,newYScale,transform.localScale.z);

    }


    public void LoadScene(){
        if(sceneName=="Game")   GameAudio.Instance.PlayGameMusic();
        else if(sceneName=="Main")   GameAudio.Instance.PlayMenuMusic();
        else if(sceneName=="GodChooseMenu" && Loader.settings.tutorial){
            GameAudio.Instance.PlayGameMusic();
            SceneManager.LoadScene("Game");
            return;
        }
        if(sceneName!="")   SceneManager.LoadScene(sceneName);
    }


}

public enum ButtonType{
    Round,Arrow,Setting,RectSetting
}
