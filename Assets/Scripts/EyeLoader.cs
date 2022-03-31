using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class EyeLoader : MonoBehaviour
{
    public List<Sprite> spriteList = new List<Sprite>();
    public Image myImage;

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame(){
        Loader.LoadAddressables();
        float loadPercent = Loader.PercentComplete();
        while(loadPercent<1f){
            int index = Mathf.FloorToInt(loadPercent*(spriteList.Count-1));
            myImage.sprite = spriteList[index];
            loadPercent = Loader.PercentComplete();
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("Main");

    }
}
