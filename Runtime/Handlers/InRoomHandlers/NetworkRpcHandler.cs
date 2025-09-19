using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkRpc.Json;
using Newtonsoft.Json;
using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public class NetworkRpcHandler : InRoomHandlerBase<NetworkRpcHandler>
    {
        protected override void OnEnterRoom(EnterRoom _)
        {
            Client.ClientAddListener<string>(NetworkEventTypes.NetworkRpc, OnGetNetworkRpc);
        }

        protected override void OnLeaveRoom()
        {
            Client.ClientRemoveListener<string>(NetworkEventTypes.NetworkRpc, OnGetNetworkRpc);
        }

        private static void OnGetNetworkRpc(string data)
        {
            var rpcData = JsonUtility.FromJson<NetworkRpcData>(data);
            OnGetNetworkRpc(rpcData);
        }

        public static void OnGetNetworkRpc(NetworkRpcData rpcData)
        {
            if (rpcData.networkMonoBehaviourId == 1)
            {
                NetworkSpawner.Spawn(rpcData.MethodParams);
                return;
            }
            var parameters =
                JsonConvert.DeserializeObject<object[]>(rpcData.MethodParams,
                    JsonConverterSettingsRpc.serializerSettings);
            NetworkMonoBehavioursManager.GetNetworkMonoBehaviour(rpcData.networkMonoBehaviourId)
                ?.RemoteProcedureCall(rpcData.RpcMethodName, parameters);
        }

        public static void SendRpcToServer(string data)
        {
            if (Instance.IsInRoom() == false) return;

            Client.ClientEmitForce(NetworkEventTypes.NetworkRpc, data);
        }
    }
}