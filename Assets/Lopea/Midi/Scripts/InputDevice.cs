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
    uint port, iport;
    Color color = Color.cyan;
    [SerializeField]Gradient gradient;
    void Start()
    {

        for (uint i = 0; i < MidiInput.portCount; i++)
        {
            print(MidiInput.GetPortName(i));
        }
        MidiOutput.OnShutdown += OnMidiShutdown;
    }
    void Update()
    {
        port = LaunchpadPro.GetOutputPort(LaunchpadProState.Standalone);
        iport = LaunchpadPro.GetInputPort(LaunchpadProState.Standalone);
        if(MidiInput.GetNote(13, (int)iport))
        {
            color = gradient.Evaluate(0.5f - Mathf.Cos(Time.time * 10));
            LaunchpadPro.SetAllLEDs(port, color);
        }
    }
    void OnMidiShutdown()
    {
        LaunchpadPro.ClearAllLEDs(port);
    }
}
