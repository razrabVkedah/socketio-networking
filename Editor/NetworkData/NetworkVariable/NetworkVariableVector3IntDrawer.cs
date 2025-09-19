using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [CustomPropertyDrawer(typeof(NetworkVariableVector3Int))]
    public class NetworkVariableVector3IntDrawer : PropertyDrawer
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
                    if (property.FindPropertyRelative("SyncMode").enumValueFlag == (int)SyncMode.Calm)
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("InterpolationType"));
                }
                else
                {
                    var targetObject = property.serializedObject.targetObject;
                    var field = targetObject.GetType().GetField(property.propertyPath,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (field != null)
                    {
                        if (field.GetValue(targetObject) is NetworkVariableVector3Int networkVariableVector3Int)
                        {
                            var valueProperty = networkVariableVector3Int.GetType().GetProperty("Value",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (valueProperty != null)
                            {
                                var value = (Vector3Int)valueProperty.GetValue(networkVariableVector3Int);
                                value = EditorGUILayout.Vector3IntField("Value", value);
                                valueProperty.SetValue(networkVariableVector3Int, value);
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