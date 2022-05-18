using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodChooseMenu : MonoBehaviour
{
    public GodButton godButtonPrefab;

    private List<GodButton> godButtons = new List<GodButton>();

    private static readonly int MAX_MARKS_AMOUNT = 3;

    public VerticalLayoutGroup VerticalTiers;
    public HorizontalLayoutGroup TierPrefab;
    void Start(){
        Dictionary<int,List<GodDO>> tiers = new Dictionary<int, List<GodDO>>();
        foreach(GodDO god in Loader.allGods.gods){

            if(!tiers.Keys.Contains(god.tier)){
                tiers[god.tier] = new List<GodDO>();
            }
            tiers[god.tier].Add(god);

            GodButton godButton = Instantiate(godButtonPrefab,transform.position,transform.rotation);
            godButton.transform.SetParent(gameObject.transform);
            godButton.UpdateGod(god.name);
            godButtons.Add(godButton);
            if(Loader.settings.previous_summon!="" &&Loader.settings.previous_summon!=null && Loader.settings.previous_summon==god.name){
                godButton.ToggleCheck();
                godButton.GetComponent<Button>().interactable = false;
                Loader.ResetLastSummon();
                GameState.GodsMarked.Enqueue(god.name);
            }
            else if(god.defeated==1){
                godButton.GodButtonClicked += CheckmarkAdded;
            }
            else{
                godButton.GetComponent<Button>().interactable = false;
            }            
        }
        RepositionGodsButtons(tiers);
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

    void RepositionGodsButtons(Dictionary<int,List<GodDO>> tiers){
        foreach(int tier in tiers.Keys.OrderBy(tier=>-tier)){
            HorizontalLayoutGroup tierGroup = Instantiate(TierPrefab,new Vector3(),transform.rotation);
            tierGroup.transform.SetParent(VerticalTiers.transform);
            for(int i=0;i<tiers[tier].Count;i++){
                GodDO god = tiers[tier][tiers[tier].Count-1-i];
                GodButton currentGodButton = godButtons.Find(godButton=>godButton.godName==god.name);
                currentGodButton.transform.SetParent(tierGroup.transform);
            }
        }
    }
}
