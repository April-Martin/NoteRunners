using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    #region Variables
    // Settings
    public bool DEBUG_InvincibleMode = false;
    private bool debuggingMode = false;
    public int ScorePerSecond = 100;
    public float Score = 0;
    public float CameraOffset = 7.5f;
    public float speedMultLowerLimit = 1f, speedMultUpperLimit = 3f, respawnSpeedPenalty = 0.75f;
    private bool warmupMode = false;

    // Game globals
    internal bool isTextActive = true;
    private float TimeOnScreen = 5;
    private string[] NotesRange = new string[2];
    private float BPM, TransitionGracePeriod, SustainedGracePeriod;
    private int LeniencyRange = 0;
    private bool SongMode = false;
    public string filename;
    private float MaxTimeBetweenRests = 4f;
    private bool WritingOn;
    private bool bassClefMode = false;
    private int infiniteNoteDensity;
    private float plrRed = 1f, plrGrn = 1f, plrBlu = 1f;
    private float scrollingInterpolation = 0.01f;

    // Dependencies
    public PlayerMovement Player;
    public Buddy Bud;
    public GameObject platform, platformText, background;
    private PitchTester pt;
    private AudioCuePlayer audioPlayer;
    private FrequencyGuide fg;
    public GameObject deathParticlePrefab;

    // Status-tracking  vars
    internal float currPos = 0, currTime = 0;
    private int currNoteIndex = 0, lastSpawnedNoteIndex = 0;
    public int noteStreak = 0;
    internal bool isRespawning = false, isChecking = true, isFalling = false, colIsFlashing = false, isCorrect = true;
    private bool singleFire = false, songIsOver = false;
    private float elapsedIncorrectTime = 0, elapsedSinceRest = 0;
    public float speedMult = 1f;

    // Utility vars
    private float spawnPosOffset = 0, worldUnitsPerSec = 0, worldUnitsPerBeat = 0;
    private float maxNoteDuration = 0, minNoteDuration = 0;
    private ParticleSystem deathParticle;

    // Time check variables (for keeping the coroutines honest)
    private float playAudioCueTime = 0, handleJumpTime = 0, addNoteTime = 0;

    // Data structures
    private List<Note> Song = new List<Note>(50);
    private List<BoxCollider2D> platforms = new List<BoxCollider2D>(50);
    private List<TextMesh> platText = new List<TextMesh>(50);
    private List<string> NotesAllowed;
    internal Dictionary<string, float> notePosLookup = new Dictionary<string, float>
    {
        {"E2", -7f}, {"F2", -6.5f}, {"F#2", -6.5f}, {"G2", -6f}, {"G#2", -6f}, {"A2", -5.5f}, {"A#2", -5.5f}, {"B2", -5f}, 
        {"C3", -4.5f}, {"C#3", -4.5f}, {"D3", -4f}, {"D#3", -4f}, {"E3", -3.5f}, {"F3", -3f}, {"F#3", -3f}, {"G3", -2.5f}, {"G#3", -2.5f}, {"A3", -2f}, {"A#3", -2f}, {"B3", -1.5f}, 
        {"C4", -1f}, {"C#4", -1f}, {"D4", -.5f}, {"D#4", -.5f}, {"E4", 0f}, {"F4", .5f}, {"F#4", .5f}, {"G4", 1f}, {"G#4", 1f}, {"A4", 1.5f}, {"A#4", 1.5f}, {"B4", 2f}, 
        {"C5", 2.5f}, {"C#5", 2.5f}, {"D5", 3f}, {"D#5", 3f}, {"E5", 3.5f}, {"F5", 4f}, {"F#5", 4f}, {"G5", 4.5f}, {"G#5", 4.5f}, {"A5", 5f}, {"A#5", 5f},{"B5", 5.5f}, 
		{"C6", 6f}, {"REST", 0}
    };
    internal Dictionary<float, Color> posColorLookup = new Dictionary<float, Color>
    {
        {-7, Color.white},
        {-6, Color.white}, {-5, new Color32(210, 155, 188, 255)},
        {-4, new Color32(210, 146, 190, 255)}, {-3, new Color32(165, 163, 208, 255)},
        {-2, new Color32(134, 193, 230, 255)}, {-1, new Color32(150, 219, 200, 255)},
        {-0, new Color32(191, 245, 145, 255)}, {1, new Color32(216, 251, 118, 255)},
        {2, new Color32(244, 238, 108, 255)}, {3, new Color32(255, 212, 91, 255)},
        {4, new Color32(254, 138, 52, 255)}, {5, new Color32(255, 111, 51, 255)},
        {6, Color.white}
    };
    internal Dictionary<string, Color> noteColorLookup = new Dictionary<string, Color>()
    {
        {"REST", new Color(.15f, .15f, .15f, 1)}
    };

    #endregion



    void Start()
    {
        if (GameGlobals.GlobalInstance != null)
        {
            GameGlobals temp = GameGlobals.GlobalInstance;
            isTextActive = temp.isTextActive;
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
            WritingOn = temp.WritingOn;
            bassClefMode = temp.bassClefMode;
            infiniteNoteDensity = temp.NoteDensity;
            plrRed = temp.plrRed;
            plrGrn = temp.plrGrn;
            plrBlu = temp.plrBlu;
        }

        // Initialize tables
        if (bassClefMode)
            BassClefTransformation();
        fillGapsInPosColorLookup();
        fillNoteColorLookup();

        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        audioPlayer = GetComponent<AudioCuePlayer>();
        background = GameObject.Find("Background");
        FillNotesAllowed();
        fg = new FrequencyGuide();
        deathParticle = GameObject.Instantiate(deathParticlePrefab).GetComponent<ParticleSystem>();


        if (infiniteNoteDensity <= 1)
        {
            minNoteDuration = 2;
            maxNoteDuration = 4;
        }
        else if (infiniteNoteDensity == 2)
        {
            minNoteDuration = 1;
            maxNoteDuration = 3;
        }
        else if (infiniteNoteDensity == 3)
        {
            minNoteDuration = 1 / 8;
            maxNoteDuration = 2;
        }


        // If we're in Song mode, read in file information
        if (SongMode)
        {
            ReaderWriter.ReadSong(ref Song, filename, ref BPM, ref bassClefMode);
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

        // Add terminating rest to song mode
        if (SongMode)
            Song.Add(new Note("REST", screenWidthInWorldUnits));

        Player.GetComponent<SpriteRenderer>().color = new Color(plrRed, plrGrn, plrBlu, 1f);
        Bud.SetParticleBPM(BPM);

        if (warmupMode)
            StartWarmup();
        else
            StartLevel();
    }

    void ClearState()
    {
        // Clear all level state variables
        platforms.Clear(); platText.Clear();
        currNoteIndex = 0; currTime = 0; currPos = 0;
        playAudioCueTime = 0; handleJumpTime = 0; addNoteTime = 0;
        lastSpawnedNoteIndex = 0;
        noteStreak = 0;
        songIsOver = false;
    }


    void StartWarmup()
    {
        ClearState();

        Song.Insert(0, new Note("REST", 2));
        SpawnPlatform(0, false, 0);

        int i = 1;
        List<string> ToAdd = NotesAllowed;
        while (ToAdd.Count > 0)
        {
            string note = ToAdd[UnityEngine.Random.Range(0, ToAdd.Count)];     // inclusive / exclusive
            ToAdd.Remove(note);
            Song.Insert(i, new Note(note, 2));
            Song[i].yOffset = notePosLookup[note];
            SpawnPlatform(i, false, 2 * i * worldUnitsPerBeat);
            i++;
        }

        StartCoroutine("HandleJump");
        StartCoroutine("OscillatePlatformOpacity");
    }

    void StartLevel()
    {
        ClearState();

        Song.Insert(0, new Note("REST", TimeOnScreen * BPM / 60));	// Time * BPM / 60 gives us the number of beats
        SpawnPlatform(0, false, 0);

        // Start core coroutines
        if (!SongMode)
        {
            StartCoroutine("AddRandomNote");
        }
        else
        {
            StartCoroutine("AddNoteFromSong");
        }
        StartCoroutine("HandleJump");
        StartCoroutine("OscillatePlatformOpacity");
        StartCoroutine("PlayAudioCues");

    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyInput();

        CheckPitch();
        AwardScore();

        currTime += Time.deltaTime;
        currPos += Time.deltaTime * worldUnitsPerSec;

        movePlayerAndCamera();


    }

    void CheckPitch()
    {
        // Update buddy color
        string playerPitch = pt.MainNote;

        // If player isn't singing, or if player's falling:
        if (string.IsNullOrEmpty(playerPitch) || isFalling)
        {
            Bud.SetColor(Color.white);
        }
        // If player is singing:
        else if (noteColorLookup.ContainsKey(playerPitch))
        {
            Bud.SetColor(noteColorLookup[playerPitch]);
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
            List<string> acceptableRanges = fg.GetLeniencyRange(targetNote, LeniencyRange);
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
                Bud.GetComponent<SpriteRenderer>().color = noteColorLookup[targetNote];
            }
            //Debug.Log("player pitch = " + playerPitch + ",\ntarget note = " + targetNote);
            // If they've stayed incorrect for long enough that it's probably not just noise, drop them.
            else if (elapsedIncorrectTime > SustainedGracePeriod && !isFalling)
            {
                noteStreak = 0;
                Player.gameObject.layer = 8;
                //  Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>());
                isFalling = true;
                isCorrect = false;
                //Player.GetComponent<PlayerMovement>().PauseAnimation();
                Player.SetAnimSpeed(3);
                Player.StartPlayerSpinning();
                platforms[currNoteIndex].GetComponent<Platform>().PlayVanishingParticles();

            }
            // Add elapsed incorrect time
            else if (!isFalling)
            {
                Bud.SetColor(noteColorLookup[targetNote]);
                elapsedIncorrectTime += Time.deltaTime;
            }
        }

        // If they got the note right:
        else
        {
            isCorrect = true;
        }
    }

    void SpawnPlatform(int index, bool autoPosition = true, float xPos = 0)
    {
        Platform plat = Instantiate(platform).GetComponent<Platform>();

        // Set the platform's color
        Color platColor = noteColorLookup[Song[index].name];
        plat.SetPlatColor(platColor);

        // Deal with sharps and flats
        if (Song[index].name[1] == '#')
        {
            // Give sharps a 50/50 chance of being the corresponding flat instead.
            // Make sure the label is correct either way
            int rnd = UnityEngine.Random.Range(0, 2);
            if (rnd == 0)
            {
                Song[index].yOffset += .5f;
                plat.SetPlatFlat();

                /*
                 *  Note: this is, well, kind of hacky. But the ASCII codes for letters A-G are in order, and 
                 *  so are digits 0-9, so works to just cast the char to an int for the increment, then cast it back to a char.
                 * */
                char startNote = Song[index].name[0];
                char newNote = (startNote == 'G') ? ('A') : ((char)(startNote + 1));

                char startOctave = Song[index].name[2];
                char newOctave = (startNote == 'B') ? (char)(Song[index].name[2] + 1) : Song[index].name[2];

                char[] newLabel = new char[2];
                newLabel[0] = newNote;
                newLabel[1] = newOctave;
                Song[index].label = new String(newLabel);
            }
            else
            {
                plat.SetPlatSharp();
                char[] newLabel = new char[2];
                newLabel[0] = Song[index].name[0];
                newLabel[1] = Song[index].name[2];
                Song[index].label = new String(newLabel);
            }
        }

        // Set the platform's width so it matches the note's duration
        float platWidth = (Song[index].duration - .05f) * worldUnitsPerBeat;
        plat.SetPlatWidth(platWidth);

        // Set it at the position corresponding to the note's start time.
        Vector3 platPos = new Vector3(0, 0);
        if (autoPosition)
            platPos.x = currPos + spawnPosOffset;
        else
            platPos.x = xPos;

        if (Song[index].name == "REST" && index != 0)
            platPos.y = Song[index - 1].yOffset;
        else
            platPos.y = Song[index].yOffset;
        plat.transform.position = platPos;

        if (isTextActive)
        {
            // Create Note Text for the platform.
            GameObject txtobj = Instantiate(platformText);
            TextMesh txtmsh = txtobj.GetComponent<TextMesh>();
            txtmsh.text = Song[index].label;
            if (Song[index].name == "REST")
                txtmsh.color = Color.white; //new Color (1 - platColor.r, 1 - platColor.g, 1 - platColor.b);
            else
                txtmsh.color = new Color(platColor.r * .2f, platColor.g * .2f, platColor.b * .2f);
            txtobj.transform.position = plat.transform.position + new Vector3(0.15f, 0.3f, 0);
            //txtobj.transform.position += new Vector3 (0, 1, 0);
            platText.Insert(index, txtmsh);
        }

        // But, bump it over to the right by half of the platform's width, so that it starts at the right spot
        plat.transform.position += Vector3.right * platWidth / 2;


        // Set its range marker if it's not a rest
        /*
         if (Song[index].name != "REST")
             plat.GetComponent<Platform>().SetRangeMarker(LeniencyRange, platColor);
         else
         * */
        plat.GetComponent<Platform>().DisableRangeMarker();

        // Add it to our list of platforms
        platforms.Insert(index, plat.GetComponent<BoxCollider2D>());


    }


    void AwardScore()
    {
        //TODO : Don't depend on colIsFlashing!
        if (!isFalling && !colIsFlashing && isCorrect && Song[currNoteIndex].name != "REST")
        {
            Score += (ScorePerSecond * speedMult * Time.deltaTime);
            Bud.SetParticleIntensity(2);
        }
        else
            Bud.SetParticleIntensity(1);
    }


    void movePlayerAndCamera()
    {
        Player.transform.position = new Vector3(currPos + Player.GetComponent<SpriteRenderer>().bounds.size.x / 2, Player.transform.position.y);
        if (!songIsOver)
            Camera.main.transform.position = new Vector3(currPos + CameraOffset, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }

    IEnumerator AddNoteFromSong()
    {
        while (lastSpawnedNoteIndex < (Song.Count - 1))
        {
            // Spawn corresponding platform
            SpawnPlatform(++lastSpawnedNoteIndex);

            // Calculate the timing error (negative if we're ahead, positive if we're behind)
            float error = addNoteTime - currTime;
            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float durInSec = Song[lastSpawnedNoteIndex].duration * 60 / BPM;
            addNoteTime += durInSec;
            // Add the current error from the next iteration's delay, so that errors don't build up.
            yield return new WaitForSeconds(durInSec + error);
        }

        // When the song is over:
        yield break;
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

            float newNoteDur = UnityEngine.Random.Range(1, (int)(maxNoteDuration));


            // Fill in note properties
            Note newNote = new Note(newNoteName, newNoteDur);
            newNote.yOffset = newNoteName == "REST" ? Song[lastNoteIndex].yOffset : notePosLookup[newNoteName];

            // Add note to song
            Song.Insert(lastNoteIndex + 1, newNote);
            if (WritingOn)
                ReaderWriter.WriteSong(Song, "HELLO.txt", 60, bassClefMode);

            // Spawn corresponding platform
            SpawnPlatform(lastNoteIndex + 1);

            // Calculate the timing error (negative if we're ahead, positive if we're behind)
            float error = addNoteTime - currTime;
            // Set the next new note to spawn as soon as this new one's duration has elapsed
            float durInSec = newNoteDur * 60 / BPM;
            addNoteTime += durInSec;
            // Add the current error from the next iteration's delay, so that errors don't build up.
            yield return new WaitForSeconds(durInSec + error);
        }
    }

    IEnumerator HandleJump()
    {
        while (currNoteIndex < Song.Count - 1)
        {
            // Wait till end of note
            float beats = Song[currNoteIndex].duration;
            float dur = Song[currNoteIndex].duration * 60 / BPM;

            // Handle transition grace period, 
            Invoke("StartTransitionGracePeriod", dur - TransitionGracePeriod / 2);
            if (Song[currNoteIndex + 1].name != "REST")
                Invoke("PlayTeleportParticles", (dur > .5f) ? dur - .5f : 0);
            // Set up audio cue of next note 
            // Note: it precedes the nect note by either half a beat, or (if that's too long) half of the current note's length.
            //Invoke("PlayNextNote", dur - Mathf.Min(0.5f, 0.5f * beats) * 60 / BPM);

            // Calculate the timing error (negative if we're ahead, positive if we're behind)
            float error = handleJumpTime - currTime;
            handleJumpTime += dur;

            // This is a little hacky: we need to make sure the runner jumps before the audio cue is played,
            // so subtract a tiny offset from the jump's delay to make sure it fires first.
            float offset = .15f;

            // Add the current error from the next iteration's delay, so that errors don't build up.
            yield return new WaitForSeconds(dur + error - offset);


            // Handle jump
            if (!isFalling)
            {
                    float jumpHeight = Song[currNoteIndex + 1].yOffset;
                    float playerHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
                    float platHeight = platform.GetComponent<Platform>().height;
                    Player.transform.position = new Vector3(Player.transform.position.x, jumpHeight + playerHeight / 2 + platHeight / 2 + .2f);
            }
            if (Song[currNoteIndex].name != "REST")
            {
                if (++noteStreak % 3 == 0 && noteStreak != 0)
                {
                    if (!singleFire)
                    {
                        Debug.Log("SF = " + singleFire);
                        ChangeScrollingSpeed(speedMult * 1.2f >= speedMultUpperLimit ? speedMultUpperLimit / speedMult : 1.2f);
                        singleFire = true;
                    }
                }
            }

            currNoteIndex++;
            singleFire = false;
        }

        // When we reach the end of the song:
        songIsOver = true;
        for (int layer = 0; layer < background.transform.childCount; layer++)
        {
            background.transform.GetChild(layer).GetComponent<BackgroundScroller>().speed = 0;
        }
        yield break;
    }

    void PlayNextNote()
    {
        string nextNote = Song[currNoteIndex + 1].name;
        audioPlayer.PlayNote(nextNote, .5f);
    }

    void PlayTeleportParticles()
    {
        platforms[currNoteIndex + 1].GetComponent<Platform>().PlayTeleportParticles();
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


    private void BassClefTransformation()
    {
        List<string> keys = notePosLookup.Keys.ToList();
        for (int i = 0; i < notePosLookup.Count; i++)
        {
            if (keys[i] != "REST")
            {
                notePosLookup[keys[i]] += 6;
            }
        }
    }

    private void FillNotesAllowed()
    {
        NotesAllowed = new List<string>();
        NotesAllowed.Add("REST");

        float notesRangeMinPos; float notesRangeMaxPos;
        /*
         * // It looks like this section was rendered obsolete by the Turian statements below?
        bool found = false;
        if (notePosLookup.TryGetValue(NotesRange[0], out notesRangeMinPos))
        {
            found = true;
        }
        else
            found = false;
         * */

        float temp = 0;
        notesRangeMinPos = notePosLookup.TryGetValue(NotesRange[0], out temp) ? temp : -0.5f;
        notesRangeMaxPos = notePosLookup.TryGetValue(NotesRange[1], out temp) ? temp : 4.5f;

        foreach (string Note in notePosLookup.Keys)
        {
            if (notePosLookup[Note] >= notesRangeMinPos)
            {
                if (notePosLookup[Note] <= notesRangeMaxPos)
                    NotesAllowed.Add(Note);
            }
        }

        // Remove sharp of max note, if it's in the list
        char[] maxSharp = new char[3];
        maxSharp[0] = NotesRange[1][0];
        maxSharp[1] = '#';
        maxSharp[2] = NotesRange[1][1];
        string ms = new string(maxSharp);
        NotesAllowed.Remove(ms);
    }

    private void showEndScreen()
    {
        // Go to victory/score scene
        SceneManager.LoadScene(2);
    }

    internal void RespawnPlayer()
    {
        // When the player runs off screen at the end, show victory screen
        if (!isFalling && songIsOver)
        {
            GameGlobals.GlobalInstance.score = (int) Score;
            showEndScreen();
            return;
        }

        isFalling = false;
        Player.gameObject.layer = 0;

        //Slow platform speed by half if greater than 1 with lower bound on speed = 1.
        ChangeScrollingSpeed(speedMult * respawnSpeedPenalty >= speedMultLowerLimit ? respawnSpeedPenalty : speedMultLowerLimit / speedMult);

        // Halt momentum of player
        Player.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        // Show particle effect
        deathParticle.transform.position = new Vector3(transform.position.x, transform.position.y - 1);
        deathParticle.Play();

        // Respawn player
        float playerHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
        float platformHeight = platform.GetComponent<Platform>().height;
        Player.gameObject.transform.position = new Vector3(currPos, Song[currNoteIndex].yOffset + playerHeight / 2 + platformHeight / 2);
        Player.SetAnimSpeed(1);
        Player.ResetPlayerRotation();

        // Pay attention to collisions again
        //Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>(), false);
        Player.gameObject.layer = 0;

        // Grant brief invulnerability
        float deathGracePeriod = 60 / BPM * 2;
        isChecking = false;
        isRespawning = true;
        Invoke("EndDeathGracePeriod", deathGracePeriod);
        IEnumerator coroutine = FlashColor(deathGracePeriod / 4);
        StartCoroutine(coroutine);

        StartCoroutine("ShakeScreen");
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

    private IEnumerator ShakeScreen()
    {
        Vector3 originalCameraPosition = Camera.main.transform.position;
        float shakeFactor = .3f;
        bool isResetting = false;
        while (shakeFactor > 0)
        {
            if (isResetting)
            {
                Camera.main.transform.position = Camera.main.transform.position + ((Vector3)UnityEngine.Random.insideUnitCircle * shakeFactor);
            }
            else
            {
                Camera.main.transform.position = new Vector3(currPos + CameraOffset, originalCameraPosition.y, originalCameraPosition.z);
            }
            shakeFactor -= Time.deltaTime * .33f;
            isResetting = !isResetting;
            yield return null;
        }
        Camera.main.transform.position = new Vector3(currPos + CameraOffset, originalCameraPosition.y, originalCameraPosition.z);
    }

    IEnumerator ChangeScrollingSpeedIncremental(float speedMultiplier, Func<float, bool> whileDelegate)
    {
        while (whileDelegate(speedMult))
        {
            speedMult *= speedMultiplier;
            for (int layer = 0; layer < background.transform.childCount; layer++)
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
                TextMesh text = platText[i];

                // Resize the platform's width so it matches the note's duration
                float platWidth = (Song[i].duration - .05f) * worldUnitsPerBeat;
                plat.SetPlatWidth(platWidth);

                // Reposition the platform so that it's still at the right timing.

                // Essentially, we get the original distance between the player and the platform,
                // then we multiply that by a conversion factor to get the new distance.
                // The conversion factor is easy - it's just the speed multiplier!

                float oldOffset = plat.transform.position.x - currPos;
                float newOffset = oldOffset * speedMultiplier;
                plat.transform.position = new Vector3(newOffset + currPos, plat.transform.position.y, plat.transform.position.z);
                // Adjust the position of the text for the platform.
                if (text != null) text.transform.position = new Vector3((plat.transform.position.x - (platWidth / 2)) + 0.05f, text.transform.position.y, text.transform.position.z);
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
            Player.GetComponent<SpriteRenderer>().color = new Color(plrRed, plrGrn, plrBlu, 0.41f);
            Invoke("EndColorFlash", 3 * interval / 4);
            yield return new WaitForSeconds(interval);
        }
    }

    void EndColorFlash()
    {
        colIsFlashing = false;
        Player.GetComponent<SpriteRenderer>().color = new Color(plrRed, plrGrn, plrBlu, 1);
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
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Break");
            debuggingMode = true;
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        return;
    }

    private void fillGapsInPosColorLookup()
    {
        // Fill in the *.5 colors by interpolating between the integers
        for (int i = -7; i < 6; i++)
        {
            Color32 mid = Color.Lerp(posColorLookup[i], posColorLookup[i + 1], 0.5f);
            posColorLookup.Add((i + .5f), mid);
        }

    }

    private void fillNoteColorLookup()
    {
        List<string> notes = notePosLookup.Keys.ToList();
        for (int i = 0; i < notePosLookup.Count; i++)
        {
            if (notes[i] == "REST") continue;
            if (notePosLookup[notes[i]] > 6) break;
            noteColorLookup.Add(notes[i], posColorLookup[notePosLookup[notes[i]]]);
            //   noteColorLookup.Add(notes[i], Color.black);
        }
    }

    private IEnumerator OscillatePlatformOpacity()
    {
        while (true)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                if (i == currNoteIndex && isFalling && Song[i].name != "REST")
                {
                    LineRenderer renderer = platforms[i].gameObject.GetComponent<LineRenderer>();

                    renderer.startColor = new Color(renderer.startColor.r, renderer.startColor.g, renderer.startColor.b, Mathf.Lerp(renderer.startColor.a, 0.2f, 0.1f));
                    renderer.endColor = new Color(renderer.endColor.r, renderer.endColor.g, renderer.endColor.b, Mathf.Lerp(renderer.startColor.a, 0.2f, 0.1f));

                    SpriteRenderer sr = platforms[i].GetComponentInChildren<SpriteRenderer>();
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(renderer.startColor.a, 0.2f, 0.1f));

                    continue;
                }
                if (platforms[i] != null && Song[i].name != "REST")
                {
                    LineRenderer renderer = platforms[i].gameObject.GetComponent<LineRenderer>();

                    renderer.startColor = new Color(renderer.startColor.r, renderer.startColor.g, renderer.startColor.b, Mathf.Lerp(renderer.startColor.a, Mathf.Abs(Mathf.Sin(currTime) * .5f) + 0.5f, 0.1f));
                    renderer.endColor = new Color(renderer.endColor.r, renderer.endColor.g, renderer.endColor.b, Mathf.Lerp(renderer.startColor.a, Mathf.Abs(Mathf.Sin(currTime * 2) * .5f) + 0.5f, 0.1f));

                    SpriteRenderer sr = platforms[i].GetComponentInChildren<SpriteRenderer>();
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Lerp(renderer.startColor.a, Mathf.Abs(Mathf.Sin(currTime) * .5f) + 0.5f, 0.1f));
                }
            }
            yield return null;
        }
    }

    private IEnumerator PlayAudioCues()
    {
        while (currNoteIndex < Song.Count)
        {
            Note currNote = Song[currNoteIndex];
            audioPlayer.PlayNote(currNote.name, 1);
            audioPlayer.PlayTick();

            // Calculate the timing error (negative if we're ahead, positive if we're behind)
            float error = playAudioCueTime - currTime;
            float secPerBeat = 60 / BPM;

            // Determine how long the next delay should be
            float delay = secPerBeat * Mathf.Min(1, currNote.duration);
            playAudioCueTime += delay;

            //Add the current error from the next iteration's delay, so that errors don't build up.
            yield return new WaitForSeconds(delay + error);
        }
    }
}
