using UnityEngine;

namespace ClientSocketIO.NetworkData
{
    // ReSharper disable InconsistentNaming
    public struct Vector3Sync
    {
        public float? x;
        public float? y;
        public float? z;

        public Vector3 ToVector3() => new Vector3(x ?? 0, y ?? 0, z ?? 0);

        public string ToJson()
        {
            var json = "{";
            if (x.HasValue) json += "x:" + x + ",";
            if (y.HasValue) json += "y:" + x + ",";
            if (z.HasValue) json += "z:" + x + ",";
            json += "}";
            return json;
        }
    }
}