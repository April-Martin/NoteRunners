using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
public class PitchTester : MonoBehaviour {
//    public float volThreshold = 1.0f;
	public float volThreshold = 0.0f;
	public float specFlatnessThreshold = 0.2f;

    private int samplerate;
    private const int bins = 8192;

    internal int minFreq = 75;
    internal int maxFreq = 1075;

    public String MainNote;
    public Text text;
    internal FrequencyGuide guide = new FrequencyGuide();
    private AudioSource src;
    private float[] volSamples = new float[64];
    private float[] freqSamples = new float[bins];


	void Start () {
        src = GetComponent<AudioSource>();

        // Start realtime audio input / playback
        //samplerate = AudioSettings.outputSampleRate;
        samplerate = 8000;
        src.clip = Microphone.Start(null, true, 10, samplerate);
        src.loop = true;
        while (Microphone.GetPosition(null) <= 0) { }   // Wait for recording to start
        src.Play();
	}
	
	// Update is called once per frame
	void Update () {

        // Check if the volume is above threshold
        src.GetOutputData(volSamples, 0);
        float avg = 0;
        for (int i = 0; i < volSamples.Length; i++)
        {
            avg += Mathf.Abs(volSamples[i]);
        }
        avg = avg / volSamples.Length;
        if (avg * 100 < volThreshold) 
        {
            MainNote = "";
            return;
        }
        
        // Get frequency information
        src.GetSpectrumData(freqSamples, 0, FFTWindow.BlackmanHarris);

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Break");
        }

		// Loop over frequency bins within human range:
		// 	 - Calculate spectral flatness to determine if sound is noise/tone
        //   - Find out which frequency bin, within desired range, has the strongest signal
		int minBin = minFreq * (2 * bins) / samplerate;
		int maxBin = maxFreq * (2 * bins) / samplerate;

		double geometricMean = 0;
		float arithmeticMean = 0;
        int maxIndex = 0;
        float maxVal = 0.0f;

        for (int i=minBin; i<maxBin; i++)
        {
			// Update sums
			if (freqSamples[i] != 0)
			{
				geometricMean += Mathf.Log(freqSamples [i]);
				arithmeticMean += freqSamples [i];
			}

			// Update max frequency
            if (freqSamples[i] >= maxVal)
            {
                maxIndex = i;
                maxVal = freqSamples[i];
            }
        }
			
		arithmeticMean /= (maxBin - minBin);
		geometricMean /= (maxBin - minBin);
		geometricMean = Math.Exp (geometricMean);

        // Exclude frequencies outside of range
		if (maxIndex==0 || (geometricMean/arithmeticMean > specFlatnessThreshold))
        {
            MainNote = "";
            return;
        }

        // Log frequency
        float frequency = (float)maxIndex * samplerate / (2 * bins);

        // Log note
        MainNote = guide.GetClosestNote(frequency);
        //Debug.Log(MainNote);
	}
}
