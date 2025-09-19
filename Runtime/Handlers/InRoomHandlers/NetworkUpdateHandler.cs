using System.Collections.Generic;
using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkVariables;
using ClientSocketIO.Types.NetworkUpdate;
using ClientSocketIO.Types.NetworkUpdate.Json;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace ClientSocketIO.Handlers
{
    public class NetworkUpdateHandler : InRoomHandlerBase<NetworkUpdateHandler>
    {
        public static readonly UnityEvent OnSendDataToServer = new();
        public static int TickRate;
        public static int HostTimeOffset;
        private float _timer;

        public static float GetServerTime()
        {
            return Time.unscaledTime + HostTimeOffset / 1000f;
        }

        public const float InterpolationStateDefaultLifetime = 5f;
        private const float InterpolationSafetyBuffer = 0.05f;

        public static float GetInterpolationTime(float safetyBuffer = InterpolationSafetyBuffer)
        {
            var interpolationDelay = PingHandler.AveragePingMilliseconds() / 1000f + safetyBuffer +
                                     1f / TickRate;
            return GetServerTime() - interpolationDelay;
        }

        protected override void OnEnterRoom(EnterRoom _)
        {
            Client.ClientAddListener<string>(NetworkEventTypes.NetworkUpdate, OnGetNetworkData);
        }

        protected override void OnLeaveRoom()
        {
            Client.ClientRemoveListener<string>(NetworkEventTypes.NetworkUpdate, OnGetNetworkData);
        }

        private void Update()
        {
            if (IsInRoom() == false) return;

            _timer += Time.deltaTime;
            if (_timer < 1.0f / TickRate) return;

            SendDataToServer();
            _timer = 0;
        }

        private static readonly List<BaseNetworkData> ToSendData = new();

        public static void AddDataToSend(BaseNetworkData data)
        {
            for (var i = ToSendData.Count - 1; i >= 0; i--)
            {
                if (ToSendData[i].dataType != data.dataType) continue;
                if (ToSendData[i].networkMonoBehaviourId != data.networkMonoBehaviourId) continue;
                ToSendData.RemoveAt(i);
                i--;
            }

            ToSendData.Add(data);
        }

        private static void SendDataToServer()
        {
            if (ToSendData.Count == 0) return;

            Client.ClientEmitForce(NetworkEventTypes.NetworkUpdate, JsonConvert.SerializeObject(ToSendData));
            ToSendData.Clear();
            OnSendDataToServer.Invoke();
        }

        private static void OnGetNetworkData(string data)
        {
            var baseDataList =
                JsonConvert.DeserializeObject<List<BaseNetworkData>>(data,
                    JsonConverterSettingsData.serializerSettings);

            foreach (var d in baseDataList)
            {
                switch (d)
                {
                    case NetworkRpcData rpcData:
                        NetworkRpcHandler.OnGetNetworkRpc(rpcData);
                        break;
                    case NetworkVariableData variableData:
                        NetworkVariableHandler.OnGetNetworkVariable(variableData);
                        break;
                    default:
                        NetworkMonoBehavioursManager.GetNetworkMonoBehaviour(d.networkMonoBehaviourId)
                            ?.SetDataFromServer(d);
                        break;
                }
            }
        }
    }
}