using UnityEngine;
using UnityEditor;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioClip))]
public class ClipPitchTester : MonoBehaviour
{
    int samplerate;
    const int bins = 8192;
    const int minFreq = 60;
    const int maxFreq = 2000;

    AudioSource src;
    AudioClip clip;
    float[] volSamples = new float[64];
    float[] freqSamples = new float[bins];


    // Use this for initialization
    void Start()
    {
        src = GetComponent<AudioSource>();
        src.loop = true;
        src.Play();

        samplerate = AudioSettings.outputSampleRate;
        Debug.Log(samplerate);
    }

    // Update is called once per frame
    void Update()
    {



        /*
        src.GetOutputData(volSamples, 0);
        float avg = 0;
        for (int i = 0; i < volSamples.Length; i++)
        {
            avg += Mathf.Abs(volSamples[i]);
        }
        avg = avg / volSamples.Length;
        if (avg * 100 < 5)
        {
            return;
        }

        */
        src.GetSpectrumData(freqSamples, 0, FFTWindow.BlackmanHarris);

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Break");
        }
//        int minBin = minFreq * bins / samplerate;
//        int maxBin = maxFreq * bins / samplerate;

        int maxIndex = 0;
        float maxVal = 0.0f;

        // Find out which frequency bin, within range, has the strongest signal
//        for (int i = minBin; i < maxBin; i++)
        for (int i = 0; i < freqSamples.Length / 2; i++)
        {
            if (freqSamples[i] >= maxVal)
            {
                maxIndex = i;
                maxVal = freqSamples[i];
            }
        }

        float frequency = (float)(maxIndex - 1) * samplerate / (2*bins);
        Debug.Log("lower frequency = " + frequency);
        frequency = (float)maxIndex * samplerate / (2 * bins);
        Debug.Log("frequency = " + frequency);
        frequency = (float)(maxIndex + 1) * samplerate / (2 * bins);
        Debug.Log("upper frequency = " + frequency);
    }
}
