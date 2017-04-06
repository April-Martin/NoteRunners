using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour {

    public Text Tolerance;
    public Text GracePeriod;
    public Text InfNoteDen;
    public Text ScrollSpeed;
    public Text TBtwnRests;
    public Text LowNote;
    public Text HighNote;


	void Start () {
        Tolerance.text = GameGlobals.GlobalInstance.LeniencyRange + "";
        GracePeriod.text = GameGlobals.GlobalInstance.TransitionGracePeriod + "";
        InfNoteDen.text = GameGlobals.GlobalInstance.NoteDensity + "";
        ScrollSpeed.text = GameGlobals.GlobalInstance.TimeOnScreen + "";
        TBtwnRests.text = GameGlobals.GlobalInstance.MaxTimeBetweenRests + "";
        LowNote.text = GameGlobals.GlobalInstance.getLowNote();
        HighNote.text = GameGlobals.GlobalInstance.getHighNote();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Tolerance.text = GameGlobals.GlobalInstance.LeniencyRange + "";
        GracePeriod.text = GameGlobals.GlobalInstance.TransitionGracePeriod + "";
        InfNoteDen.text = GameGlobals.GlobalInstance.NoteDensity + "";
        ScrollSpeed.text = GameGlobals.GlobalInstance.TimeOnScreen + "";
        TBtwnRests.text = GameGlobals.GlobalInstance.MaxTimeBetweenRests + "";
        LowNote.text = GameGlobals.GlobalInstance.getLowNote();
        HighNote.text = GameGlobals.GlobalInstance.getHighNote();
    }
}
