
using System.Collections.Generic;
using UnityEngine;

public class Rewards : MonoBehaviour
{

}

[System.Serializable]
public class RewardsDO:IRewardsDO{
    public int relic;
	public int money;
	public int cultist;
	public int food;
	public int prisoner;
	public int suspicion;

    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions(){
        return new Dictionary<Resource, System.Func<int>>(){
            {Resource.Food,()=>food},
            {Resource.Money,()=>money},
            {Resource.Cultist,()=>cultist},
            {Resource.Prisoner,()=>prisoner},
            {Resource.Suspision,()=>suspicion},
            {Resource.Relic,()=>relic},
        };
    }
}


[System.Serializable]
public class FightCultRewardsDO:IRewardsDO{
    public int holy;
	public int money;
	public int foe;
	public int ally;
	public int corruption;
	public int reputation;
    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions(){
        return new Dictionary<Resource, System.Func<int>>(){
            {Resource.Foe,()=>foe},
            {Resource.Money,()=>money},
            {Resource.Ally,()=>ally},
            {Resource.Corruption,()=>corruption},
            {Resource.Holy,()=>holy},
            {Resource.Reputation,()=>reputation},
        };
    }
}


public interface IRewardsDO{
    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions();
}