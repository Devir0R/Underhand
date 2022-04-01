using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public string sceneName;

    public Vector3 RelativePosition;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        UpdateOnce();

    }

    void UpdateOnce(){
        RectTransform menuRect = GameObject.FindGameObjectWithTag("GameMenu").GetComponent<RectTransform>();
        float newYPosition = (menuRect.rect.height*RelativePosition.y);
        float newXPosition = (menuRect.rect.width*RelativePosition.x);
        transform.position = menuRect.transform.position + new Vector3(newXPosition,newYPosition,transform.position.z);
        float yScale = 1f/4.5f;
        float xScale  = 0.36f;
        Rect mySize = GetComponent<RectTransform>().rect;
        float newYScale = (menuRect.rect.height/mySize.height)*yScale;
        float newXScale = (menuRect.rect.width/mySize.width)*xScale;
        transform.localScale = new Vector3(newXScale,newYScale,transform.localScale.z);
    }


    public void LoadScene(){
        if(sceneName=="Game")   GameAudio.Instance.PlayGameMusic();
        else if(sceneName=="Main")   GameAudio.Instance.PlayMenuMusic();
        SceneManager.LoadScene(sceneName);
    }

}
