using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkComponents.Help;
using UnityEngine;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    [System.Serializable]
    public class NetworkVariableVector3Int : NetworkVariable<Vector3Int>
    {
        public NetworkVariableVector3Int(Vector3Int initialValue, AuthorityMode authorityMode = AuthorityMode.Both,
            SyncMode syncMode = SyncMode.Calm, InterpolationType interpolationType = InterpolationType.None) : base(
            initialValue, authorityMode, syncMode, interpolationType)
        {
        }

        public NetworkVariableVector3Int() : base(new Vector3Int())
        {
        }

        public override void InterpolationUpdate()
        {
            switch (States.Count)
            {
                case 0:
                    return;
                case 1:
                    _variable = States[0].Value;
                    return;
            }

            var interpolationTime = NetworkUpdateHandler.GetInterpolationTime();

            for (var i = 0; i < States.Count - 1; i++)
            {
                if (States[i].Time > interpolationTime || States[i + 1].Time <= interpolationTime) continue;
                if (States[i].Time > States[i + 1].BeginChangesTime)
                {
                    Debug.LogWarning(
                        $"BeginChangesServerTime is greater! {States[i].Time} {States[i + 1].BeginChangesTime} {VariableIdentifierName}");
                }

                var t = (interpolationTime - States[i + 1].BeginChangesTime) /
                        (States[i + 1].Time - States[i + 1].BeginChangesTime);

                var v = Vector3.Lerp(States[i].Value, States[i + 1].Value, t);
                _variable = new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
                OnGetValueFromServer.Invoke(_variable);
                UsedLast = false;
                return;
            }

            UseLastState();
        }
    }
}