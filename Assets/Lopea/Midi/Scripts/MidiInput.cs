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
using System.Runtime.InteropServices.WindowsRuntime;
using Lopea.Midi.Internal;
using UnityEngine.PlayerLoop;

namespace Lopea.Midi
{
    public delegate void MidiInFunc();

    public delegate void MidiInNoteFunc(MidiData context);
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

            public MidiData getLastSpecial(MidiStatus status)
            {
                //check if the status given is special
                if (isSpecial(status) && activedata.ContainsKey(11 - (int)status))
                {
                    return activedata[11 - (int)status];
                }
                return CreateMidiDummy(0);
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

        //store port count
        static uint _port;


        #endregion

        #region Public Static Constants

        //stores the biggest value a Midi device can send.
        public const uint MaxMidiValue = 127;

        //stores the Error handler for invalid/unknown input device ports
        public const uint MIDIINERROR = UInt32.MaxValue;

        #endregion

        #region Public Static Variables



        /// <summary>
        /// Gets invoked when the input system gets shutdown
        /// </summary>
        public static event MidiInFunc OnShutdown;

        /// <summary>
        /// Gets invoked whenever a midi device has been received from any device
        /// </summary>
       

        /// <summary>
        /// get the amount of devices plugged in at a given time
        /// </summary>
        public static uint portCount
        {
            get
            {
                if (!_initialized)
                    return GetPortCount();
                return _port;
            }
            private set
            {
                _port = value;
            }
        }

        public bool IsInitialized
        {
            get { return _initialized; }
        }
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
        static MidiInDevice CreateDevice(uint port)
        {
            //create reference to RtMidi device
            IntPtr reference = MidiInternal.rtmidi_in_create_default();

            //get port name 
            string name = MidiInternal.rtmidi_get_port_name(reference, port);

            //ignore types
            MidiInternal.rtmidi_in_ignore_types(reference, false, false, false);

            //add port to RtMidi device
            MidiInternal.rtmidi_open_port(reference, port, "LopeaMidi port: " + name);

            //create midi input handle
            return new MidiInDevice(port, reference, name);


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
            //free ptr
            freeHandle(currdevices[port].ptr);
            currdevices[port] = null;
        }

        //remove all references of midi devices and clean all values.
        public static void Shutdown()
        {
            //call event
            //?.Invoke();
            if (!_initialized)
                return;


            for (uint i = 0; i < currdevices.Length; i++)
            {
                removeDevice(i);
            }
            currdevices = null;
            Destroy(_update);
            _update = null;
            _initialized = false;
            OnShutdown = null;
        }



        //get data2 from midi
        static int GetMidi(int data1, int port = -1, int channel = -1,MidiStatus status = MidiStatus.Dummy)
        {
            //initialize if necessary
            if (!_initialized)
            {
                Initialize();

                //quit if initialization was not done successfully
                if (!_initialized)
                    return 0;
            }
            // check if no port was given
            if (port < 0)
            {
                //check every port 
                for (int i = 0; i < currdevices.Length; i++)
                {
                    //if the note is active 
                    if (currdevices[i].DataActive(data1, status))
                    {
                        var data = currdevices[i].getData(data1);
                        if(channel > 0 || channel == data.channel)
                            return currdevices[i].getData(data1).data2;
                    }
                }
                return 0;
            }
            // if port was specified, find and get data from port given.
            else if (port < portCount && currdevices[port].DataActive(data1, status))
            {
                return currdevices[port].getData(data1).data2;
            }
            return 0;
        }



        static void RestartDevices()
        {
            //cannot restart device if it hasn't started in the first place,
            if (!_initialized)
                return;

            var newPortCount = GetPortCount();
            if (newPortCount == 0)
            {
                for (uint i = 0; i < portCount; i++)
                {
                    print("Midi Device Removed: " + currdevices[i].name);
                }

                portCount = 0;

                Shutdown();
            }
            else
            {
                //find the device removed/added 
                for (uint i = 0; i < newPortCount; i++)
                {
                    string name = MidiInternal.rtmidi_get_port_name(currdevices[0].ptr, i);
                    for (int j = 0; j < currdevices.Length; j++)
                    {
                        if (currdevices[j].name == name)
                            continue;
                        //once found, print message
                        if (currdevices[j].name != name && j == currdevices.Length - 1)
                        {
                            if (portCount < newPortCount)
                                print("Midi Device Added: " + name);
                            else
                                print("Midi Device Removed: " + currdevices[j].name);
                        }
                    }

                }

                portCount = newPortCount;

                for (uint i = 0; i < currdevices.Length; i++)
                {
                    removeDevice(i);
                }

                currdevices = null;
                currdevices = new MidiInDevice[portCount];
                for (uint i = 0; i < portCount; i++)
                {
                    currdevices[i] = CreateDevice(i);
                }
            }


        }

        #endregion

        #region Public Static Functions

        /// <summary>
        /// Find a device port based on the device name. <br></br>
        /// The name may be a portion of the actual device name. <br></br>
        /// This function can also handle multiple strings to narrow your search.
        /// </summary>
        /// <param name="name"> string that is contained in the desired device name.</param>
        /// <returns>port number referencing desired input device, MIDIINERROR if no device contained the name given.</returns>
        public static uint FindPortByName(params string[] name)
        {
            //initialize if necessary
            if (!_initialized)
            {
                Initialize();

                //return if the initialization process was incomplete
                if (!_initialized)
                    return MIDIINERROR;
            }
            for (uint i = 0; i < portCount; i++)
            {
                bool check = false;
                for (int j = 0; j < name.Length; j++)
                {
                    if (!currdevices[i].name.Contains(name[j]))
                        check = true;
                }

                if (!check)
                    return i;
            }

            //setup error msg
            string errMsg = "";
            for (uint i = 0; i < name.Length; i++)
            {
                errMsg += name[i] + " ";
            }
            //no devices with name found, send warning and give zero
            Debug.LogWarning(string.Format("No device port containing names: {0}.", errMsg));
            return MIDIINERROR;

        }

        //
        //There are certain Midi messages that do not follow the data1 = id, data2 = value format. 
        //In this script they are conidered "Special". 
        //the following functions get the last value that was given of this type.
        //

        public static byte[] GetLastSysex(uint port)
        {
            if (port >= portCount)
                return new byte[0];

            return currdevices[port].getLastSpecial(MidiStatus.Sysex).rawData;
        }

        public static byte GetLastProgramChange(uint port)
        {
            if (port >= portCount)
                return 0;

            return currdevices[port].getLastSpecial(MidiStatus.ProgramChange).data1;
        }

        /// <summary>
        /// gives the last Channel aftertouch value that was sent from device with port given.
        /// </summary>
        /// <param name="port">device port</param>
        /// <returns>Aftertouch value </returns>
        public static byte GetLastChannelAfterTouch(uint port)
        {
            if (port >= portCount)
                return 0;

            return currdevices[port].getLastSpecial(MidiStatus.PolyChannel).data1;
        }

        /// <summary>
        /// Initializes MidiInput and connects all Midi Devices.
        /// While it is not necessary to run this function as all Get"" functions intialize properly,
        /// this function exists to be able to initialize at Awake().
        /// </summary>
        public static void Initialize()
        {

            //run only if the system has not been initialized
            if (!_initialized)
            {
                //get port count 
                portCount = GetPortCount();
                //dont initialize if there are no midi devices available
                if (portCount == 0)
                {
                    Debug.LogError("MIDIIN: No Midi Device Available! Lopea Midi is not Initializing.");
                    return;
                }

                //get the gameobject with MidiInput Component
                _update = FindObjectOfType<MidiInput>();

                //if gameobject does not exist, make a new one.
                if (_update == null)
                {
                    _update = new GameObject("LopeaMidi").AddComponent<MidiInput>();
                }
                //hide the game object from everything
                //_update.gameObject.hideFlags = HideFlags.HideInHierarchy;

                currdevices = new MidiInDevice[portCount];
                //add all devices
                for (uint i = 0; i < portCount; i++)
                {
                    currdevices[i] = CreateDevice(i);
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
            //if value is not initialized, 
            if (!_initialized)
            {
                //start initialization process
                Initialize();
                //leave if something happened during the initialization process
                if (!_initialized)
                    return null;
            }

            //if device does not exist...
            if (port >= portCount)
            {
                //print message
                Debug.LogError(string.Format("Port Number {0} cannot be used for Midi Input!\nPort range 0-{1}", port,
                                                                                   portCount - 1));
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
        /// <param name="channel">Optional: specify the channel the note is in.</param>
        /// <returns>CC value</returns>
        public static int GetCCValue(int data1, int port = -1, int channel = -1)
        {
            return GetMidi(data1, port, channel, MidiStatus.ControlChange);
        }

        /// <summary>
        /// Get note velocity / velocity aftertouch.
        /// Returns note velocity from midi note value given.
        /// Finds note value from all devices unless specified a given port.
        /// </summary>
        /// <param name="data1">Midi Note Value</param>
        /// <param name="port">Optional: Device port</param>
        /// <param name="channel">Optional: specify the channel the note is in.</param>
        /// <returns>Velocity/Velocity Aftertouch</returns>
        public static int GetNoteValue(int data1, int port = -1, int channel = -1)
        {
            var aftertouch = GetMidi(data1, port, channel, MidiStatus.PolyKey);
            if (aftertouch != 0)
                return aftertouch;
            else
                return GetMidi(data1, port, channel, MidiStatus.NoteOn);
        }


        /// <summary>
        /// Check if Midi note is pressed
        /// returns true if note value is pressed and active, false if value is zero.
        /// It will find the note value from all devices unless specified a given port
        /// </summary>
        /// <param name="data1">Number representing CC midi value</param>
        /// <param name="port">Optional: device port to search CC midi value</param>
        /// <param name="channel">Optional: specify the channel the note is in</param>
        /// <returns>true if note value at data1 is not zero, false if zero</returns>
        public static bool GetNote(int data1, int port = -1, int channel = -1)
        {
            return GetNoteValue(data1, port, channel) != 0;
        }


        ///  <summary>
        /// Get Control Change status.
        /// Returns true if CC value is active and not zero, false if value is zero.
        /// It will find the CC value from all devices unless specified a given port
        ///  </summary>
        ///  <param name="data1">Number representing CC midi value</param>
        ///  <param name="port">Optional: device port to search CC midi value</param>
        ///  <param name="channel">Optional: specify the channel the note is in </param>
        ///  <returns>true if CC value at data1 is not zero, false if zero</returns>
        public static bool GetCC(int data1, int port = -1, int channel = -1)
        {
            return GetCCValue(data1, port, channel) != 0;
        }

        /// <summary>
        /// Gets the name of the port based on the port number given.
        /// </summary>
        /// <param name="port">port number to find the name of.</param>
        /// <returns></returns>
        public static string GetPortName(uint port)
        {
            //check if the value exists...
            if (port >= portCount)
            {
                //if not, print error and return nothing.
                Debug.LogError("MIDI port given does not exist when trying to find port name!");
                return string.Empty;
            }
            //if the system has not been initialized...
            if (!_initialized)
            {
                //create a reference to the port
                IntPtr refPort = MidiInternal.rtmidi_in_create_default();
                //get name
                string name = MidiInternal.rtmidi_get_port_name(refPort, port);
                //free port
                freeHandle(refPort);
                return name;
            }
            //system has already been initialized, get port name from device
            return currdevices[port].name;

        }

        public static bool OnMidi(MidiID id)
        {
            return GetMidiValue(id) != 0;
        }

        /// <summary>
        /// Get the 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int GetMidiValue(MidiID id)
        {
            return GetMidi(id.data1, id.port, id.channel, id.status);
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
                //check if a device has been added/removed.
                if (portCount != GetPortCount())
                {
                    RestartDevices();

                    //if there are no devices available, quit
                    if (!_initialized)
                        return;
                }
                //allocate memory for messages
                IntPtr messages = Marshal.AllocHGlobal(1024);
                IntPtr size = Marshal.AllocHGlobal(4);

                //loop for every device active 
                for (int i = 0; i < currdevices.Length; i++)
                {
                    int currsize = 0;

                    //loop indefinitely
                    while (true)
                    {
                        //write max size for parameter (max size for a midi message is 1024 bytes)
                        Marshal.WriteInt32(size, 1024);

                        //get message and store timestamp
                        double timestamp = MidiInternal.rtmidi_in_get_message(currdevices[i].ptr, messages, size);

                        //parse size 
                        currsize = Marshal.ReadInt32(size);

                        //if the message is empty, quit
                        if (currsize == 0)
                            break;

                        //store messages in array
                        byte[] m = new byte[currsize];
                        Marshal.Copy(messages, m, 0, currsize);

                        //
                        //parse message
                        //

                        //store new data
                        MidiData data;

                        //get status byte
                        byte status = m[0];

                        //store data
                        data = new MidiData((float)timestamp,                            //time since last midi message
                                            (MidiStatus)((status >> 4)),                 //midi type (8-15 defined)
                                            (status & 0x0F),                       //channel bit (0-15)
                                            m[1],                                   //data1 byte (stores midi note ID usually)
                                            (currsize == 2) ? byte.MinValue : m[2], //data2 byte (sometimes this is 0 due to the length of the message)
                                            m,                                           //raw data of the message
                                            i);                                      //device port ID

                        //some devices for whatever reason have both note on and off to be the same value
                        //so note on/off status is based on velocity
                        if (data.status == MidiStatus.NoteOn || data.status == MidiStatus.NoteOff)
                            data.status = (data.data2 != 0) ? MidiStatus.NoteOn : MidiStatus.NoteOff;

                        //add data to the device
                        currdevices[i].AddData(data);

                        
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