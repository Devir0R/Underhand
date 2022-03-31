using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }


    public void LoadScene(){
        if(sceneName=="Game")   GameAudio.Instance.PlayGameMusic();
        else if(sceneName=="Main")   GameAudio.Instance.PlayMenuMusic();
        SceneManager.LoadScene(sceneName);
    }

}
