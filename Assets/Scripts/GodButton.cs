using UnityEngine;
using UnityEngine.UI;

public class GodButton : MonoBehaviour
{
    public Image Checkmark;

    public bool Checked;

    public void UpdateGod(string godName){
        Sprite godSprite = Loader.GodsSprites.Find(sprite=>sprite.name==godName+"Thumbnail");
        if(godSprite!=null){
            GetComponent<Image>().sprite = godSprite;
        }
    }

    public void ToggleCheck(){
        Checked = !Checked;
        Color currentColor = Checkmark.color;
        Checkmark.color = new Color(currentColor.r,currentColor.g,currentColor.b,1.0f - currentColor.a);
    }
}
