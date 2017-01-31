using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VolumeTester : MonoBehaviour
{
    AudioSource src;
    float[] samples = new float[128];

    void Start()
    {
        src = GetComponent<AudioSource>();
        src.clip = Microphone.Start(null, true, 10, 44100);
        src.loop = true;
        while (Microphone.GetPosition(null) <= 0) {}    // Use default mic
        src.Play();
    }

    void Update()
    {
        src.GetOutputData(samples, 0);

        // Average absolute val of samples
        float avg = 0;
        for (int i=0; i<samples.Length; i++)
        {
            avg += Mathf.Abs(samples[i]);
        }
        avg /= (float) samples.Length;

        Debug.Log("VOL:   " + avg*100);
    }
}