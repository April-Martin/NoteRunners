using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseIndicator : MonoBehaviour {

    private PitchTester pt;
    private Image img;

	// Use this for initialization
	void Start () {
        pt = GameObject.Find("PitchTester").GetComponent<PitchTester>();
        img = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
        if (pt.MainNote == "")
           img.enabled = false;
        else
            img.enabled = true;
	}
}
