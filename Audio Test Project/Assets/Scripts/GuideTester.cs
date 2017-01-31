using UnityEngine;
using System.Collections;

public class GuideTester : MonoBehaviour
{

    private FrequencyGuide guide;
    public float testfreq;

    // Use this for initialization
    void Start()
    {
        guide = GetComponent<FrequencyGuide>();
        Debug.Log(guide.GetClosestNote(testfreq));


    }

    // Update is called once per frame
    void Update()
    {

    }
}
