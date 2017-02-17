using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    // Settings
    public bool DEBUG_InvincibleMode = false;
    public float TimeOnScreen;
	public List<string> NoteDetectionRange;
    public List<string> NotesRange; 
    public float BPM;
    public float TransitionGracePeriod;
    public float SustainedGracePeriod;

    // Dependencies
    public PlayerMovement Player;
    public GameObject platform;
    private PitchTester pt;

    // Private vars
    private List<Note> Song = new List<Note>(50);
    private List<BoxCollider2D> platforms = new List<BoxCollider2D>(50);

    private float currPos = 0;
    private float currTime = 0;
    private int currNoteIndex = 0;
    float spawnPosOffset = 0;
    float worldUnitsPerSec = 0;
    float worldUnitsPerBeat = 0;

    private bool isRespawning = false;
    private bool isChecking = true; 
    private bool isFalling = false;
    private bool isCorrect = true;
    private bool colIsFlashing = false;

    private Dictionary<string, float> notePosLookup;
    private Dictionary<string, Color> noteColorLookup;
    private List<string> NotesAllowed;
    
    internal float speedMultiplier = 1f;

    int deathIndex = 0;

    private float elapsedIncorrectTime = 0;


    // Use this for initialization
    void Start()
    {
        Player.GetComponent<SpriteRenderer>().color = Color.black;

        // Initialize vars
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        FillNoteColorLookup();
        FillNotePosLookup();
        FillNotesAllowed();

        pt.minFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[0], out pt.minFreq) ? pt.minFreq : 75;
        pt.maxFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[1], out pt.maxFreq) ? pt.maxFreq : 1075;

        float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                         - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
        worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
        worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
        spawnPosOffset = screenWidthInWorldUnits;

        

        // Fill song with first two notes
        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));	// Time * BPM / 60 gives us the number of beats
        SpawnPlatform(0);
        StartCoroutine("AddRandomNote");
        StartCoroutine("HandleJump");
    }

    // Update is called once per frame
    void Update()
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
        if (colIsFlashing)
        {
            return;
        }
        else if (string.IsNullOrEmpty(playerPitch))
        {
            Player.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else if (noteColorLookup.ContainsKey(playerPitch))
        {
            Player.GetComponent<SpriteRenderer>().color = noteColorLookup[playerPitch];
        }

        // Permit grace period
        if (!isChecking || DEBUG_InvincibleMode)
            return;

        // Ignore rests
        string targetNote = Song[currNoteIndex].name;
        if (targetNote == "REST")
            return;

        // Compare player pitch to target note
        //if (playerPitch != targetNote)
        if (string.IsNullOrEmpty(playerPitch) || playerPitch[0] != targetNote[0])
        {
            // If they just deviated from the pitch, start keeping track of the time.
			if (isCorrect) {
				elapsedIncorrectTime = 0;
				isCorrect = false;

				Player.GetComponent<SpriteRenderer> ().color = noteColorLookup [targetNote];
			}
            //Debug.Log("player pitch = " + playerPitch + ",\ntarget note = " + targetNote);
            // If they've stayed incorrect for long enough that it's probably not just noise, drop them.
            else if (elapsedIncorrectTime > SustainedGracePeriod) {
				Physics2D.IgnoreCollision (platforms [currNoteIndex], Player.GetComponent<Collider2D> ());
				isFalling = true;
			}
            // Add elapsed incorrect time
            else
			{
				Player.GetComponent<SpriteRenderer>().color = noteColorLookup[targetNote];
				elapsedIncorrectTime += Time.deltaTime;
			}	
        }
        else
            isCorrect = true;
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
        {
            if (Song[index].name == "REST") //If Rest, set Y of rest platform to previous note's Y
            {
                plat.transform.position = new Vector3(currPos + spawnPosOffset, Song[index - 1].yOffset);
            }
            else
            {
                plat.transform.position = new Vector3(currPos + spawnPosOffset, Song[index].yOffset);
            }

        }

        // But, bump it over to the right by half of the platform's width, so that it starts at the right spot
        plat.transform.position += Vector3.right * platWidth / 2;

        // Add it to our list of platforms
        platforms.Insert(index, plat.GetComponent<BoxCollider2D>());
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
            int lastNoteIndex = Song.Count - 1;
            string lastNoteName = ((Note)Song[lastNoteIndex]).name;
            string newNoteName = lastNoteName;
            while (newNoteName == lastNoteName)
            {
                newNoteName = NotesAllowed[UnityEngine.Random.Range(0, NotesAllowed.Count - 1)];
            }
            float newNoteDur = UnityEngine.Random.Range(1, 4);


            // Fill in note properties
            Note newNote = new Note(newNoteName, newNoteDur);
            newNote.yOffset = newNoteName == "REST" ? Song[lastNoteIndex].yOffset : notePosLookup[newNoteName];

            // Add note to song
            Song.Insert(lastNoteIndex + 1, newNote);
            ReaderWriter.WriteSong(Song, "test1.txt");

            // Spawn corresponding platform
            SpawnPlatform(lastNoteIndex + 1);

            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float delay = newNoteDur * 60 / BPM;
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator HandleJump()
    {
        while(true)
        {
            // Wait till end of note
            float dur = Song[currNoteIndex].duration * 60 / BPM;

            // Handle transition grace period
            Invoke("StartTransitionGracePeriod", dur - TransitionGracePeriod / 2);
            yield return new WaitForSeconds(Song[currNoteIndex].duration * 60 / BPM);


            // Handle jump
            if (!isFalling)
            {
                float jumpHeight = Song[currNoteIndex + 1].yOffset;// - Song[currNoteIndex].yOffset;
                float playerHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
                float platHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
                Player.transform.position = new Vector3(Player.transform.position.x, jumpHeight + playerHeight / 2 + platHeight / 2);
            }
           
            currNoteIndex++;
        }
    }

    void StartTransitionGracePeriod()
    {
        isChecking = false;
        Invoke("EndTransitionGracePeriod", TransitionGracePeriod);
    }

    void EndTransitionGracePeriod()
    {
        // This is a bit tricky:
        // Jumps need to enable and disable checking. But they should NOT override death grace periods!
        // So, a new bool (isRespawning) is necessary so that we can give respawning priority.
        if (!isRespawning)
            isChecking = true;
    }

    void EndDeathGracePeriod()
    {
        isRespawning = false;
        isChecking = true;
    }

    void FillNoteColorLookup()
    {
        noteColorLookup = new Dictionary<string, Color>();

        noteColorLookup.Add("E2", Color.red);
        noteColorLookup.Add("F2", new Color(1f, .5f, 0f));
        noteColorLookup.Add("G2", Color.yellow);
        noteColorLookup.Add("A2", Color.green);
        noteColorLookup.Add("B2", Color.cyan);
        noteColorLookup.Add("C3", Color.blue);
        noteColorLookup.Add("D3", new Color(.8f, .2f, .8f));
        noteColorLookup.Add("E3", Color.red);
        noteColorLookup.Add("F3", new Color(1f, .5f, 0f));
        noteColorLookup.Add("G3", Color.yellow);
        noteColorLookup.Add("A3", Color.green);
        noteColorLookup.Add("B3", Color.cyan);
        noteColorLookup.Add("C4", Color.blue);

        // Original list
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
        //End of original list

        noteColorLookup.Add("A5", Color.green);
        noteColorLookup.Add("B5", Color.cyan);
        noteColorLookup.Add("C6", Color.blue);
    }

    void FillNotePosLookup()
    {
        notePosLookup = new Dictionary<string, float>();

        notePosLookup.Add("E2", -7f);
        notePosLookup.Add("F2", -6.5f);
        notePosLookup.Add("G2", -6f);
        notePosLookup.Add("A2", -5.5f);
        notePosLookup.Add("B2", -5f);
        notePosLookup.Add("C3", -4.5f);
        notePosLookup.Add("D3", -4f);
        notePosLookup.Add("E3", -3.5f);
        notePosLookup.Add("F3", -3f);
        notePosLookup.Add("G3", -2.5f);
        notePosLookup.Add("A3", -2f);
        notePosLookup.Add("B3", -1.5f);
        notePosLookup.Add("C4", -1f);

        // Original list
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
        // End of original list.

        notePosLookup.Add("A5", 5f);
        notePosLookup.Add("B5", 5.5f);
        notePosLookup.Add("C6", 6f);
    }

    private void FillNotesAllowed()
    {
        NotesAllowed = new List<string>();
        NotesAllowed.Add("REST");

        float notesRangeMinPos; float notesRangeMaxPos;
        notesRangeMinPos = Player.NotePosLookup.TryGetValue(NotesRange[0], out notesRangeMinPos) ? notesRangeMinPos : -0.5f;
        notesRangeMaxPos = Player.NotePosLookup.TryGetValue(NotesRange[1], out notesRangeMaxPos) ? notesRangeMaxPos : 4.5f;
        foreach (string Note in Player.NotePosLookup.Keys)
        {
            if (Player.NotePosLookup[Note] >= notesRangeMinPos && Player.NotePosLookup[Note] <= notesRangeMaxPos)
            {
                NotesAllowed.Add(Note);
            }
        }
    }


    internal void RespawnPlayer()
    {
        isFalling = false;

        // Halt momentum of player
        Player.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        // Respawn player
        float playerHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
        float platformHeight = platform.GetComponent<SpriteRenderer>().bounds.size.y;
        Player.gameObject.transform.position = new Vector3(currPos, Song[currNoteIndex].yOffset + playerHeight/2 + platformHeight/2); 
        
        // Pay attention to collisions again
        Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>(), false);

        // Grant brief invulnerability
        float deathGracePeriod = 60 / BPM * 2;
        isChecking = false;
        isRespawning = true;
        Invoke("EndDeathGracePeriod", deathGracePeriod);
        IEnumerator coroutine = FlashColor(deathGracePeriod / 4);
        StartCoroutine(coroutine);
    }

    IEnumerator FlashColor(float interval)
    {
        for (int i=0; i<4; i++)
        {
            colIsFlashing = true;
            Player.GetComponent<SpriteRenderer>().color = Color.white;
            Invoke("EndColorFlash", interval/3);
            yield return new WaitForSeconds(interval);
        }
    }
    
    void EndColorFlash()
    {
        colIsFlashing = false;
    }
}
