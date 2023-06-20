using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using UniGLTF;

[RequireComponent (typeof (AudioSource))]  
public class VDataCapture : MonoBehaviour
{
    private bool micConnected = false;

    private int minFreq;    
    private int maxFreq;
    private AudioSource goAudioSource;

    public const int recordingDuration = 10;

    [SerializeField]
    private Button recordButton;

    [SerializeField]
    private Button saveButton;

    private Dictionary<BlendShapeKey, float> BlendShapeToValueDictionary;

    // Start is called before the first frame update
    void Start()
    {
        if(Microphone.devices.Length <= 0) {    
            Debug.LogWarning("Microphone not connected!");    
            recordButton.interactable = false;  
        }    
        else {    
            micConnected = true;  
            recordButton.interactable = true;  
    
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);    
       
            if(minFreq == 0 && maxFreq == 0) { // Microphone supports any frequency 
                maxFreq = 44100;    
            }    
      
            goAudioSource = this.GetComponent<AudioSource>();    
        }
        saveButton.interactable = false;
    }

    public void StartRecording()
    {
        if(micConnected)    
        {    
            if(!Microphone.IsRecording(null))    
            {    
                goAudioSource.clip = Microphone.Start(null, true, recordingDuration, maxFreq);
                recordButton.GetComponentInChildren<Text>().text = "Recording...";
                recordButton.interactable = false;

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
        recordButton.GetComponentInChildren<Text>().text = "Redo Recording";
        recordButton.interactable = true;
        saveButton.interactable = true;
    }

    public void SaveRecording()
    {
        SavWav.Save("audio-" + System.DateTime.Now.ToString().Replace(":", "-"), goAudioSource.clip);
        saveButton.interactable = false;
    }

    public void RecordBlendShapeKeys(Dictionary<BlendShapeKey, float> BlendShapeToValueDictionary)
    {
        foreach(var item in BlendShapeToValueDictionary) {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }

    public void RecordBones(Dictionary<HumanBodyBones, Vector3> HumanBodyBonesPositionTable, Dictionary<HumanBodyBones, Quaternion> HumanBodyBonesRotationTable) {
        foreach(var item in HumanBodyBonesPositionTable) {
            Debug.Log(item.Key + "_Pos: " + item.Value);
        }
        foreach(var item in HumanBodyBonesRotationTable) {
            Debug.Log(item.Key + "_Rot: " + item.Value);
        }
    }

}
