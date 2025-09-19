using System.Collections.Generic;
using System.Linq;
using Best.SocketIO.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ClientSocketIO.Handlers
{
    public class PingHandler : Singleton<PingHandler>
    {
        public static readonly UnityEvent<int> OnGetPing = new();
        public static int LastCalculatedPing { get; private set; }
        private static readonly List<int> PingMeasurements = new();
        private const int MeasuresCount = 10;
        private float _lastSentTime;

        public static int AveragePingMilliseconds()
        {
            if (PingMeasurements.Count == 0) return 100;
            return (int)PingMeasurements.Average();
        }

        private static void AddPingMeasure(int milliseconds)
        {
            PingMeasurements.Add(milliseconds);
            if (PingMeasurements.Count >= MeasuresCount) PingMeasurements.RemoveAt(0);
        }

        protected override void OnStartInstance()
        {
            Client.OnConnectedEvent.AddListener(OnClientConnected);
            Client.OnDisconnectEvent.AddListener(OnClientDisconnected);
            Client.ClientAddListener(NetworkEventTypes.Ping, OnPing);
        }

        protected override void OnDestroyInstance()
        {
            Client.OnConnectedEvent.RemoveListener(OnClientConnected);
            Client.OnDisconnectEvent.RemoveListener(OnClientDisconnected);
        }

        private void OnPing()
        {
            Client.ClientEmitForce(NetworkEventTypes.Ping);
            var ping = Mathf.RoundToInt((Time.time - _lastSentTime) * 1000);
            AddPingMeasure(ping);
            LastCalculatedPing = ping;
            _lastSentTime = Time.time;
            OnGetPing.Invoke(ping);
        }

        private void OnClientConnected(ConnectResponse resp)
        {
        }

        private void OnClientDisconnected()
        {
        }
    }
}