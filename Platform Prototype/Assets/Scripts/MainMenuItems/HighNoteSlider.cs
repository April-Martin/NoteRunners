using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighNoteSlider : MonoBehaviour {

    private Slider slider;

    // Use this for initialization
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void changeValue()
    {
        slider.value = GameGlobals.GlobalInstance.getHighNoteIndex();
    }
}
