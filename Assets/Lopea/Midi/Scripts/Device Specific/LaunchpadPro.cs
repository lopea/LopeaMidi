//FileName:    LaunchpadPro.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: A bunch of helper functions for the Novation Launchpad Pro.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;

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

    /// <summary>
    /// This represents the type of grid to display when calling setMultipleNoteLEDGrid().
    /// Full is a 10x10 grid with all notes being affected with the grid format.
    /// Standard is a 8x8 grid with only the square notes being affected.
    /// </summary>
    public enum LaunchpadGridType
    {
        Full = 0,
        Standard = 1
    }

    
    /// <summary>
    /// this represents the orientation of the line created with SetLEDsByLine()
    /// </summary>
    public enum LaunchpadLineType
    {
        Column = 12,
        Row = 13
    }

    public enum LaunchpadNoteType
    {
        Basic = 10,
        Flash = 35,
        Pulse = 40
    }
    public static class LaunchpadPro
    {
        //check if tempo is currently being set.
        static bool _tempoSet = false;

        //Starting values to send at the launchpad for system specific messages
        static readonly byte[] sysexHeader = { 240, 0, 32, 41, 2, 16 };


        static void StartSysex(out byte[] data)
        {
            data = new byte[sysexHeader.Length];
            for(int i = 0; i < data.Length; i++)
                data[i] = sysexHeader[i];
        }
        static void AddSysex(ref byte[] result, byte[] data)
        {
            if(result == null)
                StartSysex(out result);
            if(data == null || data.Length == 0)
                return;
            
            var newArray = new byte[result.Length + data.Length];
            for (int i = 0; i < newArray.Length; i++)
            {
                if(i < result.Length)
                    newArray[i] = result[i];
                else
                    newArray[i] = data[i-result.Length];
            }
            result = newArray;
        }
        static void AddSysex(ref byte[] result, byte data)
        {
            var newArray = new byte[result.Length + 1];
            for (int i = 0; i < result.Length; i++)
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

        static string StateToName(LaunchpadProState state)
        {
            if (state != LaunchpadProState.AbletonLive)
                return "(Launchpad Pro)";

            return "LaunchpadPro";
        }

        static string StateToNameInput(LaunchpadProState state)
        {
            if (state == LaunchpadProState.AbletonLive)
                return StateToName(state);

            return "MIDIIN" + (int)state + " " + StateToName(state);
        }

        static string StateToNameOutput(LaunchpadProState state)
        {
            if (state == LaunchpadProState.AbletonLive)
                return StateToName(state);

            return "MIDIOUT" + (int)state + " " + StateToName(state);
        }

        /// <summary>
        /// Get Input port based on the type of Launchpad 
        /// </summary>
        /// <param name="state"> the type of port to look for in the launchpad pro. </param>
        /// <returns>device port based on the type of state, MIDIINERROR if the launchpad was not found. </returns>
        public static uint GetInputPort(LaunchpadProState state)
        {
            return MidiInput.FindPortByName(StateToNameInput(state));
        }

        /// <summary>
        /// Get Output port based on the type of Launchpad pro state
        /// </summary>
        /// <param name="state"> the type of port to look for in the launchpad pro. </param>
        /// <returns>device port based on the type of state, MIDIOUTERROR if the launchpad was not found. </returns>
        public static uint GetOutputPort(LaunchpadProState state)
        {
            return MidiOutput.FindPortByName(StateToNameOutput(state));
        }

        /// <summary>
        /// sets an LED to a predetermined color based on a number from 0-127
        /// </summary>
        /// <param name="port">port where the launchpad is located</param>
        /// <param name="note">note value coresponding to the LED that is lit</param>
        /// <param name="color">value from 0-127 that will light up the LED</param>
        /// <param name="type">sets the type of LED state for the launchpad, Basic = on, flashing = flashing with Midi clock, pulsing = pulsing with midiClock</param>
        public static void SetNoteLED(uint port, byte note, byte color, LaunchpadNoteType type = LaunchpadNoteType.Basic)
        {
            byte[] result;
            byte[] data = {(byte)type, note, color};
            StartSysex(out result);
            AddSysex(ref result,data);
            EndSysex(ref result);
            
            MidiOutput.SendRawData(port, data);
        }
        

        /// <summary>
        /// sets an note LED to a specific color given.
        /// </summary>
        /// <param name="port">Device port</param>
        /// <param name="note">Note number based on the LED to be lit</param>
        /// <param name="color">the color to set the LED</param>
        public static void setNoteLED(int port, byte note, Color color)
        {
            //check if all parameters are valid 
            if(port < 0 || port >= MidiInput.portCount)
            {
                return;
            }
            //set color data
            byte[] col = { (byte)(color.r * 63), (byte)(color.g * 63), (byte)(color.b * 63) };
            
            //set data to send to launchpad
            byte[] result;
            StartSysex(out result);
            //send led color flag
            AddSysex(ref result, 11);
            AddSysex(ref result, note);
            AddSysex(ref result, col);
            EndSysex(ref result);
            //send data to launchpad pro.
            MidiOutput.SendRawData((uint)port, result);
        }

        /// <summary>
        /// sets specific LEDs in a single message. 
        /// Note: due to the nature of sysex messages, the max amount of LEDs to light up is 78.
        /// </summary>
        /// <param name="port">port number the launchpad pro is located.</param>
        /// <param name="notes">all the note numbers to send messages to.</param>
        /// <param name="color">Color to set all the values given.</param>
        public static void SetNoteLEDs(uint port, byte[] notes, Color color)
        {
            if(notes.Length == 0 || notes == null)
                return;
            if(notes.Length > 78)
                Debug.LogWarning("SetNoteLEDs() can only send 78 different note values at once!");
            
            byte[] result;
            byte[] col = { (byte)(color.r * 63), (byte)(color.g * 63), (byte)(color.b * 63) };

            //set up sysex message
            StartSysex(out result);
            //add message type to led color
            AddSysex(ref result, 11);
            //add every note to desired color
            for(int i = 0; i < 78; i++)
            {
                AddSysex(ref result,notes[i]);
                AddSysex(ref result,col);
            }
            //end sysex message 
            EndSysex(ref result);
            //send message
            MidiOutput.SendRawData(port, result);
        }

        /// <summary>
        /// Set multiple leds with a single color using the launchpad's standard color format.
        /// The amount of LEDs to set is limited to 78 leds per message using this method.
        /// </summary>
        /// <param name="port">number representing the location of the Launchpad.</param>
        /// <param name="notes">the note numbers representing the LED to set</param>
        /// <param name="color">color to set all the LEDs to (0-127)</param>
        /// <param name="type">type of LED status to use</param>
        public static void SetNoteLEDs(uint port, byte[] notes, byte color, LaunchpadNoteType type = LaunchpadNoteType.Basic)
        {
            if(notes.Length == 0)
                return;
            if(notes.Length > 78)
                Debug.LogWarning("SetNoteLEDs() can only send 78 different note values at once!");
            
            byte[] result;
           
            //set up sysex message
            StartSysex(out result);
            //set the type of led to light up
            AddSysex(ref result, (byte)type);
            //add every note to desired color
            for(int i = 0; i < notes.Length; i++)
            {
                //break when limit is reached.
                if(i >= 78)
                    return;
                AddSysex(ref result,notes[i]);
                AddSysex(ref result,color);
            }
            //end sysex message 
            EndSysex(ref result);
            //send message
            MidiOutput.SendRawData(port, result);
        }

        /// <summary>
        /// Set an array of colors into the launchpad.<br></br>
        /// This is an effective way to color all the LEDs in the launchpad with specific colors.<br></br>
        /// The grid format is left to right as the first color in the index is the bottom left corner<br></br>
        /// and the color in the maximum index is at the top right corner of the launchpad.
        /// </summary>
        /// <param name="port">Port that represents the launchpad pro</param>
        /// <param name="type"> The type grid format that sets the launchpad. Full</param>
        /// <param name="colors"></param>
        public static void SetNoteLEDsGrid(uint port, LaunchpadGridType type, Color[] colors)
        {
            //set the amount of led to light
            int size = (type == LaunchpadGridType.Full) ? 100 : 64;
            
            byte[] result;
            StartSysex(out result);

            //specify that message is a grid led message
            AddSysex(ref result, 15);
            
            //specify the grid type
            AddSysex(ref result, (byte)type);

            //add all the colors
            for(int i = 0; i < size; i++)
            {
                //if the array is too small, quit
                if(i >= colors.Length)
                    break;
                byte[] col = { (byte)(colors[i].r * 63), (byte)(colors[i].g * 63), (byte)(colors[i].b * 63) };
                AddSysex(ref result, col);
            }

            //end sysex message
            EndSysex(ref result);

            //send message
            MidiOutput.SendRawData(port, result);
           
        }
        
        /// <summary>
        /// Yes, this device can display text.<br></br>
        /// The text will scroll across the launchpad with color given (0-127).<br></br>
        /// The speed of the text can vary by using escape characters(\x01 = slowest speed, \x07 = fastest)
        /// inside the text itself.<br></br>
        /// NOTE: The text will not display if you call this message multiple times!
        /// </summary>
        /// <param name="port">Port that the launchpad pro is located.</param>
        /// <param name="text">Text to display in the launchpad</param>
        /// <param name="color">Color the text is going to be</param>
        /// <param name="loop">set the text to loop across the screen</param>
        public static void SendText(uint port, string text, byte color, bool loop = false)
        { 
        //set string to be ascii text
            byte[] AsciiText = Encoding.ASCII.GetBytes(text);
            
            //store sysEx message
            byte[] result;

            //initialize message
            StartSysex(out result);

            //specify message is text
            AddSysex(ref result, 20);
            
            //specify color
            AddSysex(ref result, color);

            //add loop status 
            AddSysex(ref result, (byte)(loop ? 1 : 0));
            
            //add text 
            AddSysex(ref result, AsciiText);
            
            //end message
            EndSysex(ref result);

            //send message
            MidiOutput.SendRawData(port,result);
        }

        /// <summary>
        /// Turn all the leds to black.<br></br>
        /// This is useful when quitting the program as the LED will stay lit until specified.
        /// </summary>
        /// <param name="port">Number representing Launchpad Pro.</param>
        public static void ClearAllLEDs(uint port)
        {
            //start sysex
            byte[] result;
            
            StartSysex(out result);
            
            byte[] clear = {14, 0};
            AddSysex(ref result, clear);
            EndSysex(ref result);
            MidiOutput.SendRawData(port, result);
        }

        /// <summary>
        /// Set all leds in a launchpad to a certain color.
        /// </summary>
        /// <param name="port">The port number that represents the launchpad pro</param>
        /// <param name="color">Color to change to in</param>
        public static void SetAllLEDs(uint port, Color color)
        {
            byte[] result;
            StartSysex(out result);

            //specify that message is a grid led message
            AddSysex(ref result, 15);
            
            //specify the grid type
            AddSysex(ref result, 0);

            byte[] col = { (byte)(color.r * 63), (byte)(color.g * 63), (byte)(color.b * 63) };
            for(int i = 0; i < 100; i ++)
            {
                AddSysex(ref result, col);
            }
            EndSysex(ref result);

            MidiOutput.SendRawData(port, result);
        }

        /// <summary>
        /// Get a note id number based on the relative position on the launchpad.
        /// (0,0) represents the postion between "Record arm" and the record button.
        /// (10,10) represents the position between the "user button" and the highest arrow button.
        /// </summary>
        /// <param name="position">Relative position of the note in launchpad space.</param>
        /// <returns>the new note value that represents the relative position.</returns>
        public static int GetNoteNumberFromPosition(Vector2 position)
        {
            return ((int)Mathf.Clamp(position.y,0, 10) * 10 + (int)Mathf.Clamp(position.x,0,10) % 10);
        }

        /// <summary>
        /// Get a relative position of a note based on its ID number.
        /// Position is relative to the position of the launchpad pro.
        /// (0,0) represents the postion between "Record arm" and the record button.
        /// (10,10) represents the position between the "user button" and the highest arrow button.
        /// </summary>
        /// <param name="noteValue">MIDI data representing a note</param>
        /// <returns>Node position in space relative to the launchpad pro.</returns>
        public static Vector2 GetPositionFromNoteNumber(byte noteValue)
        {
            return new Vector2((int)noteValue / 10, (int)noteValue % 10);
        }
        


        public static void SetLEDsByLine(uint port, LaunchpadLineType type, uint lineIndex, byte color)
        {
            if(lineIndex > 9)
                lineIndex = 9;

            byte[] result;
            StartSysex(out result);
            byte[] data = {(byte)type, (byte)lineIndex, color};
            AddSysex(ref result, data);
            EndSysex(ref result);
            
            MidiOutput.SendRawData(port, result);
            
        }

        
    }
}

