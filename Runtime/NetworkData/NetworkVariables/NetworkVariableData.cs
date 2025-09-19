using ClientSocketIO.Types.NetworkUpdate;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [System.Serializable]
    public class NetworkVariableData : BaseNetworkData
    {
        public string variableName;
        public string variableDataJson;
        public float? beginChangesServerTime;

        public NetworkVariableData(int monoId, string vName, string dataJson)
        {
            networkMonoBehaviourId = monoId;
            variableName = vName;
            variableDataJson = dataJson;
            dataType = NetworkDataType.Variable;
        }

        public NetworkVariableData()
        {
            dataType = NetworkDataType.Variable;
        }
    }
}