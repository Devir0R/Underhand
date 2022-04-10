using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Gods
{

    public static AllGods allGods{get;set;}
    // Start is called before the first frame update

    public Gods(TextAsset godsJSON){
        if(!LoadGame()) allGods = JsonConvert.DeserializeObject<AllGods>(godsJSON.text);
    }
    public List<int> godBlessingCardNumbers(int num){
        if (num>=0){
            return allGods.gods.Find(god=>god.num==num).blessings;
        }
        else{
            return allGods.gods.Select(god=>god.blessings).SelectMany(b=>b).ToList();
        }
    }

    public static void GodDefeated(string godName){
        Gods.allGods.gods.Find(god=>god.name==godName).defeated=1;
    }
    public static void GodUndefeated(string godName){
        Gods.allGods.gods.Find(god=>god.name==godName).defeated=0;
    }

    public static void AllGodsUndefeated(){
        Gods.allGods.gods.ForEach(god=>god.defeated=0);
    }
    public List<GodDO> getUnlockedGods(){
        return allGods.gods
            .Where(god=>
                god.dependency.All(godnum=>
                    allGods.gods.Find(god=>god.num==godnum).defeated==1)).ToList();
    }


    public static void SaveToFile(){
	    BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(PlaceToSave());
        bf.Serialize(file,allGods);
        file.Close();
    }

    public static bool LoadGame()
    {
        if (File.Exists(PlaceToSave()))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = 
                    File.Open(PlaceToSave(), FileMode.Open);
            AllGods allGods = (AllGods)bf.Deserialize(file);
            file.Close();
            Gods.allGods = allGods;
            //Debug.Log("Game data loaded!");
            return true;
        }
        else{
            //Debug.LogError("There is no save data!");
            return false;
        }            
    }

    private static string PlaceToSave(){
        return Application.persistentDataPath + (GameState.GameMode == Mode.FightCult? "/leaders.json" : "/gods.json");
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
