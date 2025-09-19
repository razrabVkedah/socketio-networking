using ClientSocketIO.Handlers;
using TMPro;
using UnityEngine;

public class ServerTimeHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Update()
    {
        text.text = ((int)(NetworkUpdateHandler.GetServerTime() * 1000)).ToString();
    }
}