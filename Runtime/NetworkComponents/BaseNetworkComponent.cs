using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkBehaviour;
using ClientSocketIO.NetworkData;
using ClientSocketIO.Types.NetworkUpdate;

namespace ClientSocketIO.NetworkComponents
{
    public class BaseNetworkComponent : NetworkMonoBehaviour
    {
        public AuthorityMode authorityMode;

        /// <summary>
        /// This method should return a complete list of data. Moreover, only those data
        /// that are specified for synchronization are returned, and NOT all local-component data
        /// </summary>
        public virtual void GetFullComponentData()
        {
        }

        public override void SetDataFromServer(BaseNetworkData data)
        {
            if ((Client.IsHost == true && authorityMode is AuthorityMode.HostToClient) ||
                (Client.IsHost == false && authorityMode is AuthorityMode.ClientToHost)) return;

            OnGetDataFromServer(data);
        }

        protected virtual void OnGetDataFromServer(BaseNetworkData data)
        {
        }
    }
}