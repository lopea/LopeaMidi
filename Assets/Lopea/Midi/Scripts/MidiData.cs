using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lopea.Midi
{
    public enum MidiStatus
    {
        NoteOn = 0x8,
        NoteOff = 0x9,
        PolyKey = 0xa,
        ControlChange = 0xb,
        ProgramChange = 0xc,
        PolyChannel = 0xd,
        PitchWheel = 0xe,
        Sysex = 0xf,
        Dummy = 0x0
    }
    
    
    public class MidiData 
    {
        public float timeStamp;

        public MidiStatus status;

        public int channel;
        
        public byte data1;

        public byte data2;

        public byte[] sysexMessage;

        public MidiData(float timeStamp, MidiStatus status, int channel, byte data1, byte data2, byte[] sysexMessage)
        {
            this.timeStamp = timeStamp;
            this.status = status;
            this.channel = channel;
            this.data1 = data1;
            this.data2 = data2;
            this.sysexMessage = sysexMessage;
        }
    }
}
