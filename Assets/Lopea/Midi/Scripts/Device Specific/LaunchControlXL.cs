//FileName:    LaunchControlXL.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: A bunch of helper functions for the Novation LaunchControl XL.

using System.Collections;
using System.Collections.Generic;
using Lopea.Midi;
using UnityEngine;


public static class LaunchControlXL
{
    //Send A Control Knobs
    public const byte SendA1 = 0x0D;
    public const byte SendA2 = 0x0E;
    public const byte SendA3 = 0x0F;
    public const byte SendA4 = 0x10;
    public const byte SendA5 = 0x11;
    public const byte SendA6 = 0x12;
    public const byte SendA7 = 0x13;
    public const byte SendA8 = 0x14;

    //Send B Control Knobs
    public const byte SendB1 = 0x1D;
    public const byte SendB2 = 0x1E;
    public const byte SendB3 = 0x1F;
    public const byte SendB4 = 0x20;
    public const byte SendB5 = 0x21;
    public const byte SendB6 = 0x22;
    public const byte SendB7 = 0x23;
    public const byte SendB8 = 0x24;

    //Pan/Device Control Knobs
    public const byte Pan1 = 0x31;
    public const byte Pan2 = 0x32;
    public const byte Pan3 = 0x33;
    public const byte Pan4 = 0x34;
    public const byte Pan5 = 0x35;
    public const byte Pan6 = 0x36;
    public const byte Pan7 = 0x37;
    public const byte Pan8 = 0x38;

    //Sliders
    public const byte Slider1 = 0x4D;
    public const byte Slider2 = 0x4E;
    public const byte Slider3 = 0x4F;
    public const byte Slider4 = 0x50;
    public const byte Slider5 = 0x51;
    public const byte Slider6 = 0x52;
    public const byte Slider7 = 0x53;
    public const byte Slider8 = 0x54;

    //Track Focus buttons
    public const byte TrackFocus1 = 0x29;
    public const byte TrackFocus2 = 0x2A;
    public const byte TrackFocus3 = 0x2B;
    public const byte TrackFocus4 = 0x2C;
    public const byte TrackFocus5 = 0x39;
    public const byte TrackFocus6 = 0x3A;
    public const byte TrackFocus7 = 0x3B;
    public const byte TrackFocus8 = 0x3C;
    
    //Track Control buttons
    public const byte TrackControl1 = 0x49;
    public const byte TrackControl2 = 0x4A;
    public const byte TrackControl3 = 0x4B;
    public const byte TrackControl4 = 0x4C;
    public const byte TrackControl5 = 0x59;
    public const byte TrackControl6 = 0x5A;
    public const byte TrackControl7 = 0x5B;
    public const byte TrackControl8 = 0x5C;

    public static uint GetInputPort()
    {
        return MidiInput.FindPortByName("Launch Control XL");
    }
}
