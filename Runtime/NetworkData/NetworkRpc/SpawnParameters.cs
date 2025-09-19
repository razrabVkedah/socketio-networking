using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkRpc
{
    public class SpawnParameters
    {
        public int PrefabId;
        public Vector3 Position;
        public Quaternion Rotation;
        public int NetworkMonoBehaviourId;
    }
}