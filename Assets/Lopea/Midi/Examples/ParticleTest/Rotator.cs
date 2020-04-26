using System.Collections;
using System.Collections.Generic;
using Lopea.Midi;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis;

    [SerializeField] private float speed;
    [SerializeField] private MidiID OnRotate;
    private float currrot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float midi = (MidiInput.GetMidiValue(OnRotate) / (float) MidiInput.MaxMidiValue);
        currrot += midi  * speed * Time.deltaTime ;
        transform.rotation = Quaternion.Euler(axis * currrot);
        
    }
}
