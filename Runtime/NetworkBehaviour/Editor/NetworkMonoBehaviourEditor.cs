using System.Reflection;
using ClientSocketIO.NetworkBehaviour.Attributes;
using UnityEditor;

namespace ClientSocketIO.NetworkBehaviour.Editor
{
    //[CustomEditor(typeof(NetworkMonoBehaviour), true)]
    //public class NetworkMonoBehaviourEditor : UnityEditor.Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();
    //
    //        var targetType = target.GetType();
    //        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    //
    //        foreach (var method in methods)
    //        {
    //            if (method.GetCustomAttribute<RPCAttribute>() != null)
    //            {
    //                EditorGUILayout.HelpBox($"{method.Name} has NetworkMethodAttribute", MessageType.Info);
    //            }
    //        }
    //    }
    //}
}