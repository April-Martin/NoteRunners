using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreenScore : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.GetComponent<TextMesh>().text = GameGlobals.GlobalInstance.score + "";
	}
}
