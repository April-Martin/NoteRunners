using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
   
    #region Variables
    // Settings
    public bool DEBUG_InvincibleMode = false, isTextActive = true;
    public string[] NoteDetectionRange = new string[2], NotesRange = new string[2];
    public float BPM, TransitionGracePeriod, SustainedGracePeriod;
	public int LeniencyRange = 0;
    public bool SongMode = false;
    public float MaxTimeBetweenRests = 4f;
    public string filename;
    public int ScorePerSecond = 100;
    public float Score = 0;
    public float TimeOnScreen = 5;


    // Dependencies
    public PlayerMovement Player;
    public GameObject platform, platformText, particles, background;
    private PitchTester pt;

    // Private vars
    private List<Note> Song = new List<Note>(50);
    private List<BoxCollider2D> platforms = new List<BoxCollider2D>(50);
	private List<TextMesh> platText = new List<TextMesh> (50);

    internal float currPos = 0, currTime = 0;
    private int currNoteIndex = 0, lastSpawnedNoteIndex = 0;
    float spawnPosOffset = 0, worldUnitsPerSec = 0, worldUnitsPerBeat = 0;

	private bool isRespawning = false ,isChecking = true, isFalling = false, colIsFlashing = false, isCorrect = true;

	internal Dictionary<string, float> notePosLookup = new Dictionary<string, float>
	{
		{"E2", -7f}, {"F2", -6.5f}, {"G2", -6f}, {"A2", -5.5f}, {"B2", -5f}, {"C3", -4.5f}, {"D3", -4f},
		{"E3", -3.5f}, {"F3", -3f}, {"G3", -2.5f}, {"A3", -2f}, {"B3", -1.5f}, {"C4", -1f}, {"D4", -.5f},
		{"E4", 0f}, {"F4", .5f}, {"G4", 1f}, {"A4", 1.5f}, {"B4", 2f}, {"C5", 2.5f}, {"D5", 3f}, {"E5", 3.5f},
		{"F5", 4f}, {"G5", 4.5f}, {"REST", 0}, {"A5", 5f}, {"B5", 5.5f}, {"C6", 6f}
	};

	internal Dictionary<string, Color> noteColorLookup = new Dictionary<string, Color>
	{
		{"E2", new Color32(255, 0, 0, 1)}, {"F2", new Color(1f, .5f, 0f)}, {"G2", Color.yellow}, {"A2", Color.green}, {"B2", Color.cyan},
		{"C3", Color.blue}, {"D3", new Color(.8f, .2f, .8f)}, {"E3", Color.red}, {"F3", new Color(1f, .5f, 0f)}, {"G3", Color.yellow},
		{"A3", Color.green}, {"B3", Color.cyan}, {"C4", Color.blue}, {"D4", new Color(.8f, .2f, .8f)}, {"E4", Color.red}, {"F4", new Color(1f, .5f, 0f)},
		{"G4", Color.yellow}, {"A4", Color.green}, {"B4", Color.cyan}, {"C5", Color.blue}, {"D5", new Color(.8f, .2f, .8f)}, {"E5", Color.red},
		{"F5", new Color(1f, .5f, 0f)}, {"G5", Color.yellow}, {"REST", Color.black}, {"A5", Color.green}, {"B5", Color.cyan}, {"C6", Color.blue}
	};

	private FrequencyGuide fg;

    private List<string> NotesAllowed;

    public float speedMult = 1f, scrollingInterpolation = 0.01f;

    private float elapsedIncorrectTime = 0, elapsedSinceRest = 0;
    #endregion
    // Use this for initialization
    void Start()
    {
        if (GameGlobals.GlobalInstance != null)
        {
            GameGlobals temp = GameGlobals.GlobalInstance;
            isTextActive = temp.isTextActive;
            NoteDetectionRange = temp.NoteDetectionRange;
            TimeOnScreen = temp.TimeOnScreen;
            NotesRange = temp.NotesRange;
            BPM = temp.BPM;
            TransitionGracePeriod = temp.TransitionGracePeriod;
            SustainedGracePeriod = temp.SustainedGracePeriod;
            LeniencyRange = temp.LeniencyRange;
            SongMode = temp.SongMode;
            filename = temp.selectedSong;
            speedMult = temp.speedMult;
            scrollingInterpolation = temp.scrollingInterpolation;
            MaxTimeBetweenRests = temp.MaxTimeBetweenRests;
        }

        Player.GetComponent<SpriteRenderer>().color = Color.black;

        // Initialize vars
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        background = GameObject.Find("Background");
        FillNotesAllowed();
		fg = new FrequencyGuide ();
        pt.minFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[0], out pt.minFreq) ? pt.minFreq : 75;
        pt.maxFreq = pt.guide.noteToFreq.TryGetValue(NoteDetectionRange[1], out pt.maxFreq) ? pt.maxFreq : 1075;
  
        // If we're in Song mode, read in file information
        if (SongMode)
        {
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
        }

        // Calculate conversion factors
        float screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, 10)).x
                                         - Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).x;
        worldUnitsPerSec = screenWidthInWorldUnits / TimeOnScreen;
        worldUnitsPerBeat = worldUnitsPerSec * 60 / BPM;
        spawnPosOffset = screenWidthInWorldUnits;

        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));	// Time * BPM / 60 gives us the number of beats
        SpawnPlatform(0);

        // Start core coroutines
        if (!SongMode)
        {
            StartCoroutine("AddRandomNote");
            StartCoroutine("HandleJump");
        }
        else
        {
            StartCoroutine("AddNoteFromSong");
            StartCoroutine("HandleJump");
        }

    }

    // Update is called once per frame
    void Update()
    {
        AwardScore();
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
            Player.GetComponent<SpriteRenderer>().color = Color.white;
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
        {
            isCorrect = false;
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            isCorrect = true;
            return;
        }

        // Compare player pitch to target note
        // If the pitch is incorrect:
        if (string.IsNullOrEmpty(playerPitch) || playerPitch != targetNote)
        {
			// Allow recognition of a tolerance range.
			List <string> acceptableRanges = fg.GetLeniencyRange(targetNote, LeniencyRange);
			foreach (string note in acceptableRanges) 
			{
				//Debug.Log ("Acceptable note: " + note + ", player pitch: " + playerPitch);
				if (!string.IsNullOrEmpty(playerPitch) && playerPitch == note) 
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

		if (isTextActive)
		{
			// Create Note Text for the platform.
			GameObject txtobj = Instantiate (platformText);
			TextMesh txtmsh = txtobj.GetComponent<TextMesh> ();
			txtmsh.text = Song [index].name;
			txtmsh.color = new Color (1 - platColor.r, 1 - platColor.g, 1 - platColor.b);
			txtobj.transform.position = plat.transform.position + new Vector3 (0.05f, 0.3f, 0);
			//txtobj.transform.position += new Vector3 (0, 1, 0);
			platText.Insert (index, txtmsh);
		}

        // But, bump it over to the right by half of the platform's width, so that it starts at the right spot
        plat.transform.position += Vector3.right * platWidth / 2;

        // Set its range marker if it's not a rest
        if (Song[index].name != "REST")
            plat.GetComponent<Platform>().SetRangeMarker(LeniencyRange, platColor);
        else
            plat.GetComponent<Platform>().DisableRangeMarker();

        // Add it to our list of platforms
        platforms.Insert(index, plat.GetComponent<BoxCollider2D>());


    }


    void AwardScore()
    {
        if (!isFalling && isChecking && !colIsFlashing && isCorrect && Song[currNoteIndex].name != "REST")
        {
            Score += (ScorePerSecond * speedMult * Time.deltaTime);
        }
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
            // Update rest tracker
            int lastNoteIndex = Song.Count - 1;
            string lastNoteName = ((Note)Song[lastNoteIndex]).name;
            if (lastNoteName == "REST")
                elapsedSinceRest = 0;
            else
                elapsedSinceRest += Song[lastNoteIndex].duration;

            // Randomize note name, unless it's time for a compulsory rest
            string newNoteName = lastNoteName;
            if (elapsedSinceRest < MaxTimeBetweenRests)
            {
                while (newNoteName == lastNoteName)
                {
                    newNoteName = NotesAllowed[UnityEngine.Random.Range(0, NotesAllowed.Count)];    // Note: min inclusive, max exclusive
                }
            }
            else
                newNoteName = "REST";

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
				if (Song [currNoteIndex + 1].name == "REST")
				{
					float jumpHeight = Song [currNoteIndex + 1].yOffset;// - Song[currNoteIndex].yOffset;
					float playerHeight = Player.GetComponent<SpriteRenderer> ().bounds.size.y;
					float platHeight = platform.GetComponent<SpriteRenderer> ().bounds.size.y;
					Player.transform.position = new Vector3 (Player.transform.position.x, jumpHeight + playerHeight / 2 + platHeight / 2);
				} 
				else
				{
					GameObject particle1 = Instantiate (particles);
					particle1.transform.position = Player.transform.position;

					//particle.GetComponent<ParticleSystem> ().Play ();
					Destroy (particle1, 1.5f);

					float jumpHeight = Song [currNoteIndex + 1].yOffset;// - Song[currNoteIndex].yOffset;
                    
					float playerHeight = Player.GetComponent<SpriteRenderer> ().bounds.size.y;
					float platHeight = platform.GetComponent<SpriteRenderer> ().bounds.size.y;
                    Debug.Log("jumpHeight: " + jumpHeight + "\nplatHeight: " + platHeight + "\nplayerHeight: " + playerHeight);
                    Debug.Log("new y pos = " + (jumpHeight + playerHeight / 2 + platHeight / 2));
					Player.transform.position = new Vector3 (Player.transform.position.x, jumpHeight + playerHeight / 2 + platHeight / 2 + .2f);
					GameObject particle2 = Instantiate (particles);
					particle2.transform.position = Player.transform.position;

					//particle.GetComponent<ParticleSystem> ().Play ();
					Destroy (particle2, 1.5f);
				}
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

//    void FillNoteColorLookup()
//    {
//        noteColorLookup = new Dictionary<string, Color>();
//
//        noteColorLookup.Add("E2", Color.red);
//        noteColorLookup.Add("F2", new Color(1f, .5f, 0f));
//        noteColorLookup.Add("G2", Color.yellow);
//        noteColorLookup.Add("A2", Color.green);
//        noteColorLookup.Add("B2", Color.cyan);
//        noteColorLookup.Add("C3", Color.blue);
//        noteColorLookup.Add("D3", new Color(.8f, .2f, .8f));
//        noteColorLookup.Add("E3", Color.red);
//        noteColorLookup.Add("F3", new Color(1f, .5f, 0f));
//        noteColorLookup.Add("G3", Color.yellow);
//        noteColorLookup.Add("A3", Color.green);
//        noteColorLookup.Add("B3", Color.cyan);
//        noteColorLookup.Add("C4", Color.blue);
//
//        // Original list
//        noteColorLookup.Add("D4", new Color(.8f, .2f, .8f));
//        noteColorLookup.Add("E4", Color.red);
//        noteColorLookup.Add("F4", new Color(1f, .5f, 0f));
//        noteColorLookup.Add("G4", Color.yellow);
//        noteColorLookup.Add("A4", Color.green);
//        noteColorLookup.Add("B4", Color.cyan);
//        noteColorLookup.Add("C5", Color.blue);
//        noteColorLookup.Add("D5", new Color(.8f, .2f, .8f));
//        noteColorLookup.Add("E5", Color.red);
//        noteColorLookup.Add("F5", new Color(1f, .5f, 0f));
//        noteColorLookup.Add("G5", Color.yellow);
//        noteColorLookup.Add("REST", Color.black);
//        //End of original list
//
//        noteColorLookup.Add("A5", Color.green);
//        noteColorLookup.Add("B5", Color.cyan);
//        noteColorLookup.Add("C6", Color.blue);
//    }

//    void FillNotePosLookup()
//    {
//        notePosLookup = new Dictionary<string, float>();
//
//        notePosLookup.Add("E2", -7f);
//        notePosLookup.Add("F2", -6.5f);
//        notePosLookup.Add("G2", -6f);
//        notePosLookup.Add("A2", -5.5f);
//        notePosLookup.Add("B2", -5f);
//        notePosLookup.Add("C3", -4.5f);
//        notePosLookup.Add("D3", -4f);
//        notePosLookup.Add("E3", -3.5f);
//        notePosLookup.Add("F3", -3f);
//        notePosLookup.Add("G3", -2.5f);
//        notePosLookup.Add("A3", -2f);
//        notePosLookup.Add("B3", -1.5f);
//        notePosLookup.Add("C4", -1f);
//
//        // Original list
//        notePosLookup.Add("D4", -.5f);
//        notePosLookup.Add("E4", 0f);
//        notePosLookup.Add("F4", .5f);
//        notePosLookup.Add("G4", 1f);
//        notePosLookup.Add("A4", 1.5f);
//        notePosLookup.Add("B4", 2f);
//        notePosLookup.Add("C5", 2.5f);
//        notePosLookup.Add("D5", 3f);
//        notePosLookup.Add("E5", 3.5f);
//        notePosLookup.Add("F5", 4f);
//        notePosLookup.Add("G5", 4.5f);
//        notePosLookup.Add("REST", 0);
//        // End of original list.
//
//        notePosLookup.Add("A5", 5f);
//        notePosLookup.Add("B5", 5.5f);
//        notePosLookup.Add("C6", 6f);
//    }

    private void FillNotesAllowed()
    {
        NotesAllowed = new List<string>();
        NotesAllowed.Add("REST");

        float notesRangeMinPos; float notesRangeMaxPos;
        bool found = false;
        if (Player.NotePosLookup.TryGetValue(NotesRange[0], out notesRangeMinPos))
        {
            found = true;
        }
        else
            found = false;

        float temp = 0;
        notesRangeMinPos = Player.NotePosLookup.TryGetValue(NotesRange[0], out temp) ? temp : -0.5f;
        notesRangeMaxPos = Player.NotePosLookup.TryGetValue(NotesRange[1], out temp) ? temp : 4.5f;
        foreach (string Note in Player.NotePosLookup.Keys)
        {
            if (Player.NotePosLookup[Note] >= notesRangeMinPos)
            {
                if (Player.NotePosLookup[Note] <= notesRangeMaxPos)
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
            for (int layer = 0; layer < background.transform.childCount; layer ++)
            {
                background.transform.GetChild(layer).GetComponent<BackgroundScroller>().speed *= speedMultiplier;
            }

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
				if (text != null) text.transform.position = new Vector3 ((plat.transform.position.x - (platWidth / 2)) + 0.05f, text.transform.position.y, text.transform.position.z);
                // Reset its range marker if it's not a rest
                if (Song[i].name != "REST")
                    plat.SetRangeMarker(LeniencyRange, noteColorLookup[Song[i].name]);
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
