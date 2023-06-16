
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class audioCompare : MonoBehaviour
{
    public Image fillImage;
    public TMP_Dropdown dropdown;
    public AudioSource Speaker;
    AudioClip clip1 = null;
    AudioClip clip2 = null;
    bool clip1Recorded = false;
    bool clip2Recorded = false;
    AudioSource mutedSource;
    float[] spectrumData1;
    float[] spectrumData2;
    public Button btn1;
    public Button btn2;
    public Button compareButton;
    public TMP_Text compareTextArea;
    public Sprite play;
    public Sprite stop;
    public Sprite microPhone;
    bool compared = false;
    bool clip1Playing = false;
    bool clip2Playing = false;
    public Slider slider1;
    public Slider slider2;
    string[] fileArray;
    Color initialColor;
    enum recordState
    {
        clip1,
        clip2,
        none,
    };
    recordState recording = recordState.none;
    // Start is called before the first frame update
    void Start()
    {

        spectrumData1 = new float[1024];
        spectrumData2 = new float[1024];
        mutedSource = GetComponent<AudioSource>();

        btn1.onClick.AddListener(playButton1);
        btn2.onClick.AddListener(playButton2);
        compareButton.onClick.AddListener(onCompareButtonClick);


        fileArray = Directory.GetFiles("Assets/Resources/", "*.wav");
        foreach (var file in fileArray)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(file.Remove(file.Length - 4).Remove(0, "Assets/Resources/".Length)));
        }
        dropdown.onValueChanged.AddListener(onValueSelected);
        initialColor = fillImage.color;

    }
    bool loadingClip1 = false;
    // Update is called once per frame
    void Update()
    {
        float percentage = 0f;
        if (loadingClip1)
        {
            percentage = mutedSource.time / clip1.length;
            btn1.GetComponent<Image>().sprite = play;
            fillImage.color = Color.blue;
            slider1.value = percentage;
            if (percentage > 0.98)
            {
                loadingClip1 = false;
                clip1Recorded = true;
                mutedSource.Stop();
                mutedSource.clip = null;
                slider1.value = 0;
                fillImage.color = initialColor;
                recording = recordState.none;
                mutedSource.GetSpectrumData(spectrumData1, 0, FFTWindow.BlackmanHarris);
            }
        }
        if (Speaker.isPlaying)
        {
            percentage = Speaker.time / Speaker.clip.length;

        }
        if (clip1Playing)
        {
            slider1.value = percentage;
            if (!Speaker.isPlaying)
            {
                btn1.GetComponent<Image>().sprite = play;
                clip1Playing = false;
            }
        }
        else if (clip2Playing)
        {
            slider2.value = percentage;
            if (!Speaker.isPlaying)
            {
                btn2.GetComponent<Image>().sprite = play;
                clip2Playing = false;
            }

        }


    }
    void playButton1()
    {
        if (clip1Playing)//stop->play //clip1 is playing right now so we need to stop the play
        {
            slider1.value = 0;
            Speaker.Stop();
            btn1.GetComponent<Image>().sprite = play;
            clip1Playing = false;
            return;
        }
        if (recording == recordState.clip1) //stop recording and display play button
        {
            //stop recording
            btn1.GetComponent<Image>().sprite = play;
            recording = recordState.none;
            Microphone.End(null);
            mutedSource.GetSpectrumData(spectrumData1, 0, FFTWindow.BlackmanHarris);
            mutedSource.Stop();
            clip1Recorded = true;
        }
        else if (recording == recordState.none)
        {
            if (clip1Recorded)  //play is displayed -> stop is displayed
            {
                //play recorded data
                btn1.GetComponent<Image>().sprite = stop;
                Speaker.clip = clip1;
                Speaker.Play();
                clip1Playing = true;
            }
            else //microphone is being displayed -> stop is displayed
            {
                //start recording clip1
                btn1.GetComponent<Image>().sprite = stop;
                recording = recordState.clip1;
                clip1 = Microphone.Start(null, true, 5, 44100);
                mutedSource.clip = clip1;
                while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }
                mutedSource.Play();
            }

        }

    }
    void playButton2()
    {
        if (clip2Playing)//stop->play
        {
            slider2.value = 0;
            Speaker.Stop();
            btn2.GetComponent<Image>().sprite = play;
            clip2Playing = false;
            return;
        }
        if (recording == recordState.clip2)
        {
            //stop recording
            btn2.GetComponent<Image>().sprite = play;
            recording = recordState.none;
            Microphone.End(null);
            mutedSource.GetSpectrumData(spectrumData2, 0, FFTWindow.BlackmanHarris);
            mutedSource.Stop();
            clip2Recorded = true;
        }
        else if (recording == recordState.none)     //button is pressed and not recording
        {
            if (clip2Recorded)  // if recording is done and clip is recorded, the button is now play,so need to change to stop
            {
                btn2.GetComponent<Image>().sprite = stop;
                Speaker.clip = clip2;
                Speaker.Play();
                clip2Playing = true;
            }
            else
            {
                btn2.GetComponent<Image>().sprite = stop;
                recording = recordState.clip2;
                clip2 = Microphone.Start(null, true, 5, 44100);
                mutedSource.clip = clip2;
                while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }
                mutedSource.Play();

            }

        }
    }
    void onValueSelected(int index)
    {
        clip1 = Resources.Load<AudioClip>(dropdown.options[index].text);
        loadingClip1 = true;
        mutedSource.clip = clip1;
        mutedSource.Play();
    }
    void onCompareButtonClick()
    {
        if (compared)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        float comparisionResult = calculateSimilarity(spectrumData1, spectrumData2);
        compareTextArea.text = (comparisionResult.ToString() + "%");
        compared = true;
    }

    float calculateSimilarity(float[] A, float[] B)
    {

        int lowerBound = Mathf.Clamp((int)convertFrequencyToIndex(30, 1024) - 5, 0, 1024);
        int higherBound = Mathf.Clamp((int)convertFrequencyToIndex(400, 1024) + 5, 0, 1024);
        float[] vectorA = new float[higherBound - lowerBound];
        float[] vectorB = new float[higherBound - lowerBound];
        for (int i = lowerBound; i < higherBound; i++)
        {
            vectorA[i - lowerBound] = A[i];
            vectorB[i - lowerBound] = B[i];
        }
        float vecAMax = vectorA.Max();
        float vecBMax = vectorB.Max();
        float sum = 0;
        int j = 0;
        for (int i = lowerBound; i < higherBound; i++)
        {
            vectorA[i] /= vecAMax;
            vectorB[i] /= vecBMax;
            if (vectorA[i] < 0.1 || vectorB[i] < 0.1)
            {
                continue;
            }
            float temp = vectorA[i] - vectorB[i];

            temp = Mathf.Abs(temp);
            temp = temp * 100;
            if (temp > 100)
            {
                temp = 100;
            }
            if (temp < 0)
            {
                temp = 0;
            }
            sum += temp;
            j++;

        }
        if (j == 0)
            return 0;
        return sum / j;
    }
    public void resetOne()
    {
        clip1Recorded = false;
        if (clip1Playing)
        {
            Speaker.Stop();
            clip1Playing = false;
        }
        btn1.GetComponent<Image>().sprite = microPhone;
    }
    public void resetTwo()
    {
        clip2Recorded = false;
        if (clip2Playing)
        {
            Speaker.Stop();
            clip2Playing = false;
        }
        btn2.GetComponent<Image>().sprite = microPhone;
    }
    float convertFrequencyToIndex(float frequency, int sizeOfArray)
    {
        return 2 * sizeOfArray * frequency / (AudioSettings.outputSampleRate);
    }


}
