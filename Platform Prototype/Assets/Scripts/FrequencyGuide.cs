using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrequencyGuide {

    private SortedList freqToNote = new SortedList();

    internal Dictionary<string, int> noteToFreq = new Dictionary<string, int>();

	// Use this for initialization
	public FrequencyGuide() {
        freqToNote.Add(82, "E2");
        freqToNote.Add(87, "F2");
        //freqToNote.Add(93, "F#2");
        freqToNote.Add(98, "G2");
        //freqToNote.Add(104, "G#2");
        freqToNote.Add(110, "A2");
        //freqToNote.Add(117, "A#2");
        freqToNote.Add(123, "B2");
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
        //freqToNote.Add(831, "G#5");
        freqToNote.Add(880, "A5");
        //freqToNote.Add(932, "A#5");
        freqToNote.Add(988, "B5");
        freqToNote.Add(1047, "C6");

        noteToFreq.Add("E2", 82);
        noteToFreq.Add("F2", 87);
        //noteToFreq.Add("F#2", 93);
        noteToFreq.Add("G2", 98);
        //noteToFreq.Add("G#2",104);
        noteToFreq.Add("A2", 110);
        //noteToFreq.Add("A#2", 117);
        noteToFreq.Add("B2", 123);
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
        //noteToFreq.Add("G#5", 831);
        noteToFreq.Add("A5", 880);
        //noteToFreq.Add("A#5", 932);
        noteToFreq.Add("B5", 988);
        noteToFreq.Add("C6", 1047);
    }
	

    public int GetFreq(string noteName)
    {
        int ret;
        if (noteToFreq.TryGetValue(noteName, out ret))
            return ret;
        else
            return -1;
    }

	public List<string> GetLeniencyRange ( string targetNote, float range )
	{
		List<string> notes = new List<string> ();
		if (range == 0)
			return notes;

		int targetIndex = freqToNote.IndexOfKey (targetNote);
		for (int i = 1; i <= range; i++) 
		{
			notes.Add ((string) freqToNote.GetByIndex (targetIndex + i));
			notes.Add ((string) freqToNote.GetByIndex (targetIndex - i));
		}
		return notes;
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
