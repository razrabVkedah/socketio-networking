using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ClientSocketIO.Rooms
{
    public class RoomsManager : Singleton<RoomsManager>
    {
        public static readonly UnityEvent<EnterRoom> OnEnterRoomEvent = new();
        public static readonly UnityEvent<RoomsData> OnGetAllRooms = new();
        public static readonly UnityEvent OnLeaveRoomEvent = new();

        protected override void OnStartInstance()
        {
            Client.ClientAddListener<RoomsData>(NetworkEventTypes.AllRooms, OnGetRoomsData);
            Client.ClientAddListener<EnterRoom>(NetworkEventTypes.EnterRoom, OnEnterRoom);
        }

        protected override void OnDestroyInstance()
        {
            Client.ClientRemoveListener<RoomsData>(NetworkEventTypes.AllRooms, OnGetRoomsData);
            Client.ClientRemoveListener<EnterRoom>(NetworkEventTypes.EnterRoom, OnEnterRoom);
        }

        public static void CreateRoom()
        {
            var data = new CreateRoom
            {
                roomName = "HelloWorld",
                time = (int)(Time.unscaledTime * 1000)
            };
            Client.ClientEmitForce(NetworkEventTypes.CreateRoom, JsonUtility.ToJson(data));
        }

        public static void ConnectToRoom(int roomId)
        {
            Client.ClientEmitForce(NetworkEventTypes.EnterRoom, roomId.ToString());
        }

        public static void LeaveRoom()
        {
            Client.ClientEmitForce(NetworkEventTypes.LeaveRoom);
        }

        private static void OnGetRoomsData(RoomsData data)
        {
            OnGetAllRooms.Invoke(data);
        }

        private static void OnEnterRoom(EnterRoom data)
        {
            OnEnterRoomEvent.Invoke(data);
            Client.IsHost = data.isHost;
            NetworkUpdateHandler.TickRate = data.tickRate;
            NetworkUpdateHandler.HostTimeOffset = data.hostTime - (int)(Time.unscaledTime * 1000);
            Debug.Log($"Entered room {data.roomId}. Is Host: {Client.IsHost}");
            Client.ClientAddListener(NetworkEventTypes.LeaveRoom, OnLeaveRoom);
            SceneManager.LoadScene(1);
        }

        private static void OnLeaveRoom()
        {
            Debug.Log("Leaved room");
            Client.ClientRemoveListener(NetworkEventTypes.LeaveRoom, OnLeaveRoom);
            OnLeaveRoomEvent.Invoke();
            SceneManager.LoadScene(0);
        }

        public static void GerAllRoomsForced()
        {
            Client.ClientEmitForce(NetworkEventTypes.UpdateRooms);
        }
    }
}