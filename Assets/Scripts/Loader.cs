using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class Loader
{
    private static AsyncOperationHandle<IList<Sprite>> cultCardsSpriteHandler;
    private static AsyncOperationHandle<IList<Sprite>> fightCultCardsSpriteHandler;
    private static AsyncOperationHandle<IList<TextAsset>> cultCardsJsonsHandler;
    private static AsyncOperationHandle<IList<TextAsset>> fightCultCardsJsonsHandler;
    private static AsyncOperationHandle<IList<Sprite>> godsSpritesHandler;
    private static Dictionary<Resource,AsyncOperationHandle<IList<Sprite>>> resourcesSpritesHandler;

    public static List<Sprite> GodsSprites;
    public static List<Sprite> CultCardsSprites;
    public static List<Sprite> FightCultCardsSprites;
    public static List<TextAsset> CultCardsJsons;
    public static List<TextAsset> FightCultCardsJsons;
    public static Dictionary<Resource, List<Sprite>> resourcesSpritesDictionary;
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


        cultCardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("CultJsons", null);
        cultCardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  CultCardsJsons =  handleToCheck.Result.ToList();
        };


        fightCultCardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("FightCultJsons", null);
        fightCultCardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  FightCultCardsJsons =  handleToCheck.Result.ToList();
        };


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

        float totalAmountOfFiles = 140f+120f+120f+50f+50f+16f;


        resourcesLoadPercent = (resourcesLoadPercent/ResourceInfo.GetAllResources_AllModes().Count()) *(140f/totalAmountOfFiles);//140 images
        float cultCardsSpritesPercent = cultCardsSpriteHandler.PercentComplete*(120f/totalAmountOfFiles);//120 images
        float fightCultCardsSpritesPercent = fightCultCardsSpriteHandler.PercentComplete*(50f/totalAmountOfFiles);//50 images
        float cultCardsJsonsPercent = cultCardsJsonsHandler.PercentComplete*(120f/totalAmountOfFiles);//120 jsons
        float fightCultCardsJsonsPercent = fightCultCardsJsonsHandler.PercentComplete*(50f/totalAmountOfFiles);//50 jsons
        float godsSpritesPercent = godsSpritesHandler.PercentComplete*(16f/totalAmountOfFiles);//16 images
        
        return resourcesLoadPercent +
                cultCardsSpritesPercent + 
                cultCardsJsonsPercent + 
                fightCultCardsSpritesPercent + 
                fightCultCardsJsonsPercent +
                godsSpritesPercent;
    }
}
