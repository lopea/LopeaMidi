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
### Note/Control Change (CC) input
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
### Midi Output
---
## Device Specific Features
There are specific functions to help with certain devices and their features.

Supported Devices:

  -Launchpad Pro
  
  -LaunchControl XL (Partial)
