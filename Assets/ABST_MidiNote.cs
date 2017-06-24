using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;
using TMPro;
using UnityEngine.UI;

public class ABST_MidiNote : MonoBehaviour {

    public int channel;
    public int id;

    MidiDriver.NoteOnDelegate noteOnFunc;
    MidiDriver.NoteOffDelegate noteOffFunc;

    public TextMeshProUGUI idText, valueText;

    // Use this for initialization
    void Start () {
        noteOnFunc = noteOnHandler;
        noteOffFunc = noteOffHandler;
    }
	
	// Update is called once per frame
	void Update () {
        var keyPlay = MidiMaster.GetKey(id);

        idText.text = id.ToString();        
        valueText.text = keyPlay.ToString();
        GetComponent<Image>().color = new Color(keyPlay, keyPlay, keyPlay);
    }
    
    void noteOnHandler(MidiChannel channel, int id, float value)
    {
        var message = string.Format("ABST Note On: CH {0} ID {1} VAL {2}", channel.ToString(), id, value);
        GetComponent<Image>().color = new Color(value, 255, value);
        Debug.Log(message);
    }

    void noteOffHandler(MidiChannel channel, int id)
    {
        var message = string.Format("ABST Note Off: CH {0} ID {1}", channel.ToString(), id);
        GetComponent<Image>().color = Color.red;
        Debug.Log(message);
    }
}
