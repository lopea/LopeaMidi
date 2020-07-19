# LopeaMidi
A cross platform [MIDI](https://wikipedia.org/wiki/MIDI) wrapper for Unity3D using [RtMidi](https://www.music.mcgill.ca/~gary/rtmidi/) with functionality for device specific features.

## Supported Operating Systems
  - Linux with ALSA
  - Windows (Must have VS C++ Redistributable in order for this to work) 
  
  **Note:** Mac OS will not work with this library as I will need a mac to make it work. if you have a Mac and are willing to be my guinea pig for this project, feel free to     contact me.

## How To Use
```csharp
using Lopea.Midi;
```
### MIDI Input
To get the value of a note or CC value of any MIDI device, use
 ```csharp
 int MidiInput.GetNoteValue(int noteID);
 int MidiInput.GetNoteValue(int ccID);
 ```
 respectively.
 
 
 
To get the status of a note/CC value (whether the note is pressed / CC is being used) of any MIDI Device, use
```csharp
bool MidiInput.GetNote(int noteID);
bool MidiInput.GetCC(int ccID);
```
respectively.

You can also specify the type of MIDI device that you want to use by adding the device port into these functions.
```csharp
int MidiInput.GetNoteValue(int noteID, int port);
int MidiInput.GetNoteValue(int ccID, int port);
bool MidiInput.GetNote(int noteID, int port);
bool MidiInput.GetCC(int ccID, int port);
```

For Unity Editor support, you can use the MidiID variable to change the type of midi note to find for your project and easily change it's values in the editor.

Example:
```csharp
public class ExampleScript : MonoBehavior
{
  public MidiID OnRotate;
  
  void Update()
  {
    if(MidiInput.GetNote(OnRotate)
    {
      //do some rotating idk
    }
  }
}
```
**What is shown in the editor:**

![](https://i.imgur.com/4oimtJ1.png)

**Clicking 'Get MIDI Value' will change the values on the MidiID based on the next midi message that gets sent to any Midi device connected.**

### MIDI Output
Sending MIDI data to a device can happen in multiple ways:

If the message that is being sent is a single note, use
```csharp
//port: output device id to send data to.
//status: type of note to send to the device.(NoteOn, NoteOff, CC, etc.)
//data1: first half of note. (usually note value)
//data2: second half of note. (usually note velocity)
//channel: channel number to send the channel to the device.
void MidiOutput.SendSimpleData(uint port, MidiStatus status, byte data1, byte data2, byte channel =0);
```
to send the single note.

If more complicated messages(like Sysex) need to be sent, use 
```csharp
//port: output device id to send data to.
//data: data that gets sent straight to the device.
void MidiOutput.SendRawData(uint port, byte[] data);
```

---
## Device Specific Features
There are specific functions to help with certain devices and their features.

Supported Devices:

  -Launchpad Pro
  
  -LaunchControl XL (Partial)
