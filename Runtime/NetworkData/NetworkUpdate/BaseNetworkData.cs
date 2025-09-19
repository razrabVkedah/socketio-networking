// ReSharper disable InconsistentNaming
namespace ClientSocketIO.Types.NetworkUpdate
{
    public class BaseNetworkData
    {
        public NetworkDataType dataType { get; set; }
        public int networkMonoBehaviourId;
        public float serverTime;
    }
}