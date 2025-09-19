using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientSocketIO.NetworkBehaviour
{
    public class NetworkMonoBehavioursManager : MonoBehaviour
    {
        #region Global

        private static readonly List<NetworkMonoBehaviour> NetworkMonoBehaviours = new();

        public static void AddNetworkMonoBehaviour(NetworkMonoBehaviour networkMonoBehaviour)
        {
            NetworkMonoBehaviours.Add(networkMonoBehaviour);
        }

        public static void RemoveNetworkMonoBehaviour(NetworkMonoBehaviour networkMonoBehaviour)
        {
            NetworkMonoBehaviours.Remove(networkMonoBehaviour);
        }

        public static NetworkMonoBehaviour GetNetworkMonoBehaviour(int id)
        {
            var first = NetworkMonoBehaviours.FirstOrDefault(behaviour => behaviour.GetNetworkMonoBehaviourId == id);
            if (first != null)
                return first;

            Debug.LogWarning($"You are trying to find NetworkMonoBehaviour with id {id}, " +
                             "but it doesn't exist on current scene!");
            return null;
        }

        public static int GetFreeNetworkMonoBehaviourId()
        {
            // 1 - spawner
            var id = Random.Range(101, 10000000);
            while (NetworkMonoBehaviours.FirstOrDefault(n => n.GetNetworkMonoBehaviourId == id) != null)
            {
                id = Random.Range(101, 10000000);
            }

            return id;
        }

        #endregion

        #region StationaryNetworkMonoBehaviours

        [SerializeField] private NetworkMonoBehaviour[] stationary;

        #endregion
    }
}