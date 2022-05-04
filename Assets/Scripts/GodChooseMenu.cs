using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GodChooseMenu : MonoBehaviour
{
    public GodButton godButtonPrefab;

    private List<GodButton> godButtons = new List<GodButton>();

    private static readonly int MAX_MARKS_AMOUNT = 3;
    void Start(){
        foreach(GodDO god in Loader.allGods.gods){
            GodButton godButton = Instantiate(godButtonPrefab,transform.position,transform.rotation);
            godButton.transform.SetParent(gameObject.transform);
            godButton.UpdateGod(god.name);
            godButtons.Add(godButton);
            ScaleButton(godButton);
            if(god.defeated==1){
                godButton.GodButtonClicked += CheckmarkAdded;
            }
            else{
                godButton.GetComponent<Button>().interactable = false;
            }
            
        }
        StartCoroutine(RepositionGodsButtons());
    }

    void CheckmarkAdded(GodButton sender){
        if(!sender.Checked){
            if(GameState.GodsMarked.Contains(sender.godName)){
                GameState.GodsMarked = new Queue<string>(GameState.GodsMarked.Where(x => x != sender.godName));
                return;
            }
        }
        GameState.GodsMarked.Enqueue(sender.godName);
        while(GameState.GodsMarked.Count>MAX_MARKS_AMOUNT){
            godButtons
                .Find(godButton=>godButton.godName==GameState.GodsMarked.Dequeue())
                .ToggleCheck();
        }
    }

    IEnumerator RepositionGodsButtons(){
        while(true){
            RepositionGodsButtonsOnce();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void RepositionGodsButtonsOnce(){
        RectTransform menuRect = GetComponent<RectTransform>();
        Dictionary<int,List<GodDO>> tiers = new Dictionary<int, List<GodDO>>();
        foreach(GodDO god in Loader.allGods.gods){
            if(!tiers.Keys.Contains(god.tier)){
                tiers[god.tier] = new List<GodDO>();
            }
            tiers[god.tier].Add(god);
        }

        float topPadding = menuRect.rect.height/24f;
        float bottomBoundary = transform.position.y-topPadding-((tiers.Keys.Count)/2f)*godButtons[0].GetComponent<RectTransform>().rect.height*1.03f;
        foreach(int tier in tiers.Keys.OrderBy(tier=>tier)){
            float yPosition = bottomBoundary + ((tier-1)*1.3f)*godButtons[0].GetComponent<RectTransform>().rect.height*1.03f;
            float leftBoundary = transform.position.x-((tiers[tier].Count-1)/2f)*godButtons[0].GetComponent<RectTransform>().rect.width*1.3f;
            for(int i=0;i<tiers[tier].Count;i++){
                GodDO god = tiers[tier][tiers[tier].Count-1-i];
                GodButton currentGodButton = godButtons.Find(godButton=>godButton.godName==god.name);
                float xPosition = leftBoundary+i*1.3f*currentGodButton.GetComponent<RectTransform>().rect.width;
                Vector3 godPosition = new Vector3(xPosition,yPosition,transform.position.z);
                currentGodButton.transform.position = godPosition;
                //ScaleButton(currentGodButton);
            }
        }
    }

    private void ScaleButton(GodButton godButton){
        RectTransform menuRectTransform = GetComponent<RectTransform>();
        Rect godButtonRect = godButton.GetComponent<RectTransform>().rect;
        float newScale = (menuRectTransform.rect.height/4f)/godButtonRect.height;

        godButton.transform.localScale = godButton.transform.localScale*newScale;
    }
}
