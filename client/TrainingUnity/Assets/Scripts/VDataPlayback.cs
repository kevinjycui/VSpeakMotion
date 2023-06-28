using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using VRM;

[RequireComponent(typeof(uOSC.uOscClient))]
public class VDataPlayback : MonoBehaviour 
{
    uOSC.uOscClient client = null;

    private float loadTime = -1.0f;
    private int cachedIndex = 0;
    
    [SerializeField]
    private VDataCapture captureObject;

    void Start()
    {
        client = GetComponent<uOSC.uOscClient>();
    }

    public void Init()
    {
        loadTime = Time.realtimeSinceStartup;
    }

    void Update()
    {

    }
}