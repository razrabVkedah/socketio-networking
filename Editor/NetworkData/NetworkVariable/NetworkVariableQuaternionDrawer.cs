using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [CustomPropertyDrawer(typeof(NetworkVariableQuaternion))]
    public class NetworkVariableQuaternionDrawer : PropertyDrawer
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
                    var q = property.FindPropertyRelative("_variable");
                    var quaternion = q.quaternionValue;
                    quaternion.x = EditorGUILayout.FloatField("X", quaternion.x);
                    quaternion.y = EditorGUILayout.FloatField("Y", quaternion.y);
                    quaternion.z = EditorGUILayout.FloatField("Z", quaternion.z);
                    quaternion.w = EditorGUILayout.FloatField("W", quaternion.w);
                    q.quaternionValue = quaternion;
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
                        if (field.GetValue(targetObject) is NetworkVariableQuaternion networkVariableQuaternion)
                        {
                            var valueProperty = networkVariableQuaternion.GetType().GetProperty("Value",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (valueProperty != null)
                            {
                                var value = (Quaternion)valueProperty.GetValue(networkVariableQuaternion);
                                value.x = EditorGUILayout.FloatField("X", value.x);
                                value.y = EditorGUILayout.FloatField("Y", value.y);
                                value.z = EditorGUILayout.FloatField("Z", value.z);
                                value.w = EditorGUILayout.FloatField("W", value.w);
                                valueProperty.SetValue(networkVariableQuaternion, value);
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