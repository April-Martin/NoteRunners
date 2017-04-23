/* Note: 
 * Highest note for bass clef = C4
 * Highest note for treble clef = A5
 * */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighNoteSlider : MonoBehaviour
{

    private Slider slider;

    // Use this for initialization
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = GameGlobals.GlobalInstance.getHighNoteIndex();
        UpdateClef();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateClef()
    {
        if (GameGlobals.GlobalInstance.bassClefMode)
        {
            slider.maxValue = 12;
            slider.minValue = 1;
        }
        else
        {
            slider.maxValue = 26;
            slider.minValue = 8;
        }
    }

    public void changeValue()
    {
        slider.value = GameGlobals.GlobalInstance.getHighNoteIndex();
    }
}
