using System;
using UnityEngine;
using UnityEngine.Events;

namespace ClientSocketIO.Chat
{
    public class ChatController : MonoBehaviour
    {
        public UnityEvent<ChatMessageData> onGetMessage = new();
        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }
    }
}