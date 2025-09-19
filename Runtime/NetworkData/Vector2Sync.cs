using UnityEngine;

namespace ClientSocketIO.NetworkData
{
    // ReSharper disable InconsistentNaming
    public struct Vector2Sync
    {
        public float? x;
        public float? y;

        public Vector3 ToVector2() => new Vector2(x ?? 0, y ?? 0);

        public string ToJson()
        {
            var json = "{";
            if (x.HasValue) json += "x:" + x + ",";
            if (y.HasValue) json += "y:" + x + ",";
            json += "}";
            return json;
        }
    }
}