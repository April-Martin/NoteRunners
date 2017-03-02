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
	public int LeniencyRange = 0;
    public bool SongMode = false;
    public string filename;

    // Dependencies
    public PlayerMovement Player;
    public GameObject platform, platformText;
    private PitchTester pt;

    // Private vars
    private List<Note> Song = new List<Note>(50);
    private List<BoxCollider2D> platforms = new List<BoxCollider2D>(50);
	private List<TextMesh> platText = new List<TextMesh> (50);

    private float currPos = 0;
    private float currTime = 0;
    private int currNoteIndex = 0;
    private int lastSpawnedNoteIndex = 0;
    float spawnPosOffset = 0;
    float worldUnitsPerSec = 0;
    float worldUnitsPerBeat = 0;

    private bool isRespawning = false;
    private bool isChecking = true;
    private bool isFalling = false;
    private bool isCorrect = true;
    private bool colIsFlashing = false;

    internal Dictionary<string, float> notePosLookup;
    internal Dictionary<string, Color> noteColorLookup;
	private FrequencyGuide fg;

    private List<string> NotesAllowed;

    public float speedMult = 1f;
    public float scrollingInterpolation = 0.01f;

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
		fg = new FrequencyGuide ();


        pt.minFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[0], out pt.minFreq) ? pt.minFreq : 75;
        pt.maxFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[1], out pt.maxFreq) ? pt.maxFreq : 1075;

        float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                         - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
        worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
        worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
        spawnPosOffset = screenWidthInWorldUnits;

        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));	// Time * BPM / 60 gives us the number of beats
        SpawnPlatform(0);

        // If infinite mode:
        if (!SongMode)
        {
            StartCoroutine("AddRandomNote");
            StartCoroutine("HandleJump");
        }

        // If song mode:
        else
        {
            // Read information from file into Song array
            ReaderWriter.ReadSong(ref Song, filename, ref BPM);
            for (int i = 1; i < Song.Count; i++)
            {
                if (Song[i].name == "REST")
                {
                    Song[i].yOffset = Song[i - 1].yOffset;
                }
                else
                {
                    Song[i].yOffset = notePosLookup[Song[i].name];
                }
            }

            StartCoroutine("AddNoteFromSong");
            StartCoroutine("HandleJump");
        }



    }

    // Update is called once per frame
    void Update()
    {
        CheckPitch();

        currTime += Time.deltaTime;
        currPos += Time.deltaTime * worldUnitsPerSec;
        movePlayerAndCamera();

        CheckKeyInput();
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
		// If the pitch is incorrect:
        if (string.IsNullOrEmpty(playerPitch) || playerPitch[0] != targetNote[0])
        {
			// Allow recognition of a tolerance range.
			List <string> acceptableRanges = fg.GetLeniencyRange(targetNote, LeniencyRange);
			foreach (string note in acceptableRanges) 
			{
				Debug.Log ("Acceptable note: " + note + ", player pitch: " + playerPitch);
				if (!string.IsNullOrEmpty(playerPitch) && playerPitch[0] == note[0]) 
				{
					isCorrect = true;
					return;
				}
			}

            // If they just deviated from the pitch, start keeping track of the time.
            if (isCorrect)
            {
                elapsedIncorrectTime = 0;
                isCorrect = false;

                Player.GetComponent<SpriteRenderer>().color = noteColorLookup[targetNote];
            }
            //Debug.Log("player pitch = " + playerPitch + ",\ntarget note = " + targetNote);
            // If they've stayed incorrect for long enough that it's probably not just noise, drop them.
            else if (elapsedIncorrectTime > SustainedGracePeriod)
            {
                Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>());
                isFalling = true;
            }
            // Add elapsed incorrect time
            else
            {
                Player.GetComponent<SpriteRenderer>().color = noteColorLookup[targetNote];
                elapsedIncorrectTime += Time.deltaTime;
            }
        }

		// If they got the note right:
        else
            isCorrect = true;
    }

    void SpawnPlatform(int index)
    {
        GameObject plat = Instantiate(platform);

		Color platColor = noteColorLookup [Song [index].name];
        // Set the platform's color
        plat.GetComponent<SpriteRenderer>().color = platColor;

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

		// Create Note Text for the platform.
		GameObject txtobj = Instantiate (platformText);
		TextMesh txtmsh = txtobj.GetComponent<TextMesh>();
		txtmsh.text = Song [index].name;
		txtmsh.color = new Color (1 - platColor.r, 1 - platColor.g, 1 - platColor.b);
		txtobj.transform.position = plat.transform.position + new Vector3(0.05f, 0.3f, 0);
		//txtobj.transform.position += new Vector3 (0, 1, 0);
		platText.Insert (index, txtmsh);

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

    IEnumerator AddNoteFromSong()
    {
        while (lastSpawnedNoteIndex < (Song.Count - 1))
        {
            // Spawn corresponding platform
            SpawnPlatform(++lastSpawnedNoteIndex);

            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float delay = Song[lastSpawnedNoteIndex].duration * 60 / BPM;
            yield return new WaitForSeconds(delay);
        }
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
            ReaderWriter.WriteSong(Song, "HELLO.txt", 60);

            // Spawn corresponding platform
            SpawnPlatform(lastNoteIndex + 1);

            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float delay = newNoteDur * 60 / BPM;
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator HandleJump()
    {
        while (currNoteIndex < Song.Count)
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
        Player.gameObject.transform.position = new Vector3(currPos, Song[currNoteIndex].yOffset + playerHeight / 2 + platformHeight / 2);

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

    // FACADE FUNCTION 
    void ChangeScrollingSpeed(float changeToSpeedMultiplier)
    {
        float targetSpeedMultiplier = speedMult * changeToSpeedMultiplier;

        if (changeToSpeedMultiplier > 1)
        {
            IEnumerator coroutine = ChangeScrollingSpeedIncremental(1 + scrollingInterpolation, (sm) => sm < targetSpeedMultiplier);
            StartCoroutine(coroutine);
        }
        else
        {
            IEnumerator coroutine = ChangeScrollingSpeedIncremental(1 - scrollingInterpolation, (sm) => sm > targetSpeedMultiplier);
            StartCoroutine(coroutine);
        }

    }

    IEnumerator ChangeScrollingSpeedIncremental(float speedMultiplier, Func<float, bool> whileDelegate)
    {
        while (whileDelegate(speedMult))
        {
            speedMult *= speedMultiplier;

            // Update time on screen
            TimeOnScreen /= speedMultiplier;

            // Recalculate conversions between time, beats, and screen space
            float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                     - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
            worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
            worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
            spawnPosOffset = screenWidthInWorldUnits * speedMult;



            // Resize and offset existing platforms

            // Note: this skips over any platforms we've already deleted.
            int minIndex = currNoteIndex;
            while (minIndex > -1 && platforms[minIndex] != null)
            {
                minIndex--;
            }
            minIndex++;

            for (int i = minIndex; i < platforms.Count; i++)
            {

                Platform plat = platforms[i].GetComponent<Platform>();
				TextMesh text = platText [i];

                // Resize the platform's width so it matches the note's duration
                float platWidth = Song[i].duration * worldUnitsPerBeat;
                float startWidth = plat.GetComponent<SpriteRenderer>().bounds.size.x;
                // WHAT. WHY DOES THIS NOT WORK.
                // If you're not going to work, Unity, then give me an error, dammit!
                //plat.transform.localScale.Scale( new Vector3(platWidth/startWidth, 1, 1) );
                plat.transform.localScale = new Vector3(plat.transform.localScale.x * (platWidth / startWidth), plat.transform.localScale.y, plat.transform.localScale.z);

                // Reposition the platform so that it's still at the right timing.

                // Essentially, we get the original distance between the player and the platform,
                // then we multiply that by a conversion factor to get the new distance.
                // The conversion factor is easy - it's just the speed multiplier!

                float oldOffset = plat.transform.position.x - currPos;
                float newOffset = oldOffset * speedMultiplier;
                plat.transform.position = new Vector3(newOffset + currPos, plat.transform.position.y, plat.transform.position.z);
				// Adjust the position of the text for the platform.
				text.transform.position = new Vector3 ((plat.transform.position.x - (platWidth / 2)) + 0.05f, text.transform.position.y, text.transform.position.z);
            }

            yield return null;
        }
    }

    IEnumerator FlashColor(float interval)
    {
        for (int i = 0; i < 4; i++)
        {
            colIsFlashing = true;
            Player.GetComponent<SpriteRenderer>().color = Color.white;
            Invoke("EndColorFlash", interval / 3);
            yield return new WaitForSeconds(interval);
        }
    }

    void EndColorFlash()
    {
        colIsFlashing = false;
    }

    void CheckKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeScrollingSpeed(1.25f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeScrollingSpeed(.8f);
        }

        return;
    }

}
