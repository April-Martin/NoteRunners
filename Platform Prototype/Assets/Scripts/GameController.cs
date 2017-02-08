using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    // Settings
    public float TimeOnScreen;
    public string[] NotesAllowed;
    public float BPM;

    // Dependencies
    public PlayerMovement Player;
    public GameObject platform;

    // Private vars
  //  public LinkedList<Note> Song = new LinkedList<Note>();
    public List<Note> Song = new List<Note>(50);
    private float currPos = 0;
    private float currTime = 0;
    private int currNoteIndex = 0;
    float spawnPosOffset = 0;
    float worldUnitsPerSec = 0;
    float worldUnitsPerBeat = 0;

    private Dictionary<string, float> notePosLookup;
    private FrequencyGuide freqGuide = new FrequencyGuide();

    internal float speedMultiplier = 1f;



	// Use this for initialization
	void Start () 
    {
        // Initialize vars
        FillNoteLookup();
        float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                         - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
        worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
        worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
        spawnPosOffset = screenWidthInWorldUnits;

        // Fill song with first two notes
        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));
        SpawnPlatform(0);
        StartCoroutine("AddRandomNote");
	}
	
	// Update is called once per frame
	void Update ()
    {
        currTime += Time.deltaTime;
        currPos += Time.deltaTime * worldUnitsPerSec;
        movePlayerAndCamera();
	}

    void SpawnPlatform(int index)
    {
        GameObject plat = Instantiate(platform);

        // Resize the platform's width so it matches the note's duration
        float platWidth = Song[index].duration * worldUnitsPerBeat;
        float startWidth = plat.GetComponent<SpriteRenderer>().bounds.size.x;
        plat.transform.localScale = new Vector3(platWidth / startWidth, plat.transform.localScale.y, 1);

        // Set it at the position corresponding to the note's start time.
        if (index != 0)
            plat.transform.position = new Vector3(currPos + spawnPosOffset, Song[index].yOffset);

        // But, bump it over to the right by half of the platform's width, so that it starts at the right spot
        plat.transform.position += Vector3.right * platWidth / 2;

    }


    void movePlayerAndCamera()
    {
        Camera.main.transform.position = new Vector3(currPos, Camera.main.transform.position.y, Camera.main.transform.position.z);
        Player.transform.position = new Vector3(currPos, Player.transform.position.y);
    }

    IEnumerator AddRandomNote()
    {
        while (true)
        {
            // Randomize note name
            string currNoteName = ((Note)Song[currNoteIndex]).name;
            string newNoteName = currNoteName;
            while (newNoteName == currNoteName)
            {
                newNoteName = NotesAllowed[Random.Range(0, NotesAllowed.Length)];
            }
            float newNoteDur = Random.Range(1, 4);

            // Fill in note properties
            Note newNote = new Note(newNoteName, newNoteDur);
            newNote.frequency = freqGuide.GetFreq(newNoteName);
            newNote.yOffset = notePosLookup[newNoteName];
            newNote.actualTime = ((Note)Song[currNoteIndex]).actualTime
                                + ((Note)Song[currNoteIndex]).duration * 60 / BPM;

            // Add note to song
            Song.Insert(++currNoteIndex, newNote);

            // Spawn corresponding platform
            SpawnPlatform(currNoteIndex);

            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float delay = newNoteDur * 60 / BPM;
            yield return new WaitForSeconds(delay);
        }
    }

    void FillNoteLookup()
    {
        notePosLookup = new Dictionary<string, float>();
        notePosLookup.Add("D4", -.5f);
        notePosLookup.Add("E4", 0f);
        notePosLookup.Add("F4", .5f);
        notePosLookup.Add("G4", 1f);
        notePosLookup.Add("A4", 1.5f);
        notePosLookup.Add("B4", 2f);
        notePosLookup.Add("C5", 2.5f);
        notePosLookup.Add("D5", 3f);
        notePosLookup.Add("E5", 3.5f);
        notePosLookup.Add("F5", 4f);
        notePosLookup.Add("G5", 4.5f);
        notePosLookup.Add("REST", 0);
	}



}
