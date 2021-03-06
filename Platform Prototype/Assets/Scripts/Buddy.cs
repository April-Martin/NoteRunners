﻿using System.Collections;
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
    ParticleSystem psInner;
    ParticleSystem psOuter;
    ParticleSystem psTrail;
    float psInner_startSpeed;
    float psInner_startSize;
    float psOuter_startSpeed;
    float psOuter_startSize;
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
        trailRenderer.sortingOrder = 2;
        trailRenderer.startColor = Color.black;
        trailRenderer.endColor = Color.white;
		this.GetComponentInChildren<NoteText> ().isActive = gc.isTextActive;

        psInner = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
        psOuter = transform.GetChild(1).GetChild(1).GetComponent<ParticleSystem>();
        psTrail = transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<ParticleSystem>();

        var mainInner = psInner.main;
        psInner_startSize = mainInner.startSizeMultiplier;
        psInner_startSpeed = mainInner.startSpeedMultiplier;
        var mainOuter = psOuter.main;
        psOuter_startSize = mainOuter.startSizeMultiplier;
        psOuter_startSpeed = mainOuter.startSpeedMultiplier;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //  Sorry Monish xD xD
        //spriteRenderer.color = Random.ColorHSV();

        //Remove this line for buddy trail effect. Buddy will struggle to catch up to the player as the player moves faster. Keep this line to lock at xOffset.
//        transform.position = new Vector3(playerTransform.position.x + xOffset, transform.position.y, 0f);

        float test = Random.RandomRange(0f, 1f);
        if (test < .01f)
        {
            wanderValue = Random.RandomRange(-wanderRandomFactor, wanderRandomFactor);
        }

        float targetXPos = playerTransform.position.x + xOffset + (wanderRange + wanderRange * wanderValue) * ((Mathf.Sin(Time.time * wanderSpeed) + (wanderValue)));
       // float targetXPos = playerTransform.position.x + xOffset + wanderRange * Mathf.Sin(Time.time * wanderSpeed);
       // float targetXPos = playerTransform.position.x + xOffset;
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

            //trailRenderer.endColor = Color.Lerp(trailRenderer.endColor, endGoal, 0.05f);
            //trailRenderer.startColor = Color.Lerp(trailRenderer.startColor, startGoal, 0.05f);

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
		float newYPos = Mathf.Lerp(transform.position.y, transform.position.y, InterpolationFactorY);
		float newXPos = Mathf.Lerp(transform.position.x, targetXPos, InterpolationFactorX);
		if (!gc.isFalling)
		{
        	newYPos = Mathf.Lerp(transform.position.y, targetYPos, InterpolationFactorY);
		}
        transform.position = new Vector3(newXPos, newYPos, 0);
//        transform.position = Vector3.Lerp(transform.position, targetPos, InterpolationFactor);
    }

    public void SetParticleBPM(float BPM)
    {
        float interval = 60 / BPM;
        var mainInner = psInner.main;
        mainInner.duration = interval;
        var mainOuter = psOuter.main;
        mainOuter.duration = interval;
        psInner.Play();
        psOuter.Play();
    }

    public void SetParticleIntensity(float percentageOfDefault)
    {
        var mainInner = psInner.main;
        mainInner.startSizeMultiplier = psInner_startSize * percentageOfDefault;
        mainInner.startSpeedMultiplier = psInner_startSpeed * percentageOfDefault;
        var mainOuter = psOuter.main;
        mainOuter.startSizeMultiplier = psOuter_startSize * percentageOfDefault;
        mainOuter.startSpeedMultiplier = psOuter_startSpeed * percentageOfDefault;
    }

    public void SetColor(Color c)
    {
        spriteRenderer.color = c;

        var mainInner = psInner.main;
        mainInner.startColor = new Color(c.r * 1.2f, c.g * 1.2f, c.b * 1.2f);

        var mainTrail = psTrail.main;
        mainTrail.startColor = new Color(c.r * .5f, c.g * .5f, c.b * .5f, 1);

        return;
    }

}
