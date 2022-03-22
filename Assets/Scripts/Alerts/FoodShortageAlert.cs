using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodShortageAlert : Alert
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        condition = new NoFood();
        
    }
}
