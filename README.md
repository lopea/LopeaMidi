# LopeaMidi
A cross platform MIDI wrapper for Unity3D using RtMidi with functionality for device specific features.

## Requirements
  - Linux with ALSA
  - Windows
  (Mac OS not tested yet and will not work for now.)

## How To Use
NOTE: There is no need to add a GameObject or anything, just use these functions in your script and you are good to go.
### Note/Control Change (CC) input
To get the value of a note/CC value, use
 ```csharp
 int MidiInput.GetNoteValue(int noteID);
 int MidiInput.GetNoteValue(int ccID);
 ```
 respectively.
 
 
 
To get the status of a note/CC value (whether the note is pressed / CC is being used), use
```csharp
bool MidiInput.GetNote(int noteID);
bool MidiInput.GetCC(int ccID);
```
respectively.

You can also specify the type of device that you want to use by adding the device port into these functions.
```csharp
int MidiInput.GetNoteValue(int noteID, int port);
int MidiInput.GetNoteValue(int ccID, int port);
bool MidiInput.GetNote(int noteID, int port);
bool MidiInput.GetCC(int ccID, int port);
```
---
## Device Specific Features
There are helper functions for multiple devices. These devices are:

  -Launchpad Pro

---
# TODO:
  
  -Finish Launchpad pro helper functions
  
  -Add Launch Control XL 
  
  -Integrate with SuperControls
