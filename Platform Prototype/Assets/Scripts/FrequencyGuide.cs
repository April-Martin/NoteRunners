using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrequencyGuide {

    private SortedList freqToNote = new SortedList();
    private Dictionary<string, int> noteToFreq = new Dictionary<string, int>();

	// Use this for initialization
	public FrequencyGuide() {
        freqToNote.Add(131, "C3");
        freqToNote.Add(147, "D3");
        freqToNote.Add(165, "E3");
        freqToNote.Add(175, "F3");
        freqToNote.Add(196, "G3");
        freqToNote.Add(220, "A3");
        freqToNote.Add(245, "B3");
        freqToNote.Add(261, "C4");
        freqToNote.Add(294, "D4");
        freqToNote.Add(330, "E4");
        freqToNote.Add(349, "F4");
        freqToNote.Add(392, "G4");
        freqToNote.Add(440, "A4");
        freqToNote.Add(494, "B4");
        freqToNote.Add(523, "C5");
        freqToNote.Add(587, "D5");
        freqToNote.Add(659, "E5");
        freqToNote.Add(698, "F5");
        freqToNote.Add(784, "G5");

        noteToFreq.Add("C3", 131);
        noteToFreq.Add("D3", 147);
        noteToFreq.Add("E3", 165);
        noteToFreq.Add("F3", 175);
        noteToFreq.Add("G3", 196);
        noteToFreq.Add("A3", 220);
        noteToFreq.Add("B3", 245);
        noteToFreq.Add("C4", 261);
        noteToFreq.Add("D4", 294);
        noteToFreq.Add("E4", 330);
        noteToFreq.Add("F4", 349);
        noteToFreq.Add("G4", 392);
        noteToFreq.Add("A4", 440);
        noteToFreq.Add("B4", 494);
        noteToFreq.Add("C5", 523);
        noteToFreq.Add("D5", 587);
        noteToFreq.Add("E5", 659);
        noteToFreq.Add("F5", 698);
        noteToFreq.Add("G5", 784);
    }
	

    public int GetFreq(string noteName)
    {
        int ret;
        if (noteToFreq.TryGetValue(noteName, out ret))
            return ret;
        else
            return -1;
    }


    public string GetClosestNote(float freq)
    {
        int tempkey = (int)freq;

		// If they hit the exact pitch, this is easy
        if (freqToNote.Contains(tempkey))
        {
            return (string) freqToNote.GetByIndex(freqToNote.IndexOfKey(tempkey));
        }


		// Otherwise, find the closest valid pitch (i.e., it's hard)

        freqToNote.Add(tempkey, null);

        string note = "";
        int index = freqToNote.IndexOfKey(tempkey);
        int diffLow = tempkey - (int)freqToNote.GetKey(index - 1);
        int diffHigh = (int)freqToNote.GetKey(index + 1) - tempkey;

        if (diffLow <= diffHigh)
            note = (string)freqToNote.GetByIndex(index - 1);
        else
            note = (string)freqToNote.GetByIndex(index+1);

        freqToNote.Remove(tempkey);
         
        return note;
    }
}
