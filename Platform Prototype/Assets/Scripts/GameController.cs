using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    // Settings
    public float TimeOnScreen;
    public string[] NotesAllowed;
    public float BPM;
    public float GracePeriod;

    // Dependencies
    public PlayerMovement Player;
    public GameObject platform;

    // Private vars
    private List<Note> Song = new List<Note>(50);
    private List<BoxCollider2D> platforms = new List<BoxCollider2D>(50);
    private float currPos = 0;
    private float currTime = 0;
    private int currNoteIndex = 0;
    float spawnPosOffset = 0;
    float worldUnitsPerSec = 0;
    float worldUnitsPerBeat = 0;

    private bool isJumping = false;
    private bool isChecking = true;

    private Dictionary<string, float> notePosLookup;
    private Dictionary<string, Color> noteColorLookup;
    private PitchTester pt;

    internal float speedMultiplier = 1f;



	// Use this for initialization
	void Start () 
    {
        // Initialize vars
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        FillNoteColorLookup();
        FillNotePosLookup();
        float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                         - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
        worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
        worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
        spawnPosOffset = screenWidthInWorldUnits;

        // Fill song with first two notes
        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));
        SpawnPlatform(0);
        StartCoroutine("AddRandomNote");
        StartCoroutine("HandleJump");
	}
	
	// Update is called once per frame
	void Update ()
    {
        CheckPitch();   

        currTime += Time.deltaTime;
        currPos += Time.deltaTime * worldUnitsPerSec;
        movePlayerAndCamera();
        
	}

    void CheckPitch()
    {
        // Update player color
        string playerPitch = pt.MainNote;
        if (playerPitch == "")
            Player.GetComponent<SpriteRenderer>().color = Color.black;
        else
            Player.GetComponent<SpriteRenderer>().color = noteColorLookup[playerPitch];

        // Permit grace period
        if (!isChecking)
            return;

        // Ignore rests
        string targetNote = Song[currNoteIndex].name;
        if (targetNote == "REST")
            return;

        // Compare player pitch to target note
        if ( playerPitch != targetNote)
        {
            Debug.Log("player pitch = " + playerPitch + ",\ntarget note = " + targetNote);
            Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>());
        }
    }

    void SpawnPlatform(int index)
    {
        GameObject plat = Instantiate(platform);

        // Set the platform's color
        plat.GetComponent<SpriteRenderer>().color = noteColorLookup[Song[index].name];

        // Resize the platform's width so it matches the note's duration
        float platWidth = Song[index].duration * worldUnitsPerBeat;
        float startWidth = plat.GetComponent<SpriteRenderer>().bounds.size.x;
        plat.transform.localScale = new Vector3(platWidth / startWidth, plat.transform.localScale.y, 1);

        // Set it at the position corresponding to the note's start time.
        if (index != 0)
            plat.transform.position = new Vector3(currPos + spawnPosOffset, Song[index].yOffset);

        // But, bump it over to the right by half of the platform's width, so that it starts at the right spot
        plat.transform.position += Vector3.right * platWidth / 2;

        // Add it to our list of platforms
        platforms.Insert(index, plat.GetComponent<BoxCollider2D>());
    }


    void movePlayerAndCamera()
    {
        Camera.main.transform.position = new Vector3(currPos, Camera.main.transform.position.y, Camera.main.transform.position.z);
        if (!isJumping) 
            Player.transform.position = new Vector3(currPos, Player.transform.position.y);
    }

    IEnumerator AddRandomNote()
    {
        while (true)
        {
            // Randomize note name
            int lastNoteIndex = Song.Count - 1;
            string lastNoteName = ((Note)Song[lastNoteIndex]).name;
            string newNoteName = lastNoteName;
            while (newNoteName == lastNoteName)
            {
                newNoteName = NotesAllowed[Random.Range(0, NotesAllowed.Length)];
            }
            float newNoteDur = Random.Range(1, 4);


            // Fill in note properties
            Note newNote = new Note(newNoteName, newNoteDur);
            newNote.yOffset = notePosLookup[newNoteName];
            newNote.actualTime = ((Note)Song[lastNoteIndex]).actualTime
                                + ((Note)Song[lastNoteIndex]).duration * 60 / BPM;

            // Add note to song
            Song.Insert(lastNoteIndex + 1, newNote);

            // Spawn corresponding platform
            SpawnPlatform(lastNoteIndex + 1);

            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float delay = newNoteDur * 60 / BPM;
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator HandleJump()
    {
        while (true)
        {
            // Wait till end of note
            float dur = Song[currNoteIndex].duration * 60 / BPM;
            Invoke("StartGracePeriod", dur - GracePeriod/2);
            yield return new WaitForSeconds(Song[currNoteIndex].duration * 60 / BPM);

            // Handle jump
            float jumpHeight = Song[currNoteIndex+1].yOffset - Song[currNoteIndex].yOffset;
            Player.transform.position += Vector3.up * jumpHeight;
            currNoteIndex++;
        }
    }

    void StartGracePeriod()
    {
        isChecking = false;
        Invoke("EndGracePeriod", GracePeriod);
    }

    void EndGracePeriod()
    {
        isChecking = true;
    }

    void FillNoteColorLookup()
    {
        noteColorLookup = new Dictionary<string, Color>();
        noteColorLookup.Add("D4", new Color(.8f, .2f, .8f));
        noteColorLookup.Add("E4", Color.red);
        noteColorLookup.Add("F4", new Color(1f, .5f, 0f));
        noteColorLookup.Add("G4", Color.yellow);
        noteColorLookup.Add("A4", Color.green);
        noteColorLookup.Add("B4", Color.cyan);
        noteColorLookup.Add("C5", Color.blue);
        noteColorLookup.Add("D5", new Color(.8f, .2f, .8f));
        noteColorLookup.Add("E5", Color.red);
        noteColorLookup.Add("F5", new Color(1f, .5f, 0f));
        noteColorLookup.Add("G5", Color.yellow);
        noteColorLookup.Add("REST", Color.black);
    }

    void FillNotePosLookup()
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
