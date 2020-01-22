﻿//FileName:    InputDevice.cs 
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

namespace Lopea.Midi
{
    public class InputDevice : MonoBehaviour
    {
        
        void Start()
        {
            for(uint i = 0; i < MidiInput.portCount; i++)
            {
                print(MidiInput.GetPortName(i));
            }
        }
        void Update()
        {
            if(MidiInput.GetNotePressed(12))
                print("Cool");
        }
    }
}