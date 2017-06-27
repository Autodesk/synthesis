using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.FSM
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : Editor
    {
        public const string DefaultStateNameKey = "DefaultStateName";
        string defaultStateName;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StateMachine machine = (StateMachine)target;
            
            defaultStateName = EditorGUILayout.TextField("Default State Name", defaultStateName ??
                (EditorPrefs.HasKey(DefaultStateNameKey) ? EditorPrefs.GetString(DefaultStateNameKey) : string.Empty));

            EditorPrefs.SetString(DefaultStateNameKey, defaultStateName);
        }
    }
}
