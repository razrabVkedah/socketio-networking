namespace ClientSocketIO.NetworkData
{
    public enum SyncMode
    {
        /*
         * Synchronization of the variable will occur when the network-variable pipeline is executed
         */
        Calm,

        /*
         * Synchronizing the variable immediately
         */
        Forced
    }
}