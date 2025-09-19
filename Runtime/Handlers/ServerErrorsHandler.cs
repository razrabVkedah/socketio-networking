using ClientSocketIO.NetworkData;
using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public class ServerErrorsHandler : Singleton<ServerErrorsHandler>
    {
        protected override void OnStartInstance()
        {
            Client.ClientAddListener<ServerError>(NetworkEventTypes.ServerError, OnGetServerError);
        }

        protected override void OnDestroyInstance()
        {
            Client.ClientRemoveListener<ServerError>(NetworkEventTypes.ServerError, OnGetServerError);
        }

        private static void OnGetServerError(ServerError data)
        {
            Debug.Log("Server error");
        }
    }
}