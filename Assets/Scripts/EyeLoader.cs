using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class EyeLoader : MonoBehaviour
{
    public List<Sprite> spriteList = new List<Sprite>();
    public Image myImage;

    int currentIndex = 0;

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame(){
        Loader.LoadAddressables();
        float loadPercent = Loader.PercentComplete();
        while(currentIndex<spriteList.Count-1){
            currentIndex = Mathf.Min(currentIndex+1,Mathf.FloorToInt(loadPercent*(spriteList.Count-1)));
            myImage.sprite = spriteList[currentIndex];
            loadPercent = Loader.PercentComplete();
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        GameAudio.Instance.PlayMenuMusic();
        SceneManager.LoadScene("Main");

    }
}
