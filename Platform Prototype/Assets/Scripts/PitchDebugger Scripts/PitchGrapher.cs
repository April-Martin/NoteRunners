using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
public class PitchGrapher : MonoBehaviour
{
    // Public vars
    public int graphingMult = 100;

    public int minGraphedFreq;
    public int maxGraphedFreq;
    public int labelSpacing;

    public float volThreshold = 0.0f;
    public float specFlatnessThreshold = 0.6f;
    public String MainNote;
    public int targetFreq;
    public float MagnetismPower = 2;


    // Settings
    private int samplerate;
    private const int bins = 8192;
    internal int minFreq = 100;
    internal int maxFreq = 5000;

    public LineRenderer linePrefab;
    public TextMesh labelPrefab;
    private AudioSource src;
    private float maxX;
    private float maxY;

    internal FrequencyGuide guide = new FrequencyGuide();
    private float[] volSamples = new float[64];
    private float[] freqSamples = new float[bins];
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<TextMesh> labels = new List<TextMesh>();

    void Start()
    {
        LineRenderer axis = transform.GetChild(0).GetComponent<LineRenderer>();
        maxX = axis.GetPosition(1).x;
        axis = transform.GetChild(1).GetComponent<LineRenderer>();
        maxY = axis.GetPosition(1).y;

        // Start realtime audio input / playback
        src = GetComponent<AudioSource>();
        samplerate = AudioSettings.outputSampleRate;
        src.clip = Microphone.Start(null, true, 10, samplerate);
        src.loop = true;
        while (Microphone.GetPosition(null) <= 0) { }   // Wait for recording to start
        src.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Running");
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

        float[] hpsFreqSamples = new float[freqSamples.Length / 5];
        for (int i = 1; i < hpsFreqSamples.Length; i++)
        {
            hpsFreqSamples[i] = freqSamples[i] * freqSamples[i * 2] * freqSamples[i * 3] * freqSamples[i * 4] * freqSamples[i * 5];
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Break");
        }


        // Loop over frequency bins within human range:
        // 	 - Calculate spectral flatness to determine if sound is noise/tone
        //   - Find out which frequency bin, within desired range, has the strongest signal
        int minBin = minFreq * (2 * bins) / samplerate;
        int maxBin = maxFreq * (2 * bins) / samplerate;

        int minBoostedBin = (targetFreq * bins) / samplerate;
        int targetBin = (targetFreq * 2 * bins) / samplerate;
        int maxBoostedBin = (targetFreq * 4 * bins) / samplerate;

        double geometricMean = 0;
        float arithmeticMean = 0;
        int maxIndex = 0;
        float maxVal = 0.0f;
        float magnetismMultiplier = 0.0f;

        for (int i = minBin; i < maxBin && i < hpsFreqSamples.Length; i++)
        {
            // Update sums
            if (hpsFreqSamples[i] != 0)
            {
                geometricMean += Mathf.Log(hpsFreqSamples[i]);
                arithmeticMean += hpsFreqSamples[i];
            }

            //Find ratio of given frequency to target frequency.
            //if (i >= minBoostedBin && i <= maxBoostedBin)
            {
       //         magnetismMultiplier = i > targetBin ? (i - targetBin) / (float) (maxBoostedBin - targetBin) : (targetBin - i) / (float)(targetBin - minBoostedBin);
         //       freqSamples[i] += MagnetismPower * (1 - magnetismMultiplier) * freqSamples[i];
            }


            // Update max frequency
            if (hpsFreqSamples[i] >= maxVal)
            {
                maxIndex = i;
                maxVal = hpsFreqSamples[i];
            }
        }

        arithmeticMean /= (maxBin - minBin);
        geometricMean /= (maxBin - minBin);
        geometricMean = Math.Exp(geometricMean);

        // Exclude audio that's too noisy (i.e., if the player isn't singing)
        if (maxIndex == 0 || (geometricMean / arithmeticMean > specFlatnessThreshold))
        {
            MainNote = "";
            return;
        }

        // Log frequency
        float frequency = (float)maxIndex * samplerate / (2 * bins);

        // Log note
        MainNote = guide.GetClosestNote(frequency);


        /* 
         * - - - - - - - GRAPH RESULTS OF FFT - - - - - - - - - - - 
         */

        // Wipe previous graph contents
        foreach (LineRenderer l in lines)
        {
            if (l != null)
            {
                Destroy(l.gameObject);
            }
        }
        foreach (TextMesh label in labels)
        {
            if (label != null)
            {
                Destroy(label.gameObject);
            }
        }
        lines.Clear();
        labels.Clear();


        // Figure out spacing for notches along the x-axis
        minBin = minGraphedFreq * (2 * bins) / samplerate;
        maxBin = maxGraphedFreq * (2 * bins) / samplerate;
        float notchInterval = maxX / (maxBin - minBin);

        // Loop over frequency bins within graphable range
        for (int i = minBin; i < maxBin && i<hpsFreqSamples.Length; i++)
        {
            float binVal = hpsFreqSamples[i];

            if (binVal != 0)
            {
                // Scale the y-axis so that the maximum value reaches the top
                float yPos = (binVal / maxVal) * maxY;
             //   float yPos = binVal * graphingMult * maxY;

                LineRenderer line = Instantiate<LineRenderer>(linePrefab);
                line.SetPosition(0, new Vector3(notchInterval * i, 0));
                line.SetPosition(1, new Vector3(notchInterval * i, yPos));
                lines.Add(line);
            }

            if (i % labelSpacing == 0)
            {
                TextMesh label = Instantiate<TextMesh>(labelPrefab);
                label.transform.position = new Vector3(notchInterval * i, -1);
                label.text = (i * samplerate / (2 * bins)).ToString();
                labels.Add(label);
            }

        }

    }
}
