using UnityEngine;
using UnityEngine.UI;

public class GodButton : MonoBehaviour
{
    public Image Checkmark;
    public string godName;
    public bool Checked;
    public delegate void NotifyGodButtonClick(GodButton sender);
    public event NotifyGodButtonClick GodButtonClicked;

    protected virtual void OnGodButtonClicked() //protected virtual method
    {
        //if ProcessCompleted is not null then call delegate
        GodButtonClicked?.Invoke(this); 
    }

    public void UpdateGod(string godName){
        Sprite godSprite = Loader.GodsSprites.Find(sprite=>sprite.name==godName+"Thumbnail");
        if(godSprite!=null){
            GetComponent<Image>().sprite = godSprite;
        }
        this.godName = godName;
    }

    public void Clicked(){
        ToggleCheck();
        OnGodButtonClicked();
    }

    public void ToggleCheck(){
        Checked = !Checked;
        Color currentColor = Checkmark.color;
        Checkmark.color = new Color(currentColor.r,currentColor.g,currentColor.b,1.0f - currentColor.a);
    }
}
