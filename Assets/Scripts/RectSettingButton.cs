

public class RectSettingButton : MenuButton
{
    public string Reset;
    public void OnClick(){
        if(Reset=="tutorial"){

        }
        else if(Reset=="gods"){
            Gods.AllGodsUndefeated();
            Gods.SaveToFile();
        }
    }
}
