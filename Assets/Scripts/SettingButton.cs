using UnityEngine;
using UnityEngine.UI;

public class SettingButton : MenuButton
{
    public Sprite onImage;
    public Sprite offImage;
    public Image myImage;

    // Start is called before the first frame update
    new void Start()
    {
        type = ButtonType.Setting;
        if(action=="audio"){
            setSprite(GameAudio.Instance.isAudioOn());
        }
        else if (action=="sound"){
            setSprite(GameAudio.Instance.isSoundOn());
        }
        base.Start();
    }

    void setSprite(bool isOn){
        myImage.sprite = isOn ? onImage : offImage;
    }
    
    public void OnClick(){
        if(action=="audio"){
            GameAudio.Instance.FlipAudio();
            setSprite(GameAudio.Instance.isAudioOn());
        }
        else if(action=="sound"){
            GameAudio.Instance.FlipSound();
            setSprite(GameAudio.Instance.isSoundOn());
        }
    }

}