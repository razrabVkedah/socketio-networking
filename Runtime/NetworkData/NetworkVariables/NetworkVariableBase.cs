using ClientSocketIO.NetworkComponents.Help;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    public abstract class NetworkVariableBase
    {
        public SyncMode SyncMode;
        public AuthorityMode AuthorityMode;
        public int NetworkMonoBehaviourId;
        public string VariableIdentifierName;
        public InterpolationType InterpolationType;
        protected float BeginChangeServerTime = -1;

        public abstract string ToJson();

        public abstract NetworkVariableData ToNetworkVariableData();
        public abstract void FromJson(string variableDataJson, float serverTime, float? beginChangesServerTime);

        public abstract void VariableInit();

        public virtual void InterpolationUpdate()
        {
            
        }

        public virtual void OnSendDataToServer()
        {
            BeginChangeServerTime = -1;
        }
    }
}