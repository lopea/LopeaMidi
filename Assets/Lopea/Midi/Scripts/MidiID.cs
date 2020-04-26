//FileName:    MidiID.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: Plain Old Data containing information to identify a MIDI note
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lopea.Midi
{
    [System.Serializable]
    public class MidiID
    {
        
        public int port = -1;
        public int channel = -1;
        public int data1  = -1;

        public MidiID(int channel, int data1, int port)
        {
            this.channel = channel;
            this.data1 = data1;
            this.port = port;
        }
    }

}