using System;
using System.Collections.Generic;
using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkComponents.Help;
using ClientSocketIO.NetworkData.NetworkVariables.Help;
using UnityEngine;
using UnityEngine.Events;

namespace ClientSocketIO.NetworkData.NetworkVariables
{
    public class NetworkVariable<T> : NetworkVariableBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public T _variable;

        public T Value
        {
            get => _variable;
            set
            {
                if (_variable.Equals(value)) return;
                _variable = value;
                if (BeginChangeServerTime < 0)
                    BeginChangeServerTime = NetworkUpdateHandler.GetServerTime();
                OnValueChanged?.Invoke(this);
            }
        }

        public event Action<NetworkVariable<T>> OnValueChanged;
        public readonly UnityEvent<T> OnGetValueFromServer = new();
        protected bool UsedLast;

        protected NetworkVariable(T initialValue, AuthorityMode authorityMode = AuthorityMode.Both,
            SyncMode syncMode = SyncMode.Calm, InterpolationType interpolationType = InterpolationType.None)
        {
            _variable = initialValue;
            AuthorityMode = authorityMode;
            SyncMode = syncMode;
            InterpolationType = interpolationType;
        }

        public override void VariableInit()
        {
            var interpolationTime = NetworkUpdateHandler.GetInterpolationTime();
            States = new List<InterpolationState<T>>
                { new(_variable, interpolationTime, interpolationTime) };
        }

        public override string ToJson()
        {
            return JsonUtility.ToJson(ToNetworkVariableData());
        }

        public override NetworkVariableData ToNetworkVariableData()
        {
            var v = new Wrapper<T>(_variable);
            var data = JsonUtility.ToJson(v);
            var nData = new NetworkVariableData(NetworkMonoBehaviourId, VariableIdentifierName, data)
            {
                serverTime = NetworkUpdateHandler.GetServerTime()
            };
            if (InterpolationType is InterpolationType.Interpolate)
                nData.beginChangesServerTime = BeginChangeServerTime;

            return nData;
        }

        protected List<InterpolationState<T>> States;

        private void ClearOldStates()
        {
            while (States.Count > 2)
            {
                if (NetworkUpdateHandler.GetInterpolationTime() - States[0].Time <
                    NetworkUpdateHandler.InterpolationStateDefaultLifetime) break;

                States.RemoveAt(0);
            }
        }

        public override void FromJson(string variableDataJson, float serverTime, float? beginChangesServerTime)
        {
            var data = JsonUtility.FromJson<Wrapper<T>>(variableDataJson);

            if (SyncMode is SyncMode.Forced || InterpolationType is InterpolationType.None)
            {
                _variable = data.value;
                OnGetValueFromServer.Invoke(_variable);
                return;
            }

            if (beginChangesServerTime.HasValue == false)
            {
                Debug.LogError(
                    $"NetworkVariable using interpolation, but input beginChangesServerTime parameter is empty {VariableIdentifierName}");
                return;
            }

            var state = new InterpolationState<T>(data.value, beginChangesServerTime.Value, serverTime);
            ClearOldStates();
            States.Add(state);
            UsedLast = false;
        }

        protected void UseLastState()
        {
            _variable = States[^1].Value;

            if (UsedLast == true) return;
            OnGetValueFromServer.Invoke(_variable);
            UsedLast = true;
        }
    }
}