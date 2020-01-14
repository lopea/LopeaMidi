using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lopea.Midi.Internal;
using System;
using System.Runtime.InteropServices;

public class cool : MonoBehaviour
  {
        IntPtr ptr, o;

        [Range(0, 100)]
        public float xoff = 0, yoff = 0;

        byte[] header = { 240, 0, 32, 41, 2, 16 };

        static int currindex;

        [SerializeField]
        Gradient gradient;
        Gradient oldg;
        Vector2 center = new Vector2(4.5f, 4.5f);
        
        float timer = 0;
        List<byte> list = new List<byte>();

        // Start is called before the first frame update
        void Start()
        {

            ptr = MidiInternal.rtmidi_in_create_default();
            o = MidiInternal.rtmidi_out_create_default();

            MidiInternal.rtmidi_open_port(ptr, 2, "da.");
            MidiInternal.rtmidi_open_port(o, 2, "yus.");
            // StartCoroutine(getdata());
        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime * 10;
            list = new List<byte>();
            for (int i = 0; i < header.Length; i++)
                list.Add(header[i]);
            list.Add(15);
            list.Add(0);
            for (int i = 0, y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++, i++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    //Color col = gradient.Evaluate(Mathf.Sin((x + y + Time.time) * Mathf.PI)); // checkerboard pattern

                    Color col = gradient.Evaluate(Mathf.Sin(dist + Time.time * 20) / 2 + 0.5f);
                    if(Mathf.Abs(dist - timer) < 1)
                    {
                        list.Add((byte)(col.r * 63));
                        list.Add((byte)(col.g * 63));
                        list.Add((byte)(col.b * 63));
                    }
                    else
                    {
                        list.Add(0);
                        list.Add(0);
                        list.Add(0);
                    }
                   
                }
            }

            list.Add(247);
            MidiInternal.rtmidi_out_send_message(o, list.ToArray(), list.Count);

            //store pointers 
            IntPtr message = IntPtr.Zero;
            IntPtr size = IntPtr.Zero;

            //allocate pointers 
            size = Marshal.AllocHGlobal(4);
            message = Marshal.AllocHGlobal(1024);

            // useful code right here
            while (true)
            {


                //get data from Midi device
                MidiInternal.rtmidi_in_get_message(ptr, message, size);

                //store size of message
                int s = Marshal.ReadInt32(size);

                //check if no message is sent
                if (s == 0)
                {
                    //de-allocate and exit loop
                    Marshal.FreeHGlobal(message);
                    Marshal.FreeHGlobal(size);
                    break;
                }
                
                //change center position
                byte[] data = new byte[s];
                for(int i = 0; i < data.Length; i ++)
                {
                    data[i] = Marshal.ReadByte(message,i);
                }
                
                //change center 
                changeCenter(data);
            }

        }

        void changeCenter(byte[] data)
        {
            if(data[2] == 0)
                return;
            byte pos = data[1];
            center = new Vector2(pos % 10, pos/10);
            timer = 0;
        }
        void OnDisable()
        {
            MidiInternal.rtmidi_in_free(ptr);
            MidiInternal.rtmidi_out_free(o);

            ptr = IntPtr.Zero;
            o = IntPtr.Zero;
        }




    }
