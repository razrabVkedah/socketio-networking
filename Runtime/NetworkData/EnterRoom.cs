// ReSharper disable InconsistentNaming
namespace ClientSocketIO.NetworkData
{
    public class EnterRoom
    {
        public int roomId;
        public string roomName;
        public bool isHost;
        public int tickRate;
        public int hostTime;
    }
}