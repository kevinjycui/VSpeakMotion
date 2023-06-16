using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (AudioSource))]  
public class AudioCapture : MonoBehaviour
{
    private bool micConnected = false;

    private int minFreq;    
    private int maxFreq;
    private AudioSource goAudioSource;

    public const int recordingDuration = 10;

    [SerializeField]
    private Button button;

    // Start is called before the first frame update
    void Start()
    {
        if(Microphone.devices.Length <= 0) {    
            Debug.LogWarning("Microphone not connected!");    
            button.interactable = false;  
        }    
        else {    
            micConnected = true;  
            button.interactable = true;  
    
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);    
       
            if(minFreq == 0 && maxFreq == 0) { // Microphone supports any frequency 
                maxFreq = 44100;    
            }    
      
            goAudioSource = this.GetComponent<AudioSource>();    
        }
    }

    public void StartRecording()
    {
        if(micConnected)    
        {    
            if(!Microphone.IsRecording(null))    
            {    
                goAudioSource.clip = Microphone.Start(null, true, 20, maxFreq);
                button.GetComponentInChildren<Text>().text = "Recording...";
                button.interactable = false;

                StartCoroutine(WaitForStopRecording(recordingDuration));
            } 
        }
    }

    IEnumerator WaitForStopRecording(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopRecording();  
    }

    public void StopRecording()
    {
        Microphone.End(null);
        goAudioSource.loop = true;
        goAudioSource.Play();
        button.GetComponentInChildren<Text>().text = "Redo Recording";
        button.interactable = true;
    }
}
