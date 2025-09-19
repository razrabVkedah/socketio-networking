namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [System.Serializable]
    public class NetworkVariableString : NetworkVariable<string>
    {
        public NetworkVariableString(string initialValue, AuthorityMode authorityMode = AuthorityMode.Both,
            SyncMode syncMode = SyncMode.Calm) : base(initialValue, authorityMode, syncMode)
        {
        }
        
        public NetworkVariableString() : base("")
        {
        }
    }
}