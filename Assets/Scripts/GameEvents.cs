
using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;
    // Start is called before the first frame update
    public void Awake(){
        instance = this;    
    }

    public event Action onDeckClicked;
    public void deckClicked(){
        if (onDeckClicked!=null){
            onDeckClicked();
        }
    }
}   
