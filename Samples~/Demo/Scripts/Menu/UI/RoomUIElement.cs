using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkData;
using ClientSocketIO.Rooms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIElement : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private Button connectButton;
    private RoomData _roomData;

    private void OnEnable()
    {
        connectButton.onClick.AddListener(OnClickConnectButton);
    }

    private void OnDisable()
    {
        connectButton.onClick.RemoveListener(OnClickConnectButton);
    }

    private void OnClickConnectButton()
    {
        RoomsManager.ConnectToRoom(_roomData.id);
    }

    public void SetElement(RoomData data)
    {
        _roomData = data;
        roomNameText.text = _roomData.name;
        pingText.text = "Ping: " + (_roomData.ping + PingHandler.LastCalculatedPing);
    } 
}