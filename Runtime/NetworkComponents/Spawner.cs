using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkBehaviour.Attributes;
using ClientSocketIO.NetworkData;
using UnityEngine;

namespace ClientSocketIO.NetworkComponents
{
    public class Spawner : NetworkMonoBehaviour
    {
        public void Spawn()
        {
            if (Client.IsHost == false)
                InvokeRPC(SpawnRpc);
            else
            {
                SpawnRpc();
            }
        }

        [RPCAttribute]
        private void SpawnRpc()
        {
            Debug.Log("SpawnRpc");
            NetworkSpawner.Spawn(0, Vector3.zero, Quaternion.identity);
        }
    }
}