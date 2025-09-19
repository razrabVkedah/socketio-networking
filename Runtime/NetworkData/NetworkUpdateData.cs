using System;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace ClientSocketIO.NetworkData
{
    [Serializable]
    public class Wrapper<T>
    {
        public T[] transformData;
    }
    
    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class TransformData
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class NetworkUpdateData
    {
        public TransformData[] transformData;

        public string ToJson()
        {
            var wrapper = new Wrapper<TransformData>
            {
                transformData = transformData
            };
            return JsonUtility.ToJson(wrapper);
        }
    }
}