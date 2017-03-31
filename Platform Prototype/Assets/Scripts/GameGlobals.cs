using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGlobals : MonoBehaviour {

    // Settings
    public static GameGlobals GlobalInstance;
    public bool isTextActive = true;
	public float TimeOnScreen = 5f;
    public string[] NotesRange = new string[] {"C3", "C5"};
    public float BPM = 60, TransitionGracePeriod = 0.5f, SustainedGracePeriod = 0.8f, speedMult = 1f, scrollingInterpolation = 0.01f;
    public int LeniencyRange = 1;
    public bool SongMode = false;
    public string selectedSong = "text1.txt";
    public float MaxTimeBetweenRests = 4f;
    public bool WritingOn = false;
	public bool bassClefMode = false;

    private string LowestNote = "E2", HighestNote = "C6";
    private int highNoteIndex = 25, lowNoteIndex = 0;

    internal List<string> notes = new List<string>
    {
        "E2", "F2", "G2", "A2", "B2", "C3", "D3", "E3", "F3", "G3", "A3", "B3", "C4", "D4", "E4",
        "F4", "G4", "B4", "C5", "D5", "E5", "F5", "G5", "A5", "B5", "C6"
    };

    void Awake()
    {
        if (GlobalInstance == null)
        {
            DontDestroyOnLoad(gameObject);
            GlobalInstance = this;
            // Add other initilization items here.
        }
        else if (GlobalInstance != this)
        {
            // Add modifiers or variable updates here.
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Update the tolerance level.
    /// </summary>
    public void changeTolerance(float val)
    {
        LeniencyRange = (int)val;
    }

    /// <summary>
    /// Update the grace period.
    /// </summary>
    public void changeGracePeriod (float val)
    {
        TransitionGracePeriod = val;
    }

    /// <summary>
    /// Changes the density of each note in Infinite Mode.
    /// </summary>
    public void changeNoteDensity(float val)
    {

    } 

    /// <summary>
    /// Changes the scroll speed.
    /// </summary>
    public void changeScrollSpeed(float val)
    {
		TimeOnScreen = val; 
    }

    /// <summary>
    /// Changes the time between rests.
    /// </summary>
    public void changeTimeBetweenRest(float val)
    {
        MaxTimeBetweenRests = val;
    }

    /// <summary>
    /// Change the lowest note used.
    /// </summary>
    /// <param name="val">Index value of note desired, starting from E2 - B5 (0-24 inclusive).</param>
    public void changeLowestNote(float val)
    {
        //Debug.Log("Low Index " + val);
        int trueVal = (int)val;
        if (trueVal >= highNoteIndex)
        {
            highNoteIndex = trueVal + 1;
            HighestNote = notes[lowNoteIndex];
        }
        lowNoteIndex = (int)val;
        LowestNote = notes[lowNoteIndex];
        NotesRange = new string[] { LowestNote, HighestNote };
        //Debug.Log("Low Note " + LowestNote);
    }

    /// <summary>
    /// Change the highest note used,
    /// </summary>
    /// <param name="val">Index value of note desired, starting from F2 - C6 (1-25 inclusive).</param>
    public void changeHighestNote(float val)
    {
        //Debug.Log("High Index " + val);
        int trueVal = (int)val;
        if (trueVal <= lowNoteIndex)
        {
            lowNoteIndex = trueVal - 1;
            LowestNote = notes[lowNoteIndex];
        }
        highNoteIndex = trueVal;
        HighestNote = notes[highNoteIndex];
        NotesRange = new string[] { LowestNote, HighestNote };
        //Debug.Log("High Note " + HighestNote);
    }

    /// <summary>
    /// Sets game to infinite mode
    /// </summary>
    public void infiniteModeEnabled()
    {
        SongMode = false;
    }

    /// <summary>
    /// Sets game to song mode.
    /// </summary>
    public void songModeEnabled()
    {
        SongMode = true;
    }

    public void changeTextEnabled()
    {
        isTextActive = !isTextActive;
    }

    /// <summary>
    /// Change to the selected song file.
    /// </summary>
    /// <param name="filename">Filename to the desired song.</param>
    public void changeSelectedSong(string filename)
    {
        selectedSong = filename;
    }

    public int getLowNoteIndex()
    {
        return lowNoteIndex;
    }

    public int getHighNoteIndex()
    {
        return highNoteIndex;
    }
}
