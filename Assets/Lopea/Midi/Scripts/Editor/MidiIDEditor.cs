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
        position.height /= 5;
        var button = new GUIContent("Get Midi Value");
        if (wait)
        {
            button.text = "Waiting for Input...";

        }


        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        if (EditorApplication.isPlaying)
        {
            EditorGUI.HelpBox(position, "Cannot get next midi value during play mode.", MessageType.Warning);
        }
        else if (GUI.Button(position, button) && !wait && !EditorApplication.isPlaying)
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
                    property.FindPropertyRelative("status").intValue = (int)context.status;
                    property.serializedObject.ApplyModifiedProperties();
                    wait = false;
                };
                MidiInputEditor.Initialize();
            }
            
            
        }

        EditorGUI.indentLevel++;

        position.y += position.height;
        position.x = startpos.x;
        position.width = startpos.width;

        EditorGUI.PropertyField(position, property.FindPropertyRelative("port"), new GUIContent("Device Port"));
        position.y += position.height;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("channel"), new GUIContent("Midi Channel"));
        position.y += position.height;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("data1"), new GUIContent("Midi ID#"));
        position.y += position.height;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("status"), new GUIContent("Midi Type"));
        property.serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel--;
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 5;
    }
}
