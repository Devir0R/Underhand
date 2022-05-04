using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public static class Loader
{
    private static AsyncOperationHandle<IList<Sprite>> cultCardsSpriteHandler;
    private static AsyncOperationHandle<IList<Sprite>> fightCultCardsSpriteHandler;
    private static AsyncOperationHandle<IList<TextAsset>> cultCardsJsonsHandler;
    private static AsyncOperationHandle<IList<TextAsset>> fightCultCardsJsonsHandler;
    private static AsyncOperationHandle<IList<Sprite>> godsSpritesHandler;
    private static AsyncOperationHandle<TextAsset> godsJsonHandler;
    private static AsyncOperationHandle<TextAsset> cultsJsonHandler;
    private static AsyncOperationHandle<TextAsset> settingsJsonHandler;
    private static Dictionary<Resource,AsyncOperationHandle<IList<Sprite>>> resourcesSpritesHandler;

    public static List<Sprite> GodsSprites;
    public static List<Sprite> CultCardsSprites;
    public static List<Sprite> FightCultCardsSprites;
    public static List<TextAsset> CultCardsJsons;
    public static List<TextAsset> FightCultCardsJsons;
    public static Dictionary<Resource, List<Sprite>> resourcesSpritesDictionary;
    public static Settings settings;

    public static AllGods godsInfo{get{
        return Mode.FightCult==GameState.GameMode? FightCultGods : CultGods;
    }}

    public static AllGods allGods{get{
        return GameState.GameMode==Mode.FightCult? FightCultGods : CultGods;
    }}

    public static void SaveToFile(){
	    BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(PlaceToSaveBosses());
        bf.Serialize(file,allGods);
        file.Close();
    }

    public static void SaveSettings(){
	    BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(PlaceToSaveSettings());
        bf.Serialize(file,settings);
        file.Close();
    }

    public static void ChangeLastSummon(string god){
        Loader.settings.previous_summon = god;
        Loader.SaveSettings();
    }

    public static void ResetLastSummon(){
        Loader.settings.previous_summon = "";
        Loader.SaveSettings();
    }
    private static AllGods FightCultGods;

    private static AllGods CultGods;

    private static string PlaceToSaveCults(){
        return Application.persistentDataPath +  "/cults.json";
    }

    private static string PlaceToSaveGods(){
        return Application.persistentDataPath +  "/gods.json";
    }

    private static string PlaceToSaveSettings(){
        return Application.persistentDataPath +  "/settings.json";
    }

    private static string PlaceToSaveBosses(){
        return (GameState.GameMode == Mode.FightCult? PlaceToSaveCults() : PlaceToSaveGods());
    }

    private static bool LoadCults()
    {
        if (File.Exists(PlaceToSaveCults()))
        {
            settings = LoadJson<Settings>(PlaceToSaveSettings());
            return true;
        }
        else{
            return false;
        }            
    }

    private static bool LoadSettings()
    {
        if (File.Exists(PlaceToSaveSettings()))
        {
            CultGods = LoadJson<AllGods>(PlaceToSaveGods());
            return true;
        }
        else{
            return false;
        }            
    }
    private static bool LoadGods()
    {
        if (File.Exists(PlaceToSaveGods()))
        {
            CultGods = LoadJson<AllGods>(PlaceToSaveGods());
            return true;
        }
        else{
            return false;
        }            
    }


    private static T LoadJson<T>(string path){
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = 
                File.Open(path, FileMode.Open);
        T TJson = (T)bf.Deserialize(file);
        file.Close();
        return TJson;
        
    }

    public static void LoadAddressables(){
        godsSpritesHandler = Addressables.LoadAssetsAsync<Sprite>("GodsImages", null);
        godsSpritesHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  GodsSprites =  handleToCheck.Result.ToList();
        };

        cultCardsSpriteHandler = Addressables.LoadAssetsAsync<Sprite>("CultCardsImages", null);
        cultCardsSpriteHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  CultCardsSprites =  handleToCheck.Result.ToList();
        };

        fightCultCardsSpriteHandler = Addressables.LoadAssetsAsync<Sprite>("FightCultCardsImages", null);
        fightCultCardsSpriteHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  FightCultCardsSprites =  handleToCheck.Result.ToList();
        };


        cultCardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("CultJsonCards", null);
        cultCardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  CultCardsJsons =  handleToCheck.Result.ToList();
        };


        fightCultCardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("FightCultJsons", null);
        fightCultCardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  FightCultCardsJsons =  handleToCheck.Result.ToList();
        };

        if(!LoadCults()){
            cultsJsonHandler = Addressables.LoadAssetAsync<TextAsset>("CultsJson");
            cultsJsonHandler.Completed += handleToCheck=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded){
                    TextAsset cultsJSON = handleToCheck.Result;
                    FightCultGods = JsonConvert.DeserializeObject<AllGods>(cultsJSON.text);
                }
            };
        }

        if(!LoadGods()){
            godsJsonHandler = Addressables.LoadAssetAsync<TextAsset>("GodsJson");
            godsJsonHandler.Completed += handleToCheck=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded){
                    TextAsset godsJSON = handleToCheck.Result;
                    CultGods = JsonConvert.DeserializeObject<AllGods>(godsJSON.text);
                }
            };
        }

        if(!LoadSettings()){
            settingsJsonHandler = Addressables.LoadAssetAsync<TextAsset>("SettingsJson");
            settingsJsonHandler.Completed += handleToCheck=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded){
                    TextAsset settingsJson = handleToCheck.Result;
                    settings = JsonConvert.DeserializeObject<Settings>(settingsJson.text);
                }
            };
        }

        resourcesSpritesHandler = new Dictionary<Resource, AsyncOperationHandle<IList<Sprite>>>();
        resourcesSpritesDictionary = new Dictionary<Resource, List<Sprite>>();
        foreach(Resource resourceType in ResourceInfo.GetAllResources_AllModes()){
            resourcesSpritesHandler[resourceType] = Addressables.LoadAssetAsync<IList<Sprite>>(ResourceInfo.Info[resourceType].imagesLabel);
            resourcesSpritesHandler[resourceType].Completed += handleToCheck=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded){
                    resourcesSpritesDictionary[resourceType] =  handleToCheck.Result.ToList();
                    resourcesSpritesDictionary[resourceType].Sort((sp1,sp2)=>{
                        int index1 = sp1.name.IndexOf('_')+1;
                        int index2 = sp2.name.IndexOf('_')+1;
                        string numberStr1 = sp1.name.Substring(index1);
                        string numberStr2 = sp2.name.Substring(index1);
                        if(int.TryParse(numberStr1,out int num1) &&int.TryParse(numberStr2,out int num2))
                            return num1.CompareTo(num2);
                        return 0;
                    });
                }  
            };
        }
    }
    

    public static float PercentComplete(){
        float resourcesLoadPercent = 0f;
        foreach(Resource resourceType in ResourceInfo.GetAllResources_AllModes()){
            resourcesLoadPercent+=resourcesSpritesHandler[resourceType].PercentComplete;
        }

        float totalAmountOfFiles = 140f+120f+120f+50f+50f+16f+4f+4f+2f;


        resourcesLoadPercent = (resourcesLoadPercent/ResourceInfo.GetAllResources_AllModes().Count()) *(140f/totalAmountOfFiles);//140 images
        float cultCardsSpritesPercent = cultCardsSpriteHandler.PercentComplete*(120f/totalAmountOfFiles);//120 images
        float fightCultCardsSpritesPercent = fightCultCardsSpriteHandler.PercentComplete*(50f/totalAmountOfFiles);//50 images
        float cultCardsJsonsPercent = cultCardsJsonsHandler.PercentComplete*(120f/totalAmountOfFiles);//120 jsons
        float fightCultCardsJsonsPercent = fightCultCardsJsonsHandler.PercentComplete*(50f/totalAmountOfFiles);//50 jsons
        float godsSpritesPercent = godsSpritesHandler.PercentComplete*(16f/totalAmountOfFiles);//16 images
        float cultsJsonPercent = (cultsJsonHandler.Result==null? 1: cultsJsonHandler.PercentComplete)*(4f/totalAmountOfFiles);
        float godsJsonPercent = (cultsJsonHandler.Result==null? 1: cultsJsonHandler.PercentComplete)*(4f/totalAmountOfFiles);
        float settingssJsonPercent = (settingsJsonHandler.Result==null? 1: settingsJsonHandler.PercentComplete)*(2f/totalAmountOfFiles);

        return resourcesLoadPercent +
                cultCardsSpritesPercent + 
                cultCardsJsonsPercent + 
                fightCultCardsSpritesPercent + 
                fightCultCardsJsonsPercent +
                godsSpritesPercent + godsJsonPercent + cultsJsonPercent + settingssJsonPercent;
    }
}

[System.Serializable]
public class Settings{
    public bool tutorial;
    public float master_volume;
    public string previous_summon;
}
