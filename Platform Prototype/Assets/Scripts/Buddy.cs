using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buddy : MonoBehaviour
{
    public float wanderRange = .2f;
    public float wanderSpeed = 1f;
    public float wanderRandomFactor = .1f;
    public float xOffset = 1f;
    public float InterpolationFactorX = 0.05f;
    public float InterpolationFactorY = 0.05f;

    public float wanderValue = 0;

    PitchTester pt;
    GameController gc;

    SpriteRenderer spriteRenderer;
    Transform playerTransform;

    TrailRenderer trailRenderer;

    Color startGoal, endGoal = Color.black;

    string lastNote;
	// Use this for initialization
	void Start ()
    {
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.startColor = Color.black;
        trailRenderer.endColor = Color.white;
		this.GetComponentInChildren<NoteText> ().isActive = gc.isTextActive;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //  Sorry Monish xD xD
        //spriteRenderer.color = Random.ColorHSV();

        //Remove this line for buddy trail effect. Buddy will struggle to catch up to the player as the player moves faster. Keep this line to lock at xOffset.
//        transform.position = new Vector3(playerTransform.position.x + xOffset, transform.position.y, 0f);

        Vector3 targetPos;
        float test = Random.RandomRange(0f, 1f);
        if (test < .01f)
        {
            wanderValue = Random.RandomRange(-wanderRandomFactor, wanderRandomFactor);
        }

        float targetXPos = playerTransform.position.x + xOffset + (wanderRange + wanderRange*wanderValue) * ((Mathf.Sin(Time.time * wanderSpeed) + (wanderValue)));
        float targetYPos = 1f;

        if (!string.IsNullOrEmpty(pt.MainNote))
        {
            string currentNote = pt.MainNote;
            if(lastNote != currentNote)
            {
                endGoal = trailRenderer.startColor;
                startGoal = gc.noteColorLookup[currentNote];

                lastNote = currentNote;
            }

            trailRenderer.endColor = Color.Lerp(trailRenderer.endColor, endGoal, 0.05f);
            trailRenderer.startColor = Color.Lerp(trailRenderer.startColor, startGoal, 0.05f);

			if (!gc.isFalling)
			{
				targetYPos = gc.notePosLookup[currentNote];
			}
        }
        else
        {
            targetYPos = transform.position.y;
            //transform.Rotate(new Vector3(0, 0, transform.position.y * 15)); Monish's dream is dead.
            
        }
 //       transform.position = new Vector3(xPos, transform.position.y, 0f);
		float newYPos = newYPos = Mathf.Lerp(transform.position.y, transform.position.y, InterpolationFactorY);
		float newXPos = Mathf.Lerp(transform.position.x, targetXPos, InterpolationFactorX);
		if (!gc.isFalling)
		{
        	newYPos = Mathf.Lerp(transform.position.y, targetYPos, InterpolationFactorY);
		}
        transform.position = new Vector3(newXPos, newYPos, 0);
//        transform.position = Vector3.Lerp(transform.position, targetPos, InterpolationFactor);
    }
}
