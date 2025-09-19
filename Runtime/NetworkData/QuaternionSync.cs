using UnityEngine;

namespace ClientSocketIO.NetworkData
{
    // ReSharper disable InconsistentNaming
    public struct QuaternionSync
    {
        public float? x;
        public float? y;
        public float? z;
        public float? w;

        public Quaternion ToQuaternion() => new Quaternion(x ?? 0, y ?? 0, z ?? 0, w ?? 0);

        public string ToJson()
        {
            var json = "{";
            if (x.HasValue) json += "x:" + x + ",";
            if (y.HasValue) json += "y:" + x + ",";
            if (z.HasValue) json += "z:" + x + ",";
            if (w.HasValue) json += "w:" + x + ",";
            json += "}";
            return json;
        }
    }
}