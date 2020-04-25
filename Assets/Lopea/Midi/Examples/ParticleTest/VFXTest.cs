using System;
using System.Collections;
using System.Collections.Generic;
using Lopea.Midi;
using Lopea.Midi.Devices;
using UnityEditor;
using UnityEngine;

public class VFXTest : MonoBehaviour
{

    ParticleSystem vs;

    [SerializeField] private Color HighlightedColor = Color.red;

    void Awake()
    {
        vs = GetComponent<ParticleSystem>();
    }
    
    void Update()
    {
        var main = vs.main;
        var size = vs.sizeOverLifetime;
        if (MidiInput.GetNote(0x3C, 0))
        {
            main.startSizeMultiplier = 3;
            size.sizeMultiplier += 0.3f;
        }

        if (main.startSizeMultiplier > 1)
        {
            main.startSizeMultiplier -= Time.deltaTime * 6;
        }
        else
        {
            main.startSizeMultiplier = 1;
        }
        
        float t = Mathf.InverseLerp(1, 3, main.startSizeMultiplier);
        main.startColor = Color.Lerp(Color.white, HighlightedColor, t);
        size.sizeMultiplier = Mathf.Lerp(1, 1.3f, t);
        var v = vs.noise;
        v.frequency = v.strengthMultiplier = 6 * MidiInput.GetCCValue(0x4D, 0)/(float)MidiInput.MaxMidiValue;

    }
}
