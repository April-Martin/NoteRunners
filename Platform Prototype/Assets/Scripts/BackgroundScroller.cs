using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour {

	public float speed = 1f;
    private bool paused = false;
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
        if (paused) return;
		temp += speed * Time.deltaTime;
		renderer.material.mainTextureOffset = new Vector2 (Mathf.Repeat (temp, 1), 0);
	}

    public void Pause()
    {
        paused = true;
    }
    public void Play()
    {
        paused = false;
    }
}
