using ClientSocketIO.Handlers;
using TMPro;
using UnityEngine;

public class PingText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void OnEnable()
    {
        PingHandler.OnGetPing.AddListener(OnGetPing);
    }

    private void OnGetPing(int milliseconds)
    {
        text.text = $"Ping: {milliseconds}";
    }

    private void OnDisable()
    {
        PingHandler.OnGetPing.RemoveListener(OnGetPing);
    }
}