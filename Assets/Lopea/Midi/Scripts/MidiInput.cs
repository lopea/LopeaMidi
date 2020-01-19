//FileName:    MidiInput.cs
//Author:      Javier Sandoval (Lopea)
//GitHub:      https://github.com/lopea
//Description: System to handle all Midi input devices.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Linq;
using Lopea.Midi.Internal;

namespace Lopea.Midi
{
    [HideInInspector]
    public class MidiInput : MonoBehaviour
    {
        #region  MidiInDevice
        class MidiInDevice
        {
            //public variables

            //port number
            public uint port;

            //pointer to the RtMidiDevice
            public IntPtr ptr;

            //name of the device
            public string name;


            //private variables

            Dictionary<int, MidiData> activedata;

            public MidiInDevice(uint port, IntPtr ptr, string name)
            {
                this.port = port;
                this.ptr = ptr;
                this.activedata = new Dictionary<int, MidiData>();
                this.name = name;
            }
            bool isSpecial(MidiStatus status)
            {
                //values after CC messages do not follow a unique value on data 1
                return (int)status > 11;
            }
            public void AddData(MidiData data)
            {
                //handle any special values
                if (isSpecial(data.status))
                {
                    int index = 11 - (int)data.status;
                    if (!activedata.ContainsKey(index))
                        activedata.Add(index, data);
                    else
                        activedata[index] = data;
                }
                else
                {
                    if (activedata.ContainsKey(data.data1))
                    {
                        activedata[data.data1] = data;
                    }
                    else
                    {
                        activedata.Add(data.data1, data);
                    }
                }
            }
            //get data 
            public MidiData getData(int index)
            {

                if (activedata.ContainsKey(index))
                    return activedata[index];
                return null;
            }

            public bool DataActive(int data1, MidiStatus status = MidiStatus.Dummy)
            {
                return activedata.ContainsKey(data1)
                    && activedata[data1].status == status
                    && activedata[data1].data2 != 0;
            }
        }
        #endregion

        #region Private Static Variables

        //store all active devices
        static MidiInDevice[] currdevices;

        //check if updater function is active
        static bool _initialized = false;

        //store monobehaviour object to use unity functions
        static MidiInput _update;

        #endregion

        #region Public Static Constants

        //stores the biggest value a Midi device can send.
        public const int MaxMidiValue = 127;

        #endregion

        #region Private Static Methods



        //frees RtMidiDevice
        static void freeHandle(IntPtr device)
        {
            //free pointer
            MidiInternal.rtmidi_in_free(device);

            //set pointer to null
            device = IntPtr.Zero;
        }

        //add device 
        static void addDevice(uint port)
        {
            //create reference to RtMidi device
            IntPtr reference = MidiInternal.rtmidi_in_create_default();

            //get port count 
            //not using GetPortCount to avoid creating another RtMididevice
            uint count = MidiInternal.rtmidi_get_port_count(reference);

            //check if port number is invalid
            if (port >= count)
            {
                //send error
                Debug.LogError(string.Format("Port Number {0} cannot be used for Midi Input!\nPort range 0-{1}", port,
                                                                                    count - 1));
                //free reference
                freeHandle(reference);

                //quit
                return;
            }

            //get port name 
            string name = MidiInternal.rtmidi_get_port_name(reference, port);

            //ignore types
            MidiInternal.rtmidi_in_ignore_types(reference, false, false, false);

            //add port to RtMidi device
            MidiInternal.rtmidi_open_port(reference, port, "LopeaMidi port: " + name);

            //create midi input handle
            MidiInDevice device = new MidiInDevice(port, reference, name);

            //add to array
            if (currdevices == null)
            {
                currdevices = new MidiInDevice[1];
                currdevices[0] = device;
            }
            else
            {
                var newdevices = new MidiInDevice[currdevices.Length + 1];
                for (int i = 0; i < currdevices.Length; i++)
                    newdevices[i] = currdevices[i];
                newdevices[currdevices.Length] = device;
                currdevices = newdevices;
            }
        }

        //creates a dummy data for any uninitialized midi notes
        //returns MidiData Dummy
        static MidiData CreateMidiDummy(int data1, MidiStatus status = MidiStatus.Dummy)
        {
            return new MidiData(0, status, 0, (byte)data1, 0, null);
        }

        //removes the device from the currrent devices.
        static void removeDevice(uint port)
        {
            if (port >= GetPortCount())
                return;

            //free ptr
            freeHandle(currdevices[port].ptr);
            currdevices[port] = null;
        }

        //remove all references of midi devices and clean all values.
        static void Shutdown()
        {
            for (uint i = 0; i < currdevices.Length; i++)
            {
                removeDevice(i);
            }
            currdevices = null;
            _update = null;
            _initialized = false;
        }



        //get data2 from midi
        static int GetMidi(int data1, int port = -1, MidiStatus status = MidiStatus.Dummy)
        {
            if (!_initialized)
            {
                Initialize();
                if (!_initialized)
                {
                    return 0;
                }
            }
            if (port < 0)
            {
                for (int i = 0; i < currdevices.Length; i++)
                {
                    if (currdevices[i].DataActive(data1, status))
                    {
                        return currdevices[i].getData(data1).data2;
                    }
                }
                return 0;
            }
            else if (currdevices[port].DataActive(data1, status))
            {
                return currdevices[port].getData(data1).data2;
            }
            return 0;
        }

        #endregion

        #region Public Static Functions


        /// <summary>
        /// Initializes MidiInput and connects all Midi Devices.
        /// While it is not necessary to run this function as all Get"" functions intialize properly,
        /// this function exists to be able to initialize at Awake().
        /// </summary>
        public static void Initialize()
        {
            //create the update object
            if (!_initialized)
            {
                //get port count 
                uint port = GetPortCount();
                //dont initialize if there are no midi devices available
                if (port == 0)
                {
                    Debug.LogWarning("MIDIIN: No Midi Device Available! Lopea Midi is not Initializing.");
                    return;
                }
                //create a gameobject with MidiInput Component
                _update = new GameObject("LopeaMidi").AddComponent<MidiInput>();

                //hide the game object from everything
                _update.gameObject.hideFlags = HideFlags.HideInHierarchy;
                //add all devices
                uint count = GetPortCount();
                for (uint i = 0; i < count; i++)
                {
                    addDevice(i);
                }
                _initialized = true;
            }
        }


        /// <summary>
        /// Get the number of midi devices available
        /// </summary>
        /// <returns>Number of MIDI devices connected.</returns>
        public static uint GetPortCount()
        {
            //create RTMidiDevice
            IntPtr handle = MidiInternal.rtmidi_in_create_default();

            //get port count
            uint count = MidiInternal.rtmidi_get_port_count(handle);

            //free handle
            freeHandle(handle);

            return count;
        }


        /// <summary>
        /// Get available data from the MIDI note number.
        /// An expensive process, avoid using during update.
        /// </summary>
        /// <param name="data1">MIDI Controller number</param>
        /// <param name="port">Device Port</param>
        /// <returns>MidiData struct with data from MIDI controller Number</returns>
        public static MidiData GetData(int data1, uint port)
        {
            if (!_initialized)
                Initialize();

            //if device does not exist...
            if (port >= GetPortCount())
            {
                //print message
                Debug.LogError(string.Format("Port Number {0} cannot be used for Midi Input!\nPort range 0-{1}", port,
                                                                                   GetPortCount() - 1));
                //send nothing
                return null;
            }

            //get data from device
            var data = currdevices[port].getData(data1);

            //check if value does not exist to create dummy
            if (data == null)
                data = CreateMidiDummy(data1, MidiStatus.NoteOff);

            return data;
        }


        /// <summary>
        /// Get value from CC note.
        /// Returns value from cc note given, 
        /// optional port number for specific device
        /// </summary>
        /// <param name="data1">Controller number</param>
        /// <param name="port">Optional: device port </param>
        /// <returns>CC value</returns>
        public static int GetCCData(int data1, int port = -1)
        {
            return GetMidi(data1, port, MidiStatus.ControlChange);
        }


        /// <summary>
        /// Get note velocity / velocity aftertouch.
        /// Returns note velocity from midi note value given.
        /// Finds note value from all devices unless specified a given port.
        /// </summary>
        /// <param name="data1">Midi Note Value</param>
        /// <param name="port">Optional: Device port</param>
        /// <returns>Velocity/Velocity Aftertouch</returns>
        public static int GetNoteData(int data1, int port = -1)
        {
            return GetMidi(data1, port, MidiStatus.NoteOn);
        }


        /// <summary>
        /// Check if Midi note is pressed
        /// returns true if note value is pressed and active, false if value is zero.
        /// It will find the note value from all devices unless specified a given port
        /// </summary>
        /// <param name="data1">Number representing CC midi value</param>
        /// <param name="port">Optional: device port to search CC midi value</param>
        /// <returns>true if note value at data1 is not zero, false if zero</returns>
        public static bool GetMidiNote(int data1, int port = -1)
        {
            return GetNoteData(data1, port) != 0;
        }


        /// <summary>
        ///Get Control Change status.
        ///Returns true if CC value is active and not zero, false if value is zero.
        ///It will find the CC value from all devices unless specified a given port
        /// </summary>
        /// <param name="data1">Number representing CC midi value</param>
        /// <param name="port">Optional: device port to search CC midi value</param>
        /// <returns>true if CC value at data1 is not zero, false if zero</returns>
        public static bool GetMidiCC(int data1, int port = -1)
        {
            return GetCCData(data1, port) != 0;
        }




        #endregion

        #region MonoBehaviour Functions

        void OnDisable()
        {
            Shutdown();
        }

        //On every frame, get current status of each midi device and store all its values.
        void Update()
        {
            //Loop based on Keijiro Takahashi's implementation.
            //https://github.com/keijiro/jp.keijiro.rtmidi/
            if (_initialized)
            {

                //allocate memory for messages
                IntPtr messages = Marshal.AllocHGlobal(1024);
                IntPtr size = Marshal.AllocHGlobal(4);

                //loop for every device active 
                for (int i = 0; i < currdevices.Length; i++)
                {

                    //loop indefinitely
                    while (true)
                    {



                        //get message and store timestamp
                        double timestamp = MidiInternal.rtmidi_in_get_message(currdevices[i].ptr, messages, size);

                        //parse size 
                        int currsize = Marshal.ReadInt32(size);

                        //if the message is empty, quit
                        if (currsize == 0)
                        {
                            break;
                        }

                        //
                        //parse message
                        //

                        //store new data
                        MidiData data;

                        //get status byte
                        byte status = Marshal.ReadByte(messages);
                        string s = string.Empty;

                        //store data
                        data = new MidiData((float)timestamp,
                                            (MidiStatus)((status >> 4)),
                                            (status & 0x0F),
                                            Marshal.ReadByte(messages, 1),
                                            (currsize == 2) ? byte.MinValue : Marshal.ReadByte(messages, 2),
                                            null);

                        //some devices for whatever reason have both note on and off to be the same value
                        //so note on/off status is based on velocity
                        if (data.status == MidiStatus.NoteOn || data.status == MidiStatus.NoteOff)
                            data.status = (data.data2 != 0) ? MidiStatus.NoteOn : MidiStatus.NoteOff;



                        //create array of bytes
                        byte[] sys = new byte[currsize];

                        //send each data into bytes
                        for (int j = 0; j < currsize; j++)
                        {
                            sys[j] = Marshal.ReadByte(messages, j);
                        }
                        data.rawData = sys;

                        //add data to the device
                        currdevices[i].AddData(data);
                        for (int j = 0; j < currsize; j++)
                        {
                            Marshal.WriteByte(messages, j, 0);
                        }
                    }

                }

                //deallocate pointers
                Marshal.FreeHGlobal(size);
                Marshal.FreeHGlobal(messages);

            }

        }
        void LateUpdate()
        {

        }
        #endregion
    }
}