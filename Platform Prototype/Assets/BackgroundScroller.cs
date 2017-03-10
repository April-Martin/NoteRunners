using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour {

	public float speed = 1f;

	float temp = 0f;

	MeshRenderer renderer;

	// Use this for initialization
	void Start ()
	{
		renderer = GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		temp += speed * Time.deltaTime;
		renderer.material.mainTextureOffset = new Vector2 (Mathf.Repeat (temp, 1), 0);
	}
}
