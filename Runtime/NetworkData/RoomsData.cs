// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
namespace ClientSocketIO.NetworkData
{
    public class RoomData
    {
        public int id;
        public string name;
        public int ping;
        public int clientsCount;
        public int maxClientsCount;
    }
    public class RoomsData
    {
        // ReSharper disable once UnassignedField.Global
        public RoomData[] rooms;
    }
}