using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    public float waveFactor = 4f;

    GameController gc;
    TextMesh thisTextMesh;
	// Use this for initialization
	void Start ()
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();
        thisTextMesh = this.GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        thisTextMesh.text = ((int)gc.Score).ToString();

        transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + (Mathf.Sin(gc.currTime) / waveFactor), this.transform.localPosition.z);
	}
}
