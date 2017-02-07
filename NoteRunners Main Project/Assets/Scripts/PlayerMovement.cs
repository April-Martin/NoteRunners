using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float LerpSpeed = 1f;
    private Dictionary<string, float> NotePosLookup;

    PitchTester pt;
	// Use this for initialization
	void Start () 
    {
        pt = GameObject.Find("Pitch Tester").GetComponent<PitchTester>();
        FillNoteLookup();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (pt.MainNote != null && NotePosLookup.ContainsKey(pt.MainNote))
        {
            Vector3 dest = new Vector3(transform.position.x, NotePosLookup[pt.MainNote]);
            Vector3 newPos = Vector3.Lerp(transform.position, dest, LerpSpeed);
            transform.position = newPos;
        }
	}


    void FillNoteLookup()
    {
        NotePosLookup = new Dictionary<string, float>();

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

    }


}
