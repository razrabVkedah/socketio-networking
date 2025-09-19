using System;
using UnityEngine;

namespace ClientSocketIO.Authentication
{
    public static class AuthenticationManager
    {
        private const string GuidPlayerPrefs = "GuidPlayerPrefs";

        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(GuidPlayerPrefs) == true)
                return PlayerPrefs.GetString(GuidPlayerPrefs, "EMPTY");

            var newGuid = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(GuidPlayerPrefs, newGuid);
            return PlayerPrefs.GetString(GuidPlayerPrefs, "EMPTY");
        }

        public static void UpdateGuid(string newGuid)
        {
            PlayerPrefs.SetString(GuidPlayerPrefs, newGuid);
        }
    }
}