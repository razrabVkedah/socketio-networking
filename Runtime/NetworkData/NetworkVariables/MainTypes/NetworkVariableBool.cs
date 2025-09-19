using ClientSocketIO.NetworkComponents.Help;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [System.Serializable]
    public class NetworkVariableBool : NetworkVariable<bool>
    {
        public NetworkVariableBool(bool initialValue, AuthorityMode authorityMode = AuthorityMode.Both,
            SyncMode syncMode = SyncMode.Calm) : base(initialValue, authorityMode, syncMode)
        {
            InterpolationType = InterpolationType.None;
        }
        
        public NetworkVariableBool() : base(false)
        {
        }
    }
}