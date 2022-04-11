using UnityEngine;
using System.Linq;
public class Alert : MonoBehaviour
{
    public int framesBetweenBlinks = 15;
    private int timesBlinked = 0;
    private int currentFrame = 0;
    public bool isOn = false;

    public Mode[] ActiveOnModes = new Mode[]{Mode.Cult};
    public AlertCondition condition;
    // Start is called before the first frame update
    public void Start()
    {
        framesBetweenBlinks = Mathf.RoundToInt((1f/Time.deltaTime)/3f);
        backToInitState();
    }

    // Update is called once per frame
    void Update()
    {
        if(ActiveOnModes.Contains(GameState.GameMode)){
            if(isOn){
                if(timesBlinked<3){
                    currentFrame++;
                    if(currentFrame==framesBetweenBlinks){
                        toFront();
                        currentFrame = 0;
                        timesBlinked+=1;
                    }
                    else if(currentFrame==framesBetweenBlinks/2){
                        toBack();
                    }
                }
                if(!ResourceCard.dragging &&!condition.Check()){
                    backToInitState();
                }
            }
            else if(!ResourceCard.dragging && condition.Check()){
                isOn = true;
            }
        }
    }

    public void TurnOn(){
        isOn = true;
        currentFrame = framesBetweenBlinks/2;
    }

    public void TurnOff(){
        backToInitState();
    }
    
    private void toFront(){
        GetComponent<SpriteRenderer>().sortingLayerName = "hand";
    }

    private void toBack(){
        GetComponent<SpriteRenderer>().sortingLayerName = "behindScene";
    }
    private void backToInitState(){
        toBack();
        isOn = false;
        timesBlinked = 0;
        currentFrame = 0;
    }
}
