using System;
using ClientSocketIO.NetworkData;

namespace ClientSocketIO.NetworkBehaviour.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RPCAttribute : Attribute
    {
        public AuthorityMode Authority { get; }
        public SyncMode SyncMode { get; }
        public readonly bool InvokeHereToo = false;

        public RPCAttribute(AuthorityMode authority = AuthorityMode.Both, SyncMode syncMode = SyncMode.Calm)
        {
            Authority = authority;
        }
    }
}