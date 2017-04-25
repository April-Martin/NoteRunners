using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public float waveFactor = 4f;

    GameController gc;
    //TextMesh thisTextMesh;
    Text thisText;
	// Use this for initialization
	void Start ()
    {
        gc = GameObject.Find("Game Controller").GetComponent<GameController>();
       // thisTextMesh = this.GetComponent<TextMesh>();
        thisText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        thisText.text = ((int)gc.Score).ToString();
        //transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + (Mathf.Sin(gc.currTime) / waveFactor), this.transform.localPosition.z);
	}
}
