namespace ClientSocketIO.Handlers
{
    public static class NetworkEventTypes
    {
        public const string NetworkUpdate = "network-update";
        public const string NetworkRpc = "network-rpc";
        public const string NetworkVariable = "network-variable";
        public const string ServerError = "server-error";
        public const string Ping = "ping";
        public const string AllRooms = "all-rooms";
        public const string EnterRoom = "enter-room";
        public const string LeaveRoom = "leave-room";
        public const string UpdateRooms = "update-rooms";
        public const string CreateRoom = "create-room";
    }
}