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


        cardsJsonsHandler = Addressables.LoadAssetsAsync<TextAsset>("CardsJsons", null);
        cardsJsonsHandler.Completed += handleToCheck=>{
            if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  cardsJsons =  handleToCheck.Result.ToList();
        };


        resourcesSpritesHandler = new Dictionary<Resource, AsyncOperationHandle<IList<Sprite>>>();
        resourcesSpritesDictionary = new Dictionary<Resource, List<Sprite>>();
        foreach(Resource resourceType in ResourceInfo.AllResources){
            resourcesSpritesHandler[resourceType] = Addressables.LoadAssetAsync<IList<Sprite>>(ResourceInfo.Info[resourceType].imagesLabel);
            resourcesSpritesHandler[resourceType].Completed += handleToCheck=>{
                if(handleToCheck.Status == AsyncOperationStatus.Succeeded)  resourcesSpritesDictionary[resourceType] =  handleToCheck.Result.ToList();
            };
        }
    }
    

    public static float PercentComplete(){
        float resourcesLoadPercent = 0f;
        foreach(Resource resourceType in ResourceInfo.AllResources){
            resourcesLoadPercent+=resourcesSpritesHandler[resourceType].PercentComplete;
        }
        //resourcesLoadPercent has 6 images with total 120 frames.
        //resourceSpriteHandler has 100 images
        //cardsJsonsHandler has 100 jsons so...
        resourcesLoadPercent = (resourcesLoadPercent/ResourceInfo.AllResources.Count())*0.16f;
        float cardsSpritesPercent = cardsSpriteHandler.PercentComplete*.42f;
        float cardsJsonsPercent = cardsJsonsHandler.PercentComplete*.42f;
        
        return resourcesLoadPercent+cardsSpritesPercent+cardsJsonsPercent;
    }
}
