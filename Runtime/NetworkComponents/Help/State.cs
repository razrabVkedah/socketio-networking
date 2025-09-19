using UnityEngine;

namespace ClientSocketIO.NetworkComponents.Help
{
    public class State
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public readonly float BeginChangesTime;
        public readonly float Time;

        public State(Vector3 pos, Vector3 rot, Vector3 sc, float beginChangesTime, float t)
        {
            Position = pos;
            Rotation = rot;
            Scale = sc;
            BeginChangesTime = beginChangesTime;
            Time = t;
        }
    }
}