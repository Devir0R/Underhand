public class FoeOverloadAlert : Alert
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        ActiveOnModes =new Mode[]{Mode.FightCult};
        condition = new FiveOrMoreFoe();


    }

}
