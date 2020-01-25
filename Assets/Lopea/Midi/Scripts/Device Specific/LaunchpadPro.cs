//FileName:    LaunchpadPro.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: A bunch of helper functions for the Novation Launchpad Pro.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lopea.Midi;

namespace Lopea.Midi.Devices
{
    //launchpad pro has 3 states, Ableton Live mode(1st port), Standalone mode(2nd port), and Hardware mode(3rd port)
    //
    //Ableton mode gives you a standard set of notes and cc buttons (like Programmer mode)
    //Ableton mode has more features but they have to be specified through sysex messages in order to access them.
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
        static readonly byte[] sysexHeader = { 240, 0, 32, 41, 2, 16 };


        static void startSysex(out byte[] data)
        {
            data = new byte[sysexHeader.Length];
            for(int i = 0; i < data.Length; i++)
                data[i] = sysexHeader[i];
        }
        static void AddSysex(ref byte[] result, byte[] data)
        {
            if(result == null)
                startSysex(out result);
            if(data == null || data.Length == 0)
                return;
            
            var newArray = new byte[result.Length + data.Length];
            for (int i = 0; i < newArray.Length; i++)
            {
                if(i < result.Length)
                    newArray[i] = result[i];
                else
                    newArray[i] = data[i];
            }
            result = newArray;
        }
        static void AddSysex(ref byte[] result, byte data)
        {
            var newArray = new byte[result.Length + 1];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = result[i];
            }
            newArray[result.Length] = data;
            result = newArray;
        }

        static void EndSysex(ref byte[] result)
        {
            AddSysex(ref result, 247);
        }

        //TODO: Finish function
        public static int getPort(LaunchpadProState state)
        {
            return MidiInput.FindPort("Launchpad Pro:Launchpad Pro MIDI " + (int)state);  
        }

        public static bool IsPortValid(int port)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="note"></param>
        /// <param name="color"></param>
        public static void setSingleNoteLED(int port, byte note, byte color)
        {
            if(port < 0 || port >= MidiInput.portCount || !IsPortValid(port))
            {
                return;
            }
            byte[] data = {144, note, color};
            
            
            MidiOutput.SendRawData((uint)port, data);
        }
        

        /// <summary>
        /// sets an note LED to a specific color given.
        /// </summary>
        /// <param name="port">Device port</param>
        /// <param name="note">Note number based on the LED to be lit</param>
        /// <param name="color">the color to set the LED</param>
        public static void setSingleNoteLED(int port, uint note, Color color)
        {
            //check if all parameters are valid 
            if(port < 0 || port >= MidiInput.portCount || !IsPortValid(port))
            {
                return;
            }
            //set color data
            byte[] col = { (byte)(color.r * 63), (byte)(color.g * 63), (byte)(color.b * 63) };
            
            //set data to send to launchpad
            byte[] result;
            startSysex(out result);
            //send led color flag
            AddSysex(ref result, 11);
            AddSysex(ref result, col);
            EndSysex(ref result);
            //send data to launchpad pro.
            MidiOutput.SendRawData((uint)port, result);
        }


    }
}

