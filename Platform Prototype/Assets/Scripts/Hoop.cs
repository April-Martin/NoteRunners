/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.gameObject.name == "Player")
        {
            float speedMultiplierReference = GameObject.Find("Game Controller").GetComponent<GameController>().speedMultiplier;
            GameObject.Find("Game Controller").GetComponent<GameController>().speedMultiplier += 1f;
        }
    }

}
*/