using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Lopea.Midi.Internal;

namespace Lopea.Midi
{
    
    public class MidiOutput : MonoBehaviour
    {
        static IntPtr[] OutPorts;
        static bool _init;
        static GameObject _handler;
        static uint count = 0;

        public static uint portCount
        {
            get
            {
                if (!_init)
                    return GetCurrentPortCount();

                return count;
            }
        }

        public const uint MIDIOUTERROR = UInt32.MaxValue;

        static uint GetPortCount()
        {
            if (!_init)
                return 0;

            return count;
        }

        static uint GetCurrentPortCount()
        { 
            IntPtr handle = MidiInternal.rtmidi_out_create_default();

            //get port count
            uint count = MidiInternal.rtmidi_get_port_count(handle);

            //free handle
            FreeHandle(handle);

            return count;
        }

        static void FreeHandle(IntPtr device)
        {
            //free pointer
            MidiInternal.rtmidi_out_free(device);

            //set pointer to null
            device = IntPtr.Zero;
        }

        public static void Initialize()
        {
            if(_init)
                return;

            //Get output count
            uint portCount = GetCurrentPortCount();
            
            //check if there are no midi devices available
            if(portCount == 0)
            {
                //print warning
                Debug.LogError("MIDIOUT: No Midi Output Devices Found!");

                //leave
                return;
            }
            
            //set the global count
            count = portCount;
            
            //setup output device handles
            OutPorts = new IntPtr[portCount];

            for(uint i = 0; i < portCount; i ++)
            {
                OutPorts[i] = MidiInternal.rtmidi_out_create_default();
                MidiInternal.rtmidi_open_port(OutPorts[i], i, "LopeaMidi: Out " + i);    
            }
            _handler = new GameObject("Midi Output");
            _handler.hideFlags = HideFlags.HideInHierarchy;
            _handler.AddComponent<MidiOutput>();
            _init = true;
        }

        public static void Shutdown()
        {
            //check if the device was already shutdown
            if(!_init)
                return;
            
            //free all handles...
            for(int i = 0; i < OutPorts.Length; i ++)
            {
                MidiInternal.rtmidi_out_free(OutPorts[i]);
                OutPorts[i] = IntPtr.Zero;
            }
            
            
            //erase everything
            OutPorts = null;
            _handler = null;
            //set init flag to false
            _init = false;
        }

        public static void SendRawData(uint port, byte[] data)
        {
            if (!_init)
            {
                //setup values
                Initialize();

                //if initialization did not happen, print error message
                if (!_init)
                {
                    //print error
                    Debug.LogError("Data to MIDI not sent!\n An error occured during the Initialization process!");
                    return;
                }
            }
            


            //check if port is valid
            if(port < OutPorts.Length)
            {
                //send message
                MidiInternal.rtmidi_out_send_message(OutPorts[port], data, data.Length);
            }
            else
            {
                if (port != MIDIOUTERROR)
                {
                    Debug.LogError("Device port #" + port + " is invalid for output!");
                }
                else
                {
                    Debug.LogError("Device port MIDIOUTERROR is invalid for output!");
                }
            }
        }

        /// <summary>
        /// Send a single midi note using the MidiData struct.
        /// </summary>
        /// <param name="port">the port number that represents the device to send the note to.</param>
        /// <param name="data">The data of the note that will be sent to the device.</param>
        public static void SendData(uint port, MidiData data)
        {
            //cannot do anything with a dummy 
            if(data.status == MidiStatus.Dummy)
                return;

            SendRawData(port, data.rawData);
        }

        /// <summary>
        /// Send a single midi note to a device.
        /// </summary>
        /// <param name="port">device port number that represents the device to send the note to.</param>
        /// <param name="status">the type of midi note to send.</param>
        /// <param name="data1">The first data byte (this usually represents what note to send)</param>
        /// <param name="data2">The second data byte (this usually represents the velocity of the note to send.)</param>
        /// <param name="channel">The MIDI channel to send the note to.</param>
        public static void SendSimpleData(uint port, MidiStatus status, byte data1, byte data2, byte channel = 0)
        {

            byte[] data = { (byte)(((int)status << 4) | (channel & 0xF)), data1, data2};

            SendRawData(port, data);
        }

        /// <summary>
        /// Get the name of a vaild output device based on its device port number
        /// </summary>
        /// <param name="port">port number to get it's value from</param>
        /// <returns>string containing the name of the port, empty string if the port number is invalid.</returns>
        public static string GetPortName(uint port)
        {
            //if system has not been initialized, 
            if (!_init)
            {
                //initialize system
                Initialize();

                //leave if the initialization failed,
                if(!_init)
                    return String.Empty;
                
            }

            if (port > count)
                return string.Empty;

            return MidiInternal.rtmidi_get_port_name(OutPorts[0], port);
        }

        public static uint FindPortByName(params string[] name)
        {
            // if the output system is not initialized...
            if (!_init)
            {
                //initialize output system
                Initialize();

                //initialization failed.
                if (!_init)
                    return MIDIOUTERROR;
            }

            //loop through every device port for the name of the port and compare them
            for (uint i = 0; i < count; i++)
            {
                //store if strings 
                bool check = false;
                for (uint j = 0; j < name.Length; j++)
                {
                    if (!GetPortName(i).Contains(name[j]))
                    {
                        check = true;
                        break;
                    }
                }

                //return the port number if name satisfies all strings
                if (!check)
                    return i;

            }

            //no valid name found, return error
            string errorMsg = "MIDIOUT: The keywords ";
            for (uint i = 0; i < name.Length; i++)
            {
                errorMsg += name[i] + " ";
            }
            
            Debug.LogWarning(errorMsg + "could not be found.");
            return MIDIOUTERROR;
        }

        void OnDestroy()
        {
            Shutdown();
        }

        void Update()
        {

        }
    }

}
