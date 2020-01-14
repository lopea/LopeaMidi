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
namespace Lopea.Midi.Internal
{
    [HideInInspector]
    public class MidiInput : MonoBehaviour
    {
        class MidiInDevice
        {
            //public variables
            public uint port;
            public IntPtr ptr;
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
                if(isSpecial(data.status))
                {
                    int index = 11 - (int)data.status;
                    if(!activedata.ContainsKey(index))
                        activedata.Add(index,data);
                    else
                        activedata[index] = data;
                }
                else
                {
                if(activedata.ContainsKey(data.data1))
                {   
                     activedata[data.data1] = data;
                }
                else
                {    
                    activedata.Add(data.data1,data);
                }
                }
            }
            public MidiData getData(int index)
            {
                if(activedata.ContainsKey(index))
                    return activedata[index];
                return null;
            }
            public void UpdateData()
            {
                for(int i = 0; i < activedata.Count; i ++)
                {
                    var status = activedata.ElementAt(i).Value.status;
                    if(status == MidiStatus.NoteOff)
                        activedata.Remove(activedata.ElementAt(i).Key);
                }
            }
        }
        
        #region Static Variables

        //store all active devices
        static MidiInDevice[] currdevices;

        //check if updater function is active
        static bool _initialized = false;

        //store monobehaviour object to use unity functions
        static MidiInput _update;

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
            MidiInDevice device = new MidiInDevice(port,reference, name);

            //add to array
            if(currdevices == null)
            {
                currdevices = new MidiInDevice[1];
                currdevices[0] = device;
            }
            else
            {
                var newdevices = new MidiInDevice[currdevices.Length + 1];
                for(int i = 0; i < currdevices.Length; i++)
                    newdevices[i] = currdevices[i];
                newdevices[currdevices.Length] = device;
                currdevices = newdevices;
            }
        }

        static void removeDevice(uint port)
        {
            if(port >= GetPortCount())
                return;

            //free ptr
            freeHandle(currdevices[port].ptr);
            currdevices[port] = null;
        }

        static void Shutdown()
        {
            for(uint i = 0; i < currdevices.Length; i ++)
            {
                removeDevice(i);
            }
            currdevices = null;
            _update = null;
            _initialized = false;
        }
        #endregion

        #region Public Static Functions
        //initializes MidiInput
        public static void Initialize()
        {
            //create the update object
            if (!_initialized)
            {
                //create a gameobject with MidiInput Component
                _update = new GameObject("LopeaMidi").AddComponent<MidiInput>();

                //hide the game object from everything
                _update.gameObject.hideFlags = HideFlags.HideInHierarchy;
                //add all devices
            uint count = GetPortCount(); 
            for(uint i = 0; i < count; i++ )
            {
                addDevice(i);
            }
            _initialized = true;
            }
        }

         //returns the number of Midi ports available
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

        public static MidiData GetData(uint data1, uint port)
        {
            if(!_initialized)
                Initialize();

            return currdevices[port].getData((int) data1);    
        }

       
        #endregion
        
        #region MonoBehaviour Functions
        
        void OnDisable()
        {
            Shutdown();
        }
        void Update()
        {
            if(_initialized)
            {
                

                //loop for every device active 
                for(int i = 0; i < currdevices.Length; i++)
                {
                    currdevices[i].UpdateData();
                    //loop indefinitely
                    while(true)
                    {
                        //allocate memory for messages
                        IntPtr messages = Marshal.AllocHGlobal(1024);
                        IntPtr size = Marshal.AllocHGlobal(4);
                        //get message and store timestamp
                        double timestamp = MidiInternal.rtmidi_in_get_message(currdevices[i].ptr, messages, size);

                        //parse size 
                        int currsize = Marshal.ReadInt32(size);
                        
                        //if the message is empty, quit
                        if(currsize == 0)
                        {
                            Marshal.FreeHGlobal(size);
                            Marshal.FreeHGlobal(messages);
                            break;
                        }
                        //
                        //parse message
                        //

                        //store new data
                        MidiData data;

                        //get status byte
                        byte status = Marshal.ReadByte(messages);
                        
                       

                        //store data
                        data = new MidiData((float)timestamp, 
                                            (MidiStatus)(status >> 4), 
                                            (status & 0x0F), 
                                            Marshal.ReadByte(messages, 1),
                                            (currsize == 2) ? byte.MinValue : Marshal.ReadByte(messages, 2),
                                            null);
                        
                        //some devices for whatever reason have both note on and off to be the same value
                        //so note on/off status is based on velocity
                        data.status = (data.data2 !=0) ? MidiStatus.NoteOn: MidiStatus.NoteOff;

                        if(data.status == MidiStatus.Sysex)
                        {
                            byte[] sys = new byte[currsize];
                            
                            for(int j = 0; j < currsize; j++)
                            {
                                print( Marshal.ReadByte(messages,j));
                                sys[j] = Marshal.ReadByte(messages,j);
                            }
                            data.sysexMessage = sys;
                        }
                      
                        currdevices[i].AddData(data);
                        //deallocate pointers
                        Marshal.FreeHGlobal(size);
                        Marshal.FreeHGlobal(messages);
                    }
                }

                
            }
            
        }
        void LateUpdate()
        {
            
        }
        #endregion
    }
}