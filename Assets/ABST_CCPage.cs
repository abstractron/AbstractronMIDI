using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABST_CCPage : MonoBehaviour {

    public int numControls = 127;
    public GameObject knobPrefab;

	// Use this for initialization
	void Start () {
		for (int i=1; i < numControls; i++)
        {
            var newObj = Instantiate(knobPrefab, transform);
            newObj.GetComponent<ABST_MidiCC>().id = i;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
