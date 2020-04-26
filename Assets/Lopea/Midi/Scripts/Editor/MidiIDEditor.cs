using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Lopea.Midi;
using Lopea.Midi.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MidiID))]
public class MidiIDEditor : PropertyDrawer
{ 
    private bool wait = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var startpos = position;
        position.height /= 4;
        var button = new GUIContent("Get Midi Value");
        if (wait)
        {
            button.text = "Waiting for Input...";
           
        }

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        if (GUI.Button(position, button) && !wait)
        {
            wait = !wait;
            if (wait)
            {
                MidiInputEditor.OnNextNote += context =>
                {
                    if (context.port < 0 || context.channel < 0 || context.channel < 0)
                    {
                        wait = false;
                        return;
                    }
                    property.FindPropertyRelative("port").intValue = context.port;
                    property.FindPropertyRelative("channel").intValue = context.channel;
                    property.FindPropertyRelative("data1").intValue = context.data1;
                    property.serializedObject.ApplyModifiedProperties();
                    wait = false;
                } ;
                MidiInputEditor.Initialize();
            }
            
            
        }

        position.y += position.height;
        position.x = startpos.x;
        position.width = startpos.width;
        property.FindPropertyRelative("port").intValue =
            EditorGUI.IntField(position, "Device Port",property.FindPropertyRelative("port").intValue);
        position.y += position.height;
        property.FindPropertyRelative("channel").intValue = 
            EditorGUI.IntField(position, "Channel", property.FindPropertyRelative("channel").intValue);
        position.y += position.height;
        property.FindPropertyRelative("data1").intValue = 
            EditorGUI.IntField(position, "MIDI #",property.FindPropertyRelative("data1").intValue);
        property.serializedObject.ApplyModifiedProperties();

    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 4;
    }
}
