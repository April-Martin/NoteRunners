/* Note: 
 * Lowest note for bass clef = E2
 * Lowest note for treble clef = E3
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowNoteSlider : MonoBehaviour
{

    private Slider slider;

    // Use this for initialization
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = GameGlobals.GlobalInstance.getLowNoteIndex();
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
            slider.maxValue = 11;
            slider.minValue = 0;
        }
        else
        {

            slider.maxValue = 25;
            slider.minValue = 7;
        }
    }

    public void changeValue()
    {
        slider.value = GameGlobals.GlobalInstance.getLowNoteIndex();
    }
}
