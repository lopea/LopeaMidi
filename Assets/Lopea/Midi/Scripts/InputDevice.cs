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

namespace Lopea.Midi
{
    public class InputDevice : MonoBehaviour
    {
        byte[] message = {144, 11, 69};
        void Start()
        {
            MidiOutput.SendRaw(2, message);
        }
        void Update()
        {
            
        
            
        }
    }
}