using ClientSocketIO.NetworkData;
using ClientSocketIO.Rooms;
using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public abstract class InRoomHandlerBase<T> : Singleton<T> where T : MonoBehaviour
    {
        private bool _inRoom;
        
        protected bool IsInRoom() => _inRoom;

        private void OnEnable()
        {
            RoomsManager.OnEnterRoomEvent.AddListener(OnEnterRoomMiddleware);
            RoomsManager.OnLeaveRoomEvent.AddListener(OnLeaveRoomMiddleware);
        }

        private void OnDisable()
        {
            RoomsManager.OnEnterRoomEvent.RemoveListener(OnEnterRoomMiddleware);
            RoomsManager.OnLeaveRoomEvent.RemoveListener(OnLeaveRoomMiddleware);
        }

        private void OnEnterRoomMiddleware(EnterRoom _)
        {
            _inRoom = true;
            OnEnterRoom(_);
        }

        protected virtual void OnEnterRoom(EnterRoom _)
        {
        }

        private void OnLeaveRoomMiddleware()
        {
            _inRoom = false;
            OnLeaveRoom();
        }

        protected virtual void OnLeaveRoom()
        {
        }
    }
}