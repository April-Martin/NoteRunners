using UnityEngine;
using System.Collections;

public class FrequencyGuide : MonoBehaviour {

    private SortedList guide = new SortedList();

	// Use this for initialization
	void Start () {
        guide.Add(131, "C3");
        guide.Add(147, "D3");
        guide.Add(165, "E3");
        guide.Add(175, "F3");
        guide.Add(196, "G3");
        guide.Add(220, "A3");
        guide.Add(245, "B3");
        guide.Add(261, "C4");
        guide.Add(294, "D4");
        guide.Add(330, "E4");
        guide.Add(349, "F4");
        guide.Add(392, "G4");
        guide.Add(440, "A4");
        guide.Add(494, "B4");
        guide.Add(523, "C5");
        guide.Add(587, "D5");
        guide.Add(659, "E5");
        guide.Add(698, "F5");
        guide.Add(784, "G5");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public string GetClosestNote(float freq)
    {
        int tempkey = (int)freq;
        if (guide.Contains(tempkey))
        {
            return (string) guide.GetByIndex(guide.IndexOfKey(tempkey));
        }

        guide.Add(tempkey, null);

        string note = "";
        int index = guide.IndexOfKey(tempkey);
        int diffLow = tempkey - (int)guide.GetKey(index - 1);
        int diffHigh = (int)guide.GetKey(index + 1) - tempkey;

        if (diffLow <= diffHigh)
            note = (string)guide.GetByIndex(index - 1);
        else
            note = (string)guide.GetByIndex(index+1);

        guide.Remove(tempkey);
         
        return note;
    }
}
