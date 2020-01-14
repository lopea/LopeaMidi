//FileName:    InputDevice.cs 
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: Input device handler for unity3D

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lopea.Midi.Internal;
using System.Runtime.InteropServices;
using UnityEditor;
using System.Threading;

namespace Lopea.Midi
{
    public class InputDevice : MonoBehaviour
    {
        void Start()
        {
            MidiInput.Initialize();
        }
        void Update()
        {
            if(MidiInput.GetMidiNote(11))
            {
                print("haha, yes");
            }
        }
    }
}