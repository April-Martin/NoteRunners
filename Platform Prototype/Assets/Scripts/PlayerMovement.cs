using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float ArrowSpeed = .2f;
    public float LerpSpeed = 1f;
    private Animator anim;
	private bool isSpinning = false;

    internal Dictionary<string, float> NotePosLookup;

    GameController gc;

    PitchTester pt;
	// Use this for initialization
	void Start () 
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>(); 
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        anim = GetComponent<Animator>();
        FillNoteLookup();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            Vector3 newPos = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + (Input.GetAxis("Vertical") * ArrowSpeed), -0.5f, 4.5f), transform.position.z);
            transform.position = newPos;
        }
		if (isSpinning)
			this.transform.Rotate(new Vector3(0, 0, Time.deltaTime * -45));
	}

    public void PauseAnimation()
    {
        anim.enabled = false;
    }

	public void StartPlayerSpinning()
	{
		isSpinning = true;
        //a
	}

	public void ResetPlayerRotation()
	{
		isSpinning = false;
		transform.localRotation = Quaternion.identity;
	}


    public void PlayAnimation()
    {
        anim.enabled = true;
    }

	public void SetAnimSpeed(float speed)
	{
		anim.speed = speed;
	}

    void OnBecameInvisible()
    {
        gc.RespawnPlayer();
    }

    void FillNoteLookup()
    {
        NotePosLookup = new Dictionary<string, float>();

        NotePosLookup.Add("E2", -7f);
        NotePosLookup.Add("F2", -6.5f);
        NotePosLookup.Add("G2", -6f);
        NotePosLookup.Add("A2", -5.5f);
        NotePosLookup.Add("B2", -5f);
        NotePosLookup.Add("C3", -4.5f);
        NotePosLookup.Add("D3", -4f);
        NotePosLookup.Add("E3", -3.5f);
        //Start of original list. 
        NotePosLookup.Add("F3", -3f);
        NotePosLookup.Add("G3", -2.5f);
        NotePosLookup.Add("A3", -2f);
        NotePosLookup.Add("B3", -1.5f);
        NotePosLookup.Add("C4", -1f);
        NotePosLookup.Add("D4", -.5f);
        NotePosLookup.Add("E4", 0f);
        NotePosLookup.Add("F4", .5f);
        NotePosLookup.Add("G4", 1f);
        NotePosLookup.Add("A4", 1.5f);
        NotePosLookup.Add("B4", 2f);
        NotePosLookup.Add("C5", 2.5f);
        NotePosLookup.Add("D5", 3f);
        NotePosLookup.Add("E5", 3.5f);
        NotePosLookup.Add("F5", 4f);
        NotePosLookup.Add("G5", 4.5f);
        //End of original list.
        NotePosLookup.Add("A5", 5f);
        NotePosLookup.Add("B5", 5.5f);
        NotePosLookup.Add("C6", 6f);

    }


}
