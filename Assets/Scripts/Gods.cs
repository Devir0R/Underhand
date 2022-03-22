using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class Gods
{

    public static AllGods allGods{get;set;}
    // Start is called before the first frame update

    public Gods(TextAsset godsJSON){
        allGods = JsonConvert.DeserializeObject<AllGods>(godsJSON.text);
    }
    public List<int> godBlessingCardNumbers(int num){
        if (num>=0){
            return allGods.gods.Find(god=>god.num==num).blessings;
        }
        else{
            return allGods.gods.Select(god=>god.blessings).SelectMany(b=>b).ToList();
        }
    }

    public List<GodDO> getUnlockedGods(){
        return allGods.gods
            .Where(god=>
                god.dependency.All(godnum=>
                    allGods.gods.Find(god=>god.num==godnum).defeated==1)).ToList();
    }

}

[System.Serializable]
public class AllGods{
    public List<GodDO> gods;
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

}
