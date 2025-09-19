using System.Reflection;
using ClientSocketIO.NetworkBehaviour.Attributes;

namespace ClientSocketIO.NetworkData.NetworkRpc
{
    public class RpcMethodInfo
    {
        public readonly MethodInfo MethodInfo;
        public readonly RPCAttribute RPCAttribute;

        public RpcMethodInfo(MethodInfo methodInfo, RPCAttribute rpcAttribute)
        {
            MethodInfo = methodInfo;
            RPCAttribute = rpcAttribute;
        }
    }
}