//FileName:    InputDevice.cs 
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: Input device handler for unity3D

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;
using System.Threading;
using Lopea.Midi;
using Lopea.Midi.Devices;


public class InputDevice : MonoBehaviour
{
    int port = 0;
    Color color = Color.cyan;
    [SerializeField]Gradient gradient;
    void Start()
    {
        for (uint i = 0; i < MidiInput.portCount; i++)
        {
            print(MidiInput.GetPortName(i));
        }
        port = LaunchpadPro.getPort(LaunchpadProState.Standalone);
        

    }
    void Update()
    {
        LaunchpadPro.SetAllLEDs((uint)port, color);
        color = gradient.Evaluate(0.5f - Mathf.Cos(Time.time * 30) * 0.5f);
    }
}
