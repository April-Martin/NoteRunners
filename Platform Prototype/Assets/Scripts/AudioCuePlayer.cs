using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AudioCuePlayer : MonoBehaviour {

    public AudioClip C2, D2, E2, F2, G2,
        A2, B2, C3, D3, E3, F3, G3,
        A3, B3, C4, D4, E4, F4, G4,
        A4, B4, C5, D5, E5, F5, G5,
        A5, B5, C6, TICK;
    private Dictionary<string, AudioClip> noteAudioLookup;
    private AudioSource src;


	// Use this for initialization
	void Awake () {
        src = GetComponent<AudioSource>();
        noteAudioLookup = new Dictionary<string, AudioClip>
	    {
		    {"E2", E2}, {"F2", F2}, {"G2", G2}, {"A2", A2}, {"B2", B2}, {"C3", C3}, {"D3", D3},
		    {"E3", E3}, {"F3", F3}, {"G3", G3}, {"A3", A3}, {"B3", B3}, {"C4", C4}, {"D4", D4},
		    {"E4", E4}, {"F4", F4}, {"G4", G4}, {"A4", A4}, {"B4", B4}, {"C5", C5}, {"D5", D5}, 
            {"E5", E5}, {"F5", F5}, {"G5", G5}, {"A5", A5}, {"B5", B5}, {"C6", C6}
	    };
	}
	
    public void PlayNote(string noteName, float vol)
    {
        if (noteName == "REST") return;
        src.PlayOneShot(noteAudioLookup[noteName], vol);
    }

    public void PlayTick()
    {
        src.PlayOneShot(TICK);
    }
}
