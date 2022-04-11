
public class NumberOfCardsAlert : Alert
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        ActiveOnModes = new Mode[]{Mode.Cult,Mode.FightCult};
        condition = new MoreThan15Cards();

    }
}
