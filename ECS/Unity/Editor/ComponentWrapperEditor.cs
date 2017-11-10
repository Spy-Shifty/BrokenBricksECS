using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ECS {
    [CustomEditor(typeof(ComponentWrapper), true)]
    public class ComponentDataWrapperEditor : Editor {
        public override void OnInspectorGUI() {
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            GUI.enabled = false;
            EditorGUILayout.PropertyField(prop, false, new GUILayoutOption[0]);
            GUI.enabled = true;


            if(target.GetType().GetCustomAttributes(typeof(HideInInspector), false).Any()) {
                return;
            }

            SerializedProperty property = serializedObject.FindProperty("_component");
            if(property == null) {
                EditorGUILayout.HelpBox("Component cant be displayed! Please make sure it has the [Serializable] attribute attached!", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            bool showChildren = true;
            while (property.NextVisible(showChildren)) {
                 EditorGUILayout.PropertyField(property,true);
                showChildren = false;
            }
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                (target as ComponentWrapper).SetComponentToEntityManager();
            }
        }
    }
}