using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkRpc.Json;
using ClientSocketIO.NetworkData.NetworkVariables;
using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public class NetworkVariableHandler : InRoomHandlerBase<NetworkVariableHandler>
    {
        protected override void OnEnterRoom(EnterRoom _)
        {
            Client.ClientAddListener<string>(NetworkEventTypes.NetworkVariable, OnGetNetworkVariable);
        }

        protected override void OnLeaveRoom()
        {
            Client.ClientRemoveListener<string>(NetworkEventTypes.NetworkVariable, OnGetNetworkVariable);
        }

        private static void OnGetNetworkVariable(string data)
        {
            var networkVariableData = JsonUtility.FromJson<NetworkVariableData>(data);
            OnGetNetworkVariable(networkVariableData);
        }

        public static void OnGetNetworkVariable(NetworkVariableData data)
        {
            NetworkMonoBehavioursManager.GetNetworkMonoBehaviour(data.networkMonoBehaviourId)
                ?.ReceiveVariableFromServer(data);
        }

        public static void SendVariablesToServer(string data)
        {
            if (Instance.IsInRoom() == false) return;

            Debug.Log("Send variable to server " + data);
            Client.ClientEmitForce(NetworkEventTypes.NetworkVariable, data);
        }
    }
}