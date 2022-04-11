public class SuspisionOverloadAlert : Alert
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        ActiveOnModes = new Mode[]{Mode.Cult};
        condition = new FiveOrMoreSuspision();


    }

}
