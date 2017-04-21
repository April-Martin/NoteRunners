using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AudioCuePlayer : MonoBehaviour {

    public AudioClip C2, Csharp2, D2, Dsharp2, E2, F2, Fsharp2, G2, Gsharp2,
        A2, Asharp2, B2, C3, Csharp3, D3, Dsharp3, E3, F3, Fsharp3, G3, Gsharp3,
        A3, Asharp3, B3, C4, Csharp4, D4, Dsharp4, E4, F4, Fsharp4, G4, Gsharp4,
        A4, Asharp4, B4, C5, Csharp5, D5, Dsharp5, E5, F5, Fsharp5, G5, Gsharp5,
        A5, Asharp5, B5, C6, TICK;
    private Dictionary<string, AudioClip> noteAudioLookup;
    private AudioSource src;


	// Use this for initialization
	void Awake () {
        src = GetComponent<AudioSource>();
        noteAudioLookup = new Dictionary<string, AudioClip>
	    {
		    {"E2", E2}, {"F2", F2}, {"F#2", Fsharp2}, {"G2", G2}, {"G#2", Gsharp2}, {"A2", A2}, {"A#2", Asharp2}, {"B2", B2}, {"C3", C3}, {"C#3", Csharp3}, {"D3", D3}, {"D#3", Dsharp3},
									{"Gf2", Fsharp2}, 				{"Af2", Gsharp2},			{"Bf2", Asharp2},							{"Df3", Csharp3}, 			{"Ef3", Dsharp3},
		    {"E3", E3}, {"F3", F3}, {"F#3", Fsharp3}, {"G3", G3}, {"G#3", Gsharp3}, {"A3", A3}, {"A#3", Asharp3}, {"B3", B3}, {"C4", C4}, {"C#4", Csharp4}, {"D4", D4}, {"D#4", Dsharp4},
									{"Gf3", Fsharp3}, 				{"Af3", Gsharp3},			{"Bf3", Asharp3},							{"Df4", Csharp4}, 			{"Ef4", Dsharp4},
		    {"E4", E4}, {"F4", F4}, {"F#4", Fsharp4}, {"G4", G4}, {"G#4", Gsharp4}, {"A4", A4}, {"A#4", Asharp4}, {"B4", B4}, {"C5", C5}, {"C#5", Csharp5}, {"D5", D5}, {"D#5", Dsharp5},
									{"Gf4", Fsharp4}, 				{"Af4", Gsharp4},			{"Bf4", Asharp4},							{"Df5", Csharp5}, 			{"Ef5", Dsharp5},
            {"E5", E5}, {"F5", F5}, {"F#5", Fsharp5}, {"G5", G5}, {"G#5", Gsharp5}, {"A5", A5}, {"A#5", Asharp5}, {"B5", B5}, {"C6", C6},
									{"Gf5", Fsharp5}, 				{"Af5", Gsharp5},			{"Bf5", Asharp5}
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
