using System.Collections.Generic;
using UnityEngine;

public class Requirements : MonoBehaviour
{
}

[System.Serializable]
public class RequirementsDO:IRequirementsDO{
	public int relic;
	public int money;
	public int cultist;
	public int food;
	public int prisoner;
	public int suspicion;
    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions(RequirementsOptions options = new RequirementsOptions()){
        if(!options.cultistequalsprisoner)
            return new Dictionary<Resource, System.Func<int>>(){
                {Resource.Food,()=>food},
                {Resource.Money,()=>money},
                {Resource.Cultist,()=>cultist},
                {Resource.Prisoner,()=>prisoner},
                {Resource.Suspision,()=>suspicion},
                {Resource.Relic,()=>relic},
            };
        else 
            return new Dictionary<Resource, System.Func<int>>(){
                {Resource.Food,()=>food},
                {Resource.Money,()=>money},
                {Resource.PrisonerOrCultist,()=>prisoner+cultist},
                {Resource.Suspision,()=>suspicion},
                {Resource.Relic,()=>relic},
            };
    }

}

[System.Serializable]
public class FightCultRequirementsDO:IRequirementsDO{
	public int holy;
	public int money;
	public int foe;
	public int corruption;
	public int ally;
	public int reputation;

    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions(RequirementsOptions options = new RequirementsOptions()){
        if(!options.allyequalreputation)
            return new Dictionary<Resource, System.Func<int>>(){
                {Resource.Foe,()=>foe},
                {Resource.Money,()=>money},
                {Resource.Ally,()=>ally},
                {Resource.Corruption,()=>corruption},
                {Resource.Holy,()=>holy},
                {Resource.Reputation,()=>reputation},
            };
        else
            return new Dictionary<Resource, System.Func<int>>(){
                {Resource.Foe,()=>foe},
                {Resource.Money,()=>money},
                {Resource.AllyReputation,()=>ally+reputation},
                {Resource.Holy,()=>holy},
                {Resource.Corruption,()=>corruption},
            };
    }
}

public interface IRequirementsDO{
    public Dictionary<Resource,System.Func<int>> HowManyFromFunctions(RequirementsOptions options = new RequirementsOptions());
}

public struct RequirementsOptions{
    public bool cultistequalsprisoner;
    public bool allyequalreputation;
}