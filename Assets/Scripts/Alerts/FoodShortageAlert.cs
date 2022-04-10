public class FoodShortageAlert : Alert
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        ActiveOnMode = Mode.Cult;
        condition = new NoFood();
        
    }
}
