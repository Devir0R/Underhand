using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class Loader
{
    private static AsyncOperationHandle<IList<Sprite>> cardsSpriteHandler;
    private static AsyncOperationHandle<IList<TextAsset>> cardsJsonsHandler;
    private static Dictionary<Resource,AsyncOperationHandle<IList<Sprite>>> resourcesSpritesHandler;

    public static List<Sprite> cardsSprites;
    public static List<TextAsset> cardsJsons;
    public static Dictionary<Resource, List<Sprite>> resourcesSpritesDictionary;
    public static void LoadAddressables(){
        cardsSpriteHandler = Addressables.LoadAssetsAsync<Sprite>("CardsImages", null);
        cardsSpriteHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  cardsSprites =  handleToCheck.Result.ToList();
        };


        cardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("CultJsons", null);
        cardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  cardsJsons =  handleToCheck.Result.ToList();
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
        float cardsSpritesPercent = cardsSpriteHandler.PercentComplete*.42f;
        float cardsJsonsPercent = cardsJsonsHandler.PercentComplete*.42f;
        
        return resourcesLoadPercent+cardsSpritesPercent+cardsJsonsPercent;
    }
}
