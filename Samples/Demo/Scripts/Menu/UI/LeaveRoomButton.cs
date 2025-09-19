using ClientSocketIO.Rooms;
using UnityEngine;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour
{
    private Button _button;

    private void OnEnable()
    {
        _button = GetComponent<Button>();
        if(_button != null)
            _button.onClick.AddListener(OnClickLeaveButton);
    }

    private static void OnClickLeaveButton()
    {
        RoomsManager.LeaveRoom();
    }

    private void OnDisable()
    {
        if(_button != null)
            _button.onClick.RemoveListener(OnClickLeaveButton);
    }
}
