using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    public Text Tolerance;
    public Text GracePeriod;
    public Text InfNoteDen;
    public Text ScrollSpeed;
    public Text TBtwnRests;
    public Text LowNote;
    public Text HighNote;
    public Button SongMode;
    public Button InfMode;
    public GameObject SongSelectPanel;
    public Button BassClef;
    public Button TrebleClef;
    public HighNoteSlider highSlider;
    public LowNoteSlider lowSlider;
    public Image runnerDisplay;
    public Image redIndicator, greenIndicator, blueIndicator;
    public Slider redSlider, blueSlider, greenSlider;

    private float plrRed = 1f, plrGrn = 1f, plrBlu = 1f;
    private AudioCuePlayer player;

    void Awake()
    {
        Tolerance.text = GameGlobals.GlobalInstance.LeniencyRange + "";
        GracePeriod.text = GameGlobals.GlobalInstance.TransitionGracePeriod + "";
        InfNoteDen.text = GameGlobals.GlobalInstance.NoteDensity + "";
        ScrollSpeed.text = GameGlobals.GlobalInstance.TimeOnScreen + "";
        TBtwnRests.text = GameGlobals.GlobalInstance.MaxTimeBetweenRests + "";
        LowNote.text = GameGlobals.GlobalInstance.getLowNote();
        HighNote.text = GameGlobals.GlobalInstance.getHighNote();
        SongMode.interactable = !GameGlobals.GlobalInstance.SongMode;
        InfMode.interactable = GameGlobals.GlobalInstance.SongMode;
        BassClef.interactable = !GameGlobals.GlobalInstance.bassClefMode;
        TrebleClef.interactable = GameGlobals.GlobalInstance.bassClefMode;
        plrRed = GameGlobals.GlobalInstance.plrRed;
        plrGrn = GameGlobals.GlobalInstance.plrGrn;
        plrBlu = GameGlobals.GlobalInstance.plrBlu;
        redSlider.value = plrRed;
        greenSlider.value = plrGrn;
        blueSlider.value = plrBlu;
        changeColorIndicators();

        player = GetComponent<AudioCuePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        Tolerance.text = GameGlobals.GlobalInstance.LeniencyRange + "";
        GracePeriod.text = GameGlobals.GlobalInstance.TransitionGracePeriod + "";
        InfNoteDen.text = GameGlobals.GlobalInstance.NoteDensity + "";
        ScrollSpeed.text = GameGlobals.GlobalInstance.TimeOnScreen + "";
        TBtwnRests.text = GameGlobals.GlobalInstance.MaxTimeBetweenRests + "";
        LowNote.text = GameGlobals.GlobalInstance.getLowNote();
        HighNote.text = GameGlobals.GlobalInstance.getHighNote();
        SongMode.interactable = !GameGlobals.GlobalInstance.SongMode;
        InfMode.interactable = GameGlobals.GlobalInstance.SongMode;
        BassClef.interactable = !GameGlobals.GlobalInstance.bassClefMode;
        TrebleClef.interactable = GameGlobals.GlobalInstance.bassClefMode;
        changeColorIndicators();
    }

    /// <summary>
    /// Set bass clef mode to true
    /// </summary>
    public void setBassClef()
    {
        GameGlobals.GlobalInstance.bassClefMode = true;
        UpdateSliders();
    }

    /// <summary>
    /// Set bass clef mode to false
    /// </summary>
    public void setTrebleClef()
    {
        GameGlobals.GlobalInstance.bassClefMode = false;
        UpdateSliders();
    }

    /// <summary>
    /// Play lowest note in range
    /// </summary>
    public void playLowestNote()
    {
        player.PlayNote(GameGlobals.GlobalInstance.NotesRange[0], 1);
    }

    /// <summary>
    /// Play highest note in range
    /// </summary>
    public void playHighestNote()
    {
        player.PlayNote(GameGlobals.GlobalInstance.NotesRange[1], 1);
    }

    private void UpdateSliders()
    {
        lowSlider.UpdateClef();
        highSlider.UpdateClef();

        float yPos = (GameGlobals.GlobalInstance.bassClefMode) ? (BassClef.transform.position.y) : (TrebleClef.transform.position.y);
        Transform sliderPos = lowSlider.transform.parent.parent;
        sliderPos.position = new Vector3(sliderPos.position.x, yPos, sliderPos.position.z);
        sliderPos = highSlider.transform.parent.parent;
        sliderPos.position = new Vector3(sliderPos.position.x, yPos, sliderPos.position.z);
    }

    /// <summary>
    /// Update the tolerance level.
    /// </summary>
    public void changeTolerance(float val)
    {
        GameGlobals.GlobalInstance.changeTolerance(val);
    }

    /// <summary>
    /// Update the grace period.
    /// </summary>
    public void changeGracePeriod(float val)
    {
        GameGlobals.GlobalInstance.changeGracePeriod(val);
    }

    /// <summary>
    /// Changes the density of each note in Infinite Mode.
    /// </summary>
    public void changeNoteDensity(float val)
    {
        GameGlobals.GlobalInstance.changeNoteDensity(val);
    }

    /// <summary>
    /// Changes the scroll speed.
    /// </summary>
    public void changeScrollSpeed(float val)
    {
        GameGlobals.GlobalInstance.changeScrollSpeed(val);
    }

    /// <summary>
    /// Changes the time between rests.
    /// </summary>
    public void changeTimeBetweenRest(float val)
    {
        GameGlobals.GlobalInstance.changeTimeBetweenRest(val);
    }

    /// <summary>
    /// Change the lowest note used.
    /// </summary>
    /// <param name="val">Index value of note desired, starting from E2 - B5 (0-24 inclusive).</param>
    public void changeLowestNote(float val)
    {
        GameGlobals.GlobalInstance.changeLowestNote(val);
        highSlider.changeValue();
    }

    /// <summary>
    /// Change the highest note used,
    /// </summary>
    /// <param name="val">Index value of note desired, starting from F2 - C6 (1-25 inclusive).</param>
    public void changeHighestNote(float val)
    {
        GameGlobals.GlobalInstance.changeHighestNote(val);
        lowSlider.changeValue();
    }

    /// <summary>
    /// Sets game to infinite mode
    /// </summary>
    public void infiniteModeEnabled()
    {
        GameGlobals.GlobalInstance.infiniteModeEnabled();
    }

    /// <summary>
    /// Sets game to song mode.
    /// </summary>
    public void songModeEnabled()
    {
        GameGlobals.GlobalInstance.songModeEnabled();
    }

    public void changeTextEnabled()
    {
        GameGlobals.GlobalInstance.changeTextEnabled();
    }

    /// <summary>
    /// Change to the selected song file.
    /// </summary>
    /// <param name="filename">Filename to the desired song.</param>
    public void changeSelectedSong(string filename)
    {
        GameGlobals.GlobalInstance.changeSelectedSong(filename);
    }

    public void changeRunnerColorRed(float val)
    {
        if (val < 0 || val > 1)
            return;
        plrRed = val;
        GameGlobals.GlobalInstance.setPlayerColor(plrRed, plrGrn, plrBlu);
    }

    public void changeRunnerColorGreen(float val)
    {
        if (val < 0 || val > 255)
            return;
        plrGrn = val;
        GameGlobals.GlobalInstance.setPlayerColor(plrRed, plrGrn, plrBlu);
    }

    public void changeRunnerColorBlue(float val)
    {
        if (val < 0 || val > 255)
            return;
        plrBlu = val;
        GameGlobals.GlobalInstance.setPlayerColor(plrRed, plrGrn, plrBlu);
    }

    public void songModeFix()
    {
        if (GameGlobals.GlobalInstance.SongMode)
        {
            SongSelectPanel.SetActive(true);
        }
    }


    private void changeColorIndicators()
    {
        redIndicator.color = new Color(1f, plrGrn, plrBlu, 1f);
        greenIndicator.color = new Color(plrRed, 1f, plrBlu, 1f);
        blueIndicator.color = new Color(plrRed, plrGrn, 1f, 1f);
        runnerDisplay.color = new Color(plrRed, plrGrn, plrBlu, 1f);
    }
}
