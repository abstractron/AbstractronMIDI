using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ABST_ConfigMenu : MonoBehaviour {

    public string currentPage = "CC";
    public GameObject[] pages;

	// Use this for initialization
	void Start () {
		if (pages.Length < 1)
        {
            Debug.Log("Hey, no pages in config menu dummy");
        }
        GetComponentsInChildren<Button>()[0].Select();
	}

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSidebarClick(string pageName)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i].name != pageName)
            {
                pages[i].SetActive(false);
            }
            else
            {
                pages[i].SetActive(true);
            }
        }
    }
}
