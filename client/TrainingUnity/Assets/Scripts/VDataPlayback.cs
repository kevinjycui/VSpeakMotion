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

    void Start()
    {
        client = GetComponent<uOSC.uOscClient>();
    }

    void Update()
    {
        
    }
}