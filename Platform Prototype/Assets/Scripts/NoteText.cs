using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteText : MonoBehaviour {

	private PitchTester note;
	private TextMesh text;
	public bool isActive = true;

	// Use this for initialization
	void Start () {
		if (isActive) {
			note = GameObject.Find ("Pitch Tester").GetComponent<PitchTester> ();
			text = this.GetComponent<TextMesh> ();
		} else {
			Destroy (this.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isActive && !string.IsNullOrEmpty (note.MainNote)) {
			text.text = note.MainNote;
		} else if (!isActive) {
			Destroy (this.gameObject);
		}
	}
}
