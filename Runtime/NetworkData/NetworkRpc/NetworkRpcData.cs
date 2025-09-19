using ClientSocketIO.Types.NetworkUpdate;
using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkRpc
{
    public class NetworkRpcData : BaseNetworkData
    {
        public string RpcMethodName;
        public string MethodParams;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public NetworkRpcData()
        {
            
        }

        public NetworkRpcData(string data)
        {
            
        }
    }
}