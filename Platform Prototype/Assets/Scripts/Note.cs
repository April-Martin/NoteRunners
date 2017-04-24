using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note {

    public string name;
    public float duration;
    public float yOffset;
    public float actualTime;
    public string label;

    public Note(string _name, float _duration)
    {
        name = _name;
        duration = _duration;
        yOffset = 0;
        actualTime = 0;
        label = _name;
    }


}
