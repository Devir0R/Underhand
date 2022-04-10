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
    private static Dictionary<Resource,AsyncOperationHandle<IList<Sprite>>> resourcesSpritesHandler;

    public static List<Sprite> CultCardsSprites;
    public static List<Sprite> FightCultCardsSprites;
    public static List<TextAsset> CultCardsJsons;
    public static List<TextAsset> FightCultCardsJsons;
    public static Dictionary<Resource, List<Sprite>> resourcesSpritesDictionary;
    public static void LoadAddressables(){
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
        foreach(Resource resourceType in ResourceInfo.GetAllResources()){
            resourcesLoadPercent+=resourcesSpritesHandler[resourceType].PercentComplete;
        }
        //resourcesLoadPercent has 6 images with total 120 frames.
        //resourceSpriteHandler has 100 images
        //cardsJsonsHandler has 100 jsons so...
        resourcesLoadPercent = (resourcesLoadPercent/ResourceInfo.GetAllResources().Count())*0.16f;
        float cultCardsSpritesPercent = cultCardsSpriteHandler.PercentComplete*.21f;
        float fightCultCardsSpritesPercent = fightCultCardsSpriteHandler.PercentComplete*.21f;
        float cultCardsJsonsPercent = cultCardsJsonsHandler.PercentComplete*.21f;
        float fightCultCardsJsonsPercent = fightCultCardsJsonsHandler.PercentComplete*.21f;
        
        return resourcesLoadPercent +
                cultCardsSpritesPercent + 
                cultCardsJsonsPercent + 
                fightCultCardsSpritesPercent + 
                fightCultCardsJsonsPercent;
    }
}
