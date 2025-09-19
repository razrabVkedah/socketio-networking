using System.Collections.Generic;
using ClientSocketIO.NetworkData;
using ClientSocketIO.Rooms;
using UnityEngine;

public class RoomsView : MonoBehaviour
{
    [SerializeField] private RectTransform roomsParent;
    [SerializeField] private RoomUIElement roomPrefab;
    [SerializeField] private List<RoomUIElement> spawned = new();
    
    private void OnEnable()
    {
        RoomsManager.OnGetAllRooms.AddListener(OnGetRoomsData);
    }

    private void OnDisable()
    {
        RoomsManager.OnGetAllRooms.RemoveListener(OnGetRoomsData);
    }

    private void OnGetRoomsData(RoomsData roomsData)
    {
        var toSpawnCount = roomsData.rooms.Length - spawned.Count;
        for(var i = 0; i < toSpawnCount;i++)
            spawned.Add(Instantiate(roomPrefab, roomsParent));
        for (var i = 0; i < spawned.Count; i++)
        {
            if (i < roomsData.rooms.Length)
            {
                spawned[i].gameObject.SetActive(true);
                spawned[i].SetElement(roomsData.rooms[i]);
            }
            else
            {
                spawned[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void GetAllRooms()
    {
        RoomsManager.GerAllRoomsForced();
    }

    public void CreateRoom()
    {
        RoomsManager.CreateRoom();
    }
}
