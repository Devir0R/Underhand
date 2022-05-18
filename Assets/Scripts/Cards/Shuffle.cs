using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuffle : MonoBehaviour
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
public class ShuffleDO{
    public int lowerbound;
    public int upperbound;
	public List<int> specificids;
	public int allowsdupes;
	public int numcards;
}
