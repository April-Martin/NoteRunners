using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubWalls : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hey!");
        if (other.name == "Player")
        {
            float speedMultiplierReference = GameObject.Find("Game Controller").GetComponent<GameController>().speedMultiplier;

            GameObject.Find("Game Controller").GetComponent<GameController>().speedMultiplier = speedMultiplierReference - 2f <= 1f ? 1f : speedMultiplierReference - 2f;
        }
    }
}
