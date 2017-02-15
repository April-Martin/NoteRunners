using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrequencyGuide {

    private SortedList freqToNote = new SortedList();
    private Dictionary<string, int> noteToFreq = new Dictionary<string, int>();

	// Use this for initialization
	public FrequencyGuide() {
		// Do we want to go the full general vocal range of E2 - C6 (Low Bass to High Soprano)?
		// Or do we want a smaller range and work with transposing lower/higher notes?
        freqToNote.Add(131, "C3");
        //freqToNote.Add(139, "C#3");
        freqToNote.Add(147, "D3");
        //freqToNote.Add(156, "D#3");
        freqToNote.Add(165, "E3");
        freqToNote.Add(175, "F3");
        //freqToNote.Add(185, "F#3");
        freqToNote.Add(196, "G3");
        //freqToNote.Add(208, "G#3");
        freqToNote.Add(220, "A3");
        //freqToNote.Add(233, "A#3");
        freqToNote.Add(245, "B3");
        freqToNote.Add(261, "C4");
        //freqToNote.Add(277, "C#4");
        freqToNote.Add(294, "D4");
        //freqToNote.Add(311, "D#4");
        freqToNote.Add(330, "E4");
        freqToNote.Add(349, "F4");
        //freqToNote.Add(370, "F#4");
        freqToNote.Add(392, "G4");
        //freqToNote.Add(415, "G#4");
        freqToNote.Add(440, "A4");
        //freqToNote.Add(466, "A#4");
        freqToNote.Add(494, "B4");
        freqToNote.Add(523, "C5");
        //freqToNote.Add(554, "C#5");
        freqToNote.Add(587, "D5");
        //freqToNote.Add(622, "D#5");
        freqToNote.Add(659, "E5");
        freqToNote.Add(698, "F5");
        //freqToNote.Add(740, "F#5");
        freqToNote.Add(784, "G5");

        noteToFreq.Add("C3", 131);
        //noteToFreq.Add("C#3", 139);
        noteToFreq.Add("D3", 147);
        //noteToFreq.Add("D#3", 156);
        noteToFreq.Add("E3", 165);
        noteToFreq.Add("F3", 175);
        //noteToFreq.Add("F#3", 185);
        noteToFreq.Add("G3", 196);
        //noteToFreq.Add("G#3", 208);
        noteToFreq.Add("A3", 220);
        //noteToFreq.Add("A#3", 233);
        noteToFreq.Add("B3", 245);
        noteToFreq.Add("C4", 261);
        //noteToFreq.Add("C#4", 277);
        noteToFreq.Add("D4", 294);
        //noteToFreq.Add("D#4", 311);
        noteToFreq.Add("E4", 330);
        noteToFreq.Add("F4", 349);
        //noteToFreq.Add("F#4", 370);
        noteToFreq.Add("G4", 392);
        //noteToFreq.Add("G#4", 415);
        noteToFreq.Add("A4", 440);
        //noteToFreq.Add("A#4", 466);
        noteToFreq.Add("B4", 494);
        noteToFreq.Add("C5", 523);
        //noteToFreq.Add("C#5", 554);
        noteToFreq.Add("D5", 587);
        //noteToFreq.Add("D#5", 622);
        noteToFreq.Add("E5", 659);
        noteToFreq.Add("F5", 698);
        //noteToFreq.Add("F#5", 740);
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

        // If the index is first in the list, return the next note.
        if (index == 0)
        {
            note = (string)freqToNote.GetByIndex(index + 1);
            freqToNote.Remove(tempkey);
            return note;
        }

        // If the index is last in the list, return previous note.
        else if (index == freqToNote.Count - 1)
        {
            note = (string)freqToNote.GetByIndex(index - 1);
            freqToNote.Remove(tempkey);
            return note;
        }

        int diffLow = tempkey - (int)freqToNote.GetKey(index - 1);
        int diffHigh = (int)freqToNote.GetKey(index + 1) - tempkey;

        if (diffLow <= diffHigh)
            note = (string)freqToNote.GetByIndex(index - 1);
        else
            note = (string)freqToNote.GetByIndex(index + 1);

        freqToNote.Remove(tempkey);
         
        return note;
    }
}
