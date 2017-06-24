using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MidiJack;

public class ABST_MidiCC : MonoBehaviour {

    public int channel;
    public int id;
    public int value;

    public TextMeshProUGUI idText, valueText;
    
	// Use this for initialization
	void Start () {
        idText.text = id.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        valueText.text = MidiMaster.GetKnob(id).ToString();
    }
}
