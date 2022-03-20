using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewards : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class RewardsDO{
    public int relic;
	public int money;
	public int cultist;
	public int food;
	public int prisoner;
	public int suspicion;
}
