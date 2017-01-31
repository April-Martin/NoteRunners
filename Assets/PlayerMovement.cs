using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float Speed = 1f;

	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 newPos = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + (Input.GetAxis("Vertical") * Speed), -0.5f,4.5f), transform.position.z);
        transform.position = newPos;
	}
}
