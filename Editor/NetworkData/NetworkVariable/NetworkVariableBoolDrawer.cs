using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [CustomPropertyDrawer(typeof(NetworkVariableBool))]
    public class NetworkVariableBoolDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> FoldoutStates = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var propertyKey = property.serializedObject.targetObject.GetInstanceID() + "." + property.propertyPath;
            FoldoutStates.TryAdd(propertyKey, false);
            FoldoutStates[propertyKey] =
                EditorGUI.Foldout(position, FoldoutStates[propertyKey], property.displayName, true);
            if (FoldoutStates[propertyKey])
            {
                EditorGUI.indentLevel++;

                if (PrefabUtility.IsPartOfPrefabAsset(property.serializedObject.targetObject))
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("_variable"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("AuthorityMode"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("SyncMode"));
                }
                else
                {
                    var targetObject = property.serializedObject.targetObject;
                    var field = targetObject.GetType().GetField(property.propertyPath,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (field != null)
                    {
                        if (field.GetValue(targetObject) is NetworkVariableBool networkVariableBool)
                        {
                            var valueProperty = networkVariableBool.GetType().GetProperty("Value",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (valueProperty != null)
                            {
                                var value = (bool)valueProperty.GetValue(networkVariableBool);
                                value = EditorGUILayout.Toggle("Value", value);
                                valueProperty.SetValue(networkVariableBool, value);
                            }
                        }
                    }
                }
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}