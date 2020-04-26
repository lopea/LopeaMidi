using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lopea.Midi.Internal;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Lopea.Midi.Editor
{
    public delegate void MidiIDFunc(MidiID context);
    public static class MidiInputEditor
    {
        
        private static bool _initialized = false;

        public static event MidiIDFunc OnNextNote;
        
        static IntPtr[] devices;

        private static double startTime;
        public static bool Initialize()
        {
            if (_initialized)
                return true;

            var portCount = MidiInput.GetPortCount();
            if (portCount == 0)
                return false;

            devices = new IntPtr[portCount];
            for (uint i = 0; i < portCount; i++)
            {
                devices[i] = MidiInternal.rtmidi_in_create_default();
                MidiInternal.rtmidi_in_ignore_types(devices[i], true, false, false);
                MidiInternal.rtmidi_open_port(devices[i], i, "Why am i doing this? " + i );
                
            }

            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            _initialized = true;
            return true;
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode &&  _initialized)
            {
                OnNextNote?.Invoke(new MidiID(-1, -1 , -1, MidiStatus.Dummy));
                Shutdown();
            }
        }

        private static void Update()
        {
            if(!_initialized)
                return;
            if (EditorApplication.isPlaying 
                || EditorApplication.timeSinceStartup - startTime > 10 
                || devices.Length != MidiInput.GetPortCount())
            {
                OnNextNote?.Invoke(new MidiID(-1, -1 , -1, MidiStatus.Dummy));
                Shutdown();
                return;
            }

            IntPtr messages = Marshal.AllocHGlobal(1024);
            IntPtr size = Marshal.AllocHGlobal(4);

            for (int i = 0; i < devices.Length; i++)
            {
                while (true)
                {
                    Marshal.WriteInt32(size, 1024);
                    
                    MidiInternal.rtmidi_in_get_message(devices[i], messages, size);
                    if(Marshal.ReadInt32(size) == 0)
                        break;

                    byte[] m = new byte[Marshal.ReadInt32(size)];
                    Marshal.Copy(messages, m, 0, m.Length);
                    var status = (MidiStatus) ((m[0] >> 4));

                    if (status == MidiStatus.NoteOff)
                        status = MidiStatus.NoteOn;
                    
                    OnNextNote?.Invoke(new MidiID(m[0] & 0x0F, m[1],i , status));
                    Shutdown();
                    Marshal.FreeHGlobal(size);
                    Marshal.FreeHGlobal(messages);
                    return;
                }
            }
            //deallocate pointers
            Marshal.FreeHGlobal(size);
            Marshal.FreeHGlobal(messages);
        }

        
        public static void Shutdown()
        {
            if (!_initialized)
                return;

            for (int i = 0; i < devices.Length; i++)
            {
                MidiInternal.rtmidi_close_port(devices[i]);
                MidiInternal.rtmidi_in_free(devices[i]);
                devices[i] = (IntPtr) 0;
            }
          
            _initialized = false;
            OnNextNote = null;
            EditorApplication.update = null;
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;

        }


    }
}
