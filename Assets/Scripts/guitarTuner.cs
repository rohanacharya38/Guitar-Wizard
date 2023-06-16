
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class guitarTuner : MonoBehaviour
{

    AudioSource audioSource;
    float lowCutoff;
    float highCutoff;
    SortedDictionary<float, string> freqChordPairs = new SortedDictionary<float, string>();
    const int arraySize = 8192;
    const int sampleCount = 8192;
    float targetString;
    float[] spectrumData;
    float[] samples;
    public TMP_Text stringName;
    public TMP_Text offset;
    public AudioSource circleAudioSource;
    public Outline circleOutline;
    private float refValue = 0.1f;
    public GameObject up;
    public GameObject down;
    Color initialColor;
    private IEnumerator updateUIElements(int returnValue)
    {    //Debug.Log(freqChordPairs[targetString] + "is Pressed");

        stringName.text = freqChordPairs[targetString];
        offset.text = returnValue.ToString();
        if (returnValue == 0)
        {
            circleOutline.effectColor = Color.green;
            //circleAudioSource.Play();
        }
        else
        {
            circleOutline.effectColor = initialColor;
        }
        if (returnValue > 0)
        {
            offset.text = "+" + returnValue.ToString("0");
            // Debug.Log("Tune Down");
            down.SetActive(true);

        }
        else if (returnValue < 0)
        {
            offset.text = (returnValue).ToString("0");
            //  Debug.Log("Tune Up");
            up.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);
        up.SetActive(false);
        down.SetActive(false);

    }
    // Start is called before the first frame update
    void Start()
    {

        lowCutoff = convertFrequencyToIndex(30, arraySize);
        highCutoff = convertFrequencyToIndex(400, arraySize);
        freqChordPairs[329.63f] = "E4";
        freqChordPairs[246.94f] = "B";
        freqChordPairs[196.00f] = "G";
        freqChordPairs[146.83f] = "D";
        freqChordPairs[110.00f] = "A";
        freqChordPairs[82.41f] = "E";
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start(null, true, 1, 44100);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }
        audioSource.Play();
        spectrumData = new float[arraySize];
        samples = new float[sampleCount];
        initialColor = circleOutline.effectColor;

    }

    // Update is called once per frame
    void Update()
    {

        float dbValue = getVolume();
        if (dbValue < -30f)
        {
            return;
        }

        float minDifference = getOffsetFromTargetString();
        if (minDifference < 20f)
        {
            StartCoroutine(updateUIElements((int)minDifference));
        }
    }


    float convertFrequencyToIndex(float frequency, int sizeOfArray)
    {
        return 2 * sizeOfArray * frequency / (AudioSettings.outputSampleRate);
    }


    //changes the current color to default color to imitate the action of a eraser
    float getOffsetFromTargetString()
    {
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        int maxN = 0;
        float maxValue = 0;

        for (int i = (int)lowCutoff; i < (int)highCutoff; i++)
        {
            if (i < spectrumData.Length && spectrumData[i] > maxValue)
            {
                maxValue = spectrumData[i];
                maxN = i;
            }
        }
        float pitchValue = maxN * (AudioSettings.outputSampleRate / 2) / arraySize;


        if (nearlyEqual(pitchValue, 3 * 82.41f, 10))
        {
            int index = (int)convertFrequencyToIndex(82.41f, arraySize);
            //  Debug.Log("Index:" + spectrumData[index]);
            if (spectrumData[index] > 0.00004)
            {
                Debug.Log(spectrumData[index]);
                pitchValue = pitchValue / 3;
            }
        }
        float minDifference = Mathf.Infinity;

        bool repeat = true;
        while (repeat)
        {
            foreach (var entry in freqChordPairs)
            {
                if (nearlyEqual(pitchValue / 2, entry.Key, 5f))
                {
                    pitchValue /= 2;
                    repeat = true;
                    break;
                }
                if (Mathf.Abs(pitchValue - entry.Key) < Mathf.Abs(minDifference))
                {
                    minDifference = pitchValue - entry.Key;
                    targetString = entry.Key;
                }
                repeat = false;
            }
            if (minDifference > 60 && !repeat)
            {
                pitchValue /= 2;
                if (pitchValue < 60)
                    return Mathf.Infinity;
                repeat = true;
            }
        }
        return minDifference;

    }
    float getVolume()
    {
        audioSource.GetOutputData(samples, 0); // Get all of our samples from the mic.

        // Sums squared samples
        float sum = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            sum += Mathf.Pow(samples[i], 2);
        }

        float rmsValue = Mathf.Sqrt(sum / sampleCount);          // RMS is the square root of the average value of the samples.
        float dbValue = 20 * Mathf.Log10(rmsValue / refValue);  // dB
        return dbValue;
    }


    float convertIndexToFreq(float index)
    {
        return index * (AudioSettings.outputSampleRate / 2) / arraySize;
    }
    bool nearlyEqual(float a, float b, float epsilon)
    {
        float absA = Mathf.Abs(a);
        float absB = Mathf.Abs(b);
        float diff = Mathf.Abs(a - b);
        if (diff < epsilon)
        {
            return true;
        }
        return false;
    }

}
