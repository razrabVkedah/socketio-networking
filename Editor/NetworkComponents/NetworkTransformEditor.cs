using ClientSocketIO.NetworkComponents.Help;
using ClientSocketIO.NetworkData;
using UnityEditor;
using UnityEngine;

namespace ClientSocketIO.NetworkComponents
{
    [CustomEditor(typeof(NetworkTransform))]
    public class NetworkTransformEditor : Editor
    {
        private static bool _foldoutPosition;
        private static bool _foldoutRotation;
        private static bool _foldoutScale;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var tr = (NetworkTransform)target;
            tr.authorityMode = (AuthorityMode)EditorGUILayout.EnumPopup("Authority Mode", tr.authorityMode);
            tr.interpolationType = (InterpolationType)EditorGUILayout.EnumPopup("Interpolation", tr.interpolationType);
            
            _foldoutPosition = EditorGUILayout.Foldout(_foldoutPosition, "Sync Position");
            if (_foldoutPosition)
            {
                EditorGUI.indentLevel++;
                tr.syncPositionX = EditorGUILayout.Toggle("X", tr.syncPositionX);
                tr.syncPositionY = EditorGUILayout.Toggle("Y", tr.syncPositionY);
                tr.syncPositionZ = EditorGUILayout.Toggle("Z", tr.syncPositionZ);
                EditorGUI.indentLevel--;
            }

            _foldoutRotation = EditorGUILayout.Foldout(_foldoutRotation, "Sync Rotation");
            if (_foldoutRotation)
            {
                EditorGUI.indentLevel++;
                tr.syncRotationX = EditorGUILayout.Toggle("X", tr.syncRotationX);
                tr.syncRotationY = EditorGUILayout.Toggle("Y", tr.syncRotationY);
                tr.syncRotationZ = EditorGUILayout.Toggle("Z", tr.syncRotationZ);
                EditorGUI.indentLevel--;
            }

            _foldoutScale = EditorGUILayout.Foldout(_foldoutScale, "Sync Scale");
            if (_foldoutScale)
            {
                EditorGUI.indentLevel++;
                tr.syncScaleX = EditorGUILayout.Toggle("X", tr.syncScaleX);
                tr.syncScaleY = EditorGUILayout.Toggle("Y", tr.syncScaleY);
                tr.syncScaleZ = EditorGUILayout.Toggle("Z", tr.syncScaleZ);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                Undo.RecordObject(target, "Changed ItemsData");
                EditorUtility.SetDirty(tr);
                AssetDatabase.SaveAssetIfDirty(tr);
            }
        }
    }
}