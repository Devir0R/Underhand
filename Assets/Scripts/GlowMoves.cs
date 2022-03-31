using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GlowMoves : MonoBehaviour
{
    public List<Sprite> spriteList = new List<Sprite>();

    public Image myImage;
    // Update is called once per frame
    void Start()
    {
        StartCoroutine(GlowAnimation());
    }

    IEnumerator GlowAnimation(){
        int index = 0;
        while(true){
            index = (index+1)%spriteList.Count;
            myImage.sprite = spriteList[index];
            yield return new WaitForSeconds(0.08f);
        }
    }
}
