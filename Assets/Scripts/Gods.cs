using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Gods
{

    public static void GodDefeated(string godName){
        Loader.allGods.gods.Find(god=>god.name==godName).defeated=1;
    }
    public static void GodUndefeated(string godName){
        Loader.allGods.gods.Find(god=>god.name==godName).defeated=0;
    }

    public static void AllGodsUndefeated(){
        Loader.allGods.gods.ForEach(god=>god.defeated=(god.name=="God of Beginnings"? 1: 0));
    }
    public static List<GodDO> getUnlockedGods(){
        return Loader.allGods.gods
            .Where(god=>
                god.dependency.All(godnum=>
                    Loader.allGods.gods.Find(god=>god.num==godnum).defeated==1)).ToList();
    }
}

[System.Serializable]
public class AllGods{
    [SerializeField]
    public List<GodDO> gods;

    public List<int> godBlessingCardNumbers(int num){
        if (num>=0){
            return gods.Find(god=>god.num==num).blessings;
        }
        else{
            return gods.Select(god=>god.blessings).SelectMany(b=>b).ToList();
        }
    }
}

[System.Serializable]
public class GodDO{
        public int num;
        public string name;
        public int defeated;
        public List<int> blessings;
        public List<int> dependency;
        public int startingCard;
        public int specialRequirements;
        public int tier;
}
