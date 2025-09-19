namespace ClientSocketIO.NetworkData
{
    /// <summary>
    /// Setting up the type of authoritarianism. Who can call the method remotely or change the values of variables
    /// </summary>
    public enum AuthorityMode
    {
        /// <summary>
        /// Only Host can change/invoke
        /// </summary>
        HostToClient,

        /// <summary>
        /// Only Client can change/invoke
        /// </summary>
        ClientToHost,

        /// <summary>
        /// Client and Host can change/invoke
        /// </summary>
        Both
    }
}