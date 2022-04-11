using System.Linq;

public interface AlertCondition
{
    bool Check();

}

public class MoreThan15Cards : AlertCondition{
    public bool Check(){
        int resourcesOnTable = Table.Instance.ResourcesOnTable().Count;
        int ResourcesInHand = Hand.Instance.hand.Count;
        return resourcesOnTable+ResourcesInHand>15;

    }
}


public class NoFood : AlertCondition{
    public bool Check(){
        int resourcesOnTable = Table.Instance.ResourcesOnTable().Where(res=>res==Resource.Food).Count();
        int ResourcesInHand = Hand.Instance.HowManyOfResourceInHand(Resource.Food);
        return resourcesOnTable+ResourcesInHand==0;
    }
}


public class FiveOrMoreSuspision : AlertCondition{
    public bool Check(){
        int resourcesOnTable = Table.Instance.ResourcesOnTable().Where(res=>res==Resource.Suspision).Count();
        int ResourcesInHand = Hand.Instance.HowManyOfResourceInHand(Resource.Suspision);
        return resourcesOnTable+ResourcesInHand>=5;
    }
}

public class FiveOrMoreCorrpution : AlertCondition{
    public bool Check(){
        int resourcesOnTable = Table.Instance.ResourcesOnTable().Where(res=>res==Resource.Corruption).Count();
        int ResourcesInHand = Hand.Instance.HowManyOfResourceInHand(Resource.Corruption);
        return resourcesOnTable+ResourcesInHand>=5;
    }
}

public class FiveOrMoreFoe : AlertCondition{
    public bool Check(){
        int resourcesOnTable = Table.Instance.ResourcesOnTable().Where(res=>res==Resource.Foe).Count();
        int ResourcesInHand = Hand.Instance.HowManyOfResourceInHand(Resource.Foe);
        return resourcesOnTable+ResourcesInHand>=5;
    }
}
