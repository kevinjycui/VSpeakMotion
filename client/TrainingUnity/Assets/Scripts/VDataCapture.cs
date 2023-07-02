using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    public int recordingDuration = 10;

    [SerializeField]
    private Button recordButton;

    [SerializeField]
    private Button saveButton;

    [SerializeField]
    private VDataPlayback playback;

    [SerializeField]
    public uOSC.uOscServer server;

    [SerializeField]
    public int livePort = 39540;

    [SerializeField]
    public int playbackPort = 39541;

    private float startTime = -1.0f;
    // Time | Keys
    public List<List<float>> BlendShapeData = new List<List<float>>();
    // Time | Positional keys x y z | Rotational keys w x y z
    public List<List<float>> BoneData = new List<List<float>>();

    public static readonly string[] BlendShapeKeys = {"A", "Angry", "Blink", "Blink_L", "Blink_R", "E", "Fun", "I", "Joy", "LookDown", "LookLeft", "LookRight", "LookUp", "Neutral", "O", "Sorrow", "Surprised", "U"};
    public static readonly string[] BoneKeys = {"Chest", "Head", "Hips", "LeftEye", "LeftFoot", "LeftHand", "LeftIndexDistal", "LeftIndexIntermediate", "LeftIndexProximal", "LeftLittleDistal", "LeftLittleIntermediate", "LeftLittleProximal", "LeftLowerArm", "LeftLowerLeg", "LeftMiddleDistal", "LeftMiddleIntermediate", "LeftMiddleProximal", "LeftRingDistal", "LeftRingIntermediate", "LeftRingProximal", "LeftShoulder", "LeftThumbDistal", "LeftThumbIntermediate", "LeftThumbProximal", "LeftToes", "LeftUpperArm", "LeftUpperLeg", "Neck", "RightEye", "RightFoot", "RightHand", "RightIndexDistal", "RightIndexIntermediate", "RightIndexProximal", "RightLittleDistal", "RightLittleIntermediate", "RightLittleProximal", "RightLowerArm", "RightLowerLeg", "RightMiddleDistal", "RightMiddleIntermediate", "RightMiddleProximal", "RightRingDistal", "RightRingIntermediate", "RightRingProximal", "RightShoulder", "RightThumbDistal", "RightThumbIntermediate", "RightThumbProximal", "RightToes", "RightUpperArm", "RightUpperLeg", "Spine", "UpperChest"};

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

                RestartServer(livePort);
                BlendShapeData.Clear();
                BoneData.Clear();
                startTime = Time.realtimeSinceStartup;

                StartCoroutine(WaitForStopRecording(recordingDuration));
            } 
        }
    }

    IEnumerator WaitForStopRecording(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        startTime = -1;
        StopRecording();
    }

    void RestartServer(int port)
    {
        server.StopServer();
        server.port = port;
        server.StartServer();
    }

    public void StopRecording()
    {
        Microphone.End(null);
        goAudioSource.loop = true;
        goAudioSource.Play();
        RestartServer(playbackPort);
        playback.Init();
        recordButton.GetComponentInChildren<Text>().text = "Redo Recording";
        recordButton.interactable = true;
        saveButton.interactable = true;
    }

    public void SaveRecording()
    {
        string timestamp = System.DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace("\\", "-");

        SavWav.Save("Audio/" + timestamp, goAudioSource.clip);
        SavCsv.Save("Blendshapes/" + timestamp, BlendShapeData);
        SavCsv.Save("Bones/" + timestamp, BoneData);

        saveButton.interactable = false;
    }

    public int GetBlendShapeRecordLength()
    {
        return BlendShapeData.Count;
    }

    public List<float> GetBlendShapeRecordAt(int index, float timePassed)
    {
        if (index < 0 || index >= GetBlendShapeRecordLength()) {
            return null;
        }
        List<float> record = BlendShapeData.ElementAt(index);
        if (record.ElementAt(0) > timePassed) {
            return null;
        }
        return record;
    }

    public int GetBoneRecordLength()
    {
        return BoneData.Count;
    }

    public List<float> GetBoneRecordAt(int index, float timePassed)
    {
        if (index < 0 || index >= GetBoneRecordLength()) {
            return null;
        }
        List<float> record = BoneData.ElementAt(index);
        if (record.ElementAt(0) > timePassed) {
            return null;
        }
        return record;
    }

    public void RecordBlendShapes(Dictionary<BlendShapeKey, float> BlendShapeToValueDictionary)
    {
        if (startTime == -1.0f) {
            return;
        }
        // foreach(var item in BlendShapeToValueDictionary) {
        //     Debug.Log(item.Key + ": " + item.Value);
        // }

        SortedDictionary<string, float> _BlendShapeToValueDictionary;
        _BlendShapeToValueDictionary = new SortedDictionary<string, float>(BlendShapeToValueDictionary.ToDictionary(item => item.Key.ToString(), item => item.Value));

        // string keystrings = "";
        // foreach(var key in _BlendShapeToValueDictionary.Keys) {
        //     keystrings += "\"" + key + "\", ";
        // }
        // Debug.Log(keystrings);

        List<float> BlendShapeDataWindow = new List<float>();
        BlendShapeDataWindow.Add(Time.realtimeSinceStartup - startTime);
        foreach(var key in _BlendShapeToValueDictionary.Values) {
            BlendShapeDataWindow.Add(key);
        }
        BlendShapeData.Add(BlendShapeDataWindow);
    }

    public void RecordBones(Dictionary<HumanBodyBones, Vector3> HumanBodyBonesPositionTable, Dictionary<HumanBodyBones, Quaternion> HumanBodyBonesRotationTable) 
    {
        if (startTime == -1.0f) {
            return;
        }

        // foreach(var item in HumanBodyBonesPositionTable) {
        //     Debug.Log(item.Key + "_Pos: " + item.Value);
        // }
        // foreach(var item in HumanBodyBonesRotationTable) {
        //     Debug.Log(item.Key + "_Rot: " + item.Value);
        // }

        SortedDictionary<string, Vector3> _HumanBodyBonesPositionTable;
        _HumanBodyBonesPositionTable = new SortedDictionary<string, Vector3>(
            HumanBodyBonesPositionTable.ToDictionary(item => item.Key.ToString(), item => item.Value));

        // string keystrings = "";
        // foreach(var key in _HumanBodyBonesPositionTable.Keys) {
        //     keystrings += "\"" + key + "\", ";
        // }
        // Debug.Log("Positional: " + keystrings);

        SortedDictionary<string, Quaternion> _HumanBodyBonesRotationTable;
        _HumanBodyBonesRotationTable = new SortedDictionary<string, Quaternion>(
            HumanBodyBonesRotationTable.ToDictionary(item => item.Key.ToString(), item => item.Value));

        // keystrings = "";
        // foreach(var key in _HumanBodyBonesRotationTable.Keys) {
        //     keystrings += "\"" + key + "\", ";
        // }
        // Debug.Log("Rotational: " + keystrings);

        List<float> BoneDataWindow = new List<float>();
        BoneDataWindow.Add(Time.realtimeSinceStartup - startTime);

        foreach(string key in BoneKeys) {
            Vector3 position = _HumanBodyBonesPositionTable[key];
            Quaternion rotation = _HumanBodyBonesRotationTable[key];

            BoneDataWindow.Add(position.x);
            BoneDataWindow.Add(position.y);
            BoneDataWindow.Add(position.z);
            BoneDataWindow.Add(rotation.x);
            BoneDataWindow.Add(rotation.y);
            BoneDataWindow.Add(rotation.z);
            BoneDataWindow.Add(rotation.w);
        }

        // foreach(Vector3 position in _HumanBodyBonesPositionTable.Values) {
        //     BoneDataWindow.Add(position.x);
        //     BoneDataWindow.Add(position.y);
        //     BoneDataWindow.Add(position.z);
        // }
        // foreach(Quaternion rotation in _HumanBodyBonesRotationTable.Values) {
        //     BoneDataWindow.Add(rotation.w);
        //     BoneDataWindow.Add(rotation.x);
        //     BoneDataWindow.Add(rotation.y);
        //     BoneDataWindow.Add(rotation.z);
        // }
        BoneData.Add(BoneDataWindow);
    }

}
