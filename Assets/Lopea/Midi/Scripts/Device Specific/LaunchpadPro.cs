//FileName:    LaunchpadPro.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: A bunch of helper functions for the Novation Launchpad Pro.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lopea.Midi.Devices
{
    //launchpad pro has 3 states, Ableton Live mode(1st port), Standalone mode(2nd port), and Hardware mode(3rd port)
    //
    //Ableton mode gives you a standard set of notes and cc buttons (like Programmer mode)
    //Ableton mode has more features but have to be specified through sysex messages in order to access them.
    //
    //Standalone mode is everything else (Note, drum, fader and programmer)
    //Standalone mode is the easiest to interact with all the features the launchpad provides.
    //
    //Hardware mode is to access the devices connected through the MIDI in and MIDI out ports of the launchpad
    //it is possible to send and receive messages through this port but it will go straight to the devices connected to the port and not the launchpad.
    //for more information, see https://customer.novationmusic.com/sites/customer/files/novation/downloads/10598/launchpad-pro-programmers-reference-guide_0.pdf
    public enum LaunchpadProState
    {
        AbletonLive = 1,
        Standalone = 2,
        Hardware = 3
    }
    public static class LaunchpadPro
    {
        

        //Starting values to send at the launchpad for system specific messages
        public static byte[] sysexHeader = { 240, 0, 32, 41, 2, 16 };

        //TODO: Finish function
        public static uint getPort(LaunchpadProState state)
        {
            return 0;
        }


    }
}

