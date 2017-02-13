using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    // Settings
    public bool DEBUG_InvincibleMode = false;
    public float TimeOnScreen;
    public string[] NotesAllowed;
    public float BPM;
    public float TransitionGracePeriod;
    public float SustainedGracePeriod;

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
    private bool isFalling = false;
    private bool isCorrect = true;

    private Dictionary<string, float> notePosLookup;
    private Dictionary<string, Color> noteColorLookup;
    private PitchTester pt;

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
        if (string.IsNullOrEmpty(playerPitch))
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
        if (playerPitch != targetNote)
        {
            // If they just deviated from the pitch, start keeping track of the time.
            if (isCorrect)
            {
                elapsedIncorrectTime = 0;
                isCorrect = false;
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
                elapsedIncorrectTime += Time.deltaTime;
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
                newNoteName = NotesAllowed[Random.Range(0, NotesAllowed.Length)];
            }
            float newNoteDur = Random.Range(1, 4);


            // Fill in note properties
            Note newNote = new Note(newNoteName, newNoteDur);
            newNote.yOffset = newNoteName == "REST" ? Song[lastNoteIndex].yOffset : notePosLookup[newNoteName];

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
        while(true)
        {
            // Wait till end of note
            float dur = Song[currNoteIndex].duration * 60 / BPM;
            Invoke("StartGracePeriod", dur - TransitionGracePeriod / 2);
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

    void StartGracePeriod()
    {
        isChecking = false;
        Invoke("EndGracePeriod", TransitionGracePeriod);
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


    internal void RespawnPlayer()
    {
        isFalling = false;

        //Halt momentum of player
        Player.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        float playerHeight = Player.GetComponent<SpriteRenderer>().bounds.size.y;
        float platformHeight = platform.GetComponent<SpriteRenderer>().bounds.size.y;
        Player.gameObject.transform.position = new Vector3(currPos, Song[currNoteIndex].yOffset + playerHeight/2 + platformHeight/2); 
        
        isChecking = false;
        Physics2D.IgnoreCollision(platforms[currNoteIndex], Player.GetComponent<Collider2D>(), false);

        Invoke("EndGracePeriod", 60/BPM);
    }

}
