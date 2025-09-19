using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkRpc.Json;
using Newtonsoft.Json;
using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public class NetworkSpawner : Singleton<NetworkSpawner>
    {
        [SerializeField] private NetworkMonoBehaviour[] networkPrefabs;

        #region Host_Only

        /// <summary>
        /// Данный метод вызывается только на стороне хоста
        /// </summary>
        /// <param name="prefabId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void Spawn(int prefabId, Vector3 position, Quaternion rotation)
        {
            if (Client.IsHost == false) return;

            var spawned = Instantiate(Instance.networkPrefabs[prefabId], position, rotation);
            var newId = NetworkMonoBehavioursManager.GetFreeNetworkMonoBehaviourId();
            spawned.SetNetworkMonoBehaviourId(newId);

            var spawnParameters = new SpawnParameters
            {
                PrefabId = prefabId,
                Position = position,
                Rotation = rotation,
                NetworkMonoBehaviourId = newId
            };
            var rpcData = new NetworkRpcData
            {
                networkMonoBehaviourId = 1,
                RpcMethodName = "",
                MethodParams = JsonConvert.SerializeObject(spawnParameters, JsonConverterSettingsRpc.serializerSettings)
            };
            NetworkRpcHandler.SendRpcToServer(rpcData.ToJson());
        }

        public static void DestroySpawned(int networkMonoBehaviourId)
        {
            if (Client.IsHost == false) return;
        }

        #endregion

        #region Client_Only

        public static void Spawn(string parameters)
        {
            var spawnParameters = JsonConvert.DeserializeObject<SpawnParameters>(parameters);
            Debug.Log(spawnParameters.PrefabId);

            var spawned = Instantiate(Instance.networkPrefabs[spawnParameters.PrefabId], spawnParameters.Position,
                spawnParameters.Rotation);
            spawned.SetNetworkMonoBehaviourId(spawnParameters.NetworkMonoBehaviourId);
        }

        public static void DestroySpawned(string parameters)
        {
            
        }
        
        #endregion
    }
}