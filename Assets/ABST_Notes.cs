using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABST_Notes : MonoBehaviour
{

    public int numNotes = 127;
    public GameObject notePrefab;

    // Use this for initialization
    void Start()
    {
        for (int i = 1; i < numNotes; i++)
        {
            var newObj = Instantiate(notePrefab, transform);
            newObj.GetComponent<ABST_MidiNote>().id = i;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
