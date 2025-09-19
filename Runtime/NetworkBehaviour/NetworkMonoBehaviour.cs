using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClientSocketIO.Handlers;
using ClientSocketIO.NetworkBehaviour.Attributes;
using ClientSocketIO.NetworkComponents.Help;
using ClientSocketIO.NetworkData;
using ClientSocketIO.NetworkData.NetworkRpc;
using ClientSocketIO.NetworkData.NetworkRpc.Json;
using ClientSocketIO.NetworkData.NetworkVariables;
using ClientSocketIO.Types.NetworkUpdate;
using Newtonsoft.Json;
using UnityEngine;

namespace ClientSocketIO.NetworkBehaviour
{
    public class NetworkMonoBehaviour : MonoBehaviour
    {
        [Tooltip(
            "Should this NetworkMonoBehaviour attempt to synchronize with the same objects on other clients when starting the scene?\n" +
            "Synchronization is performed using the name of the object, therefore, it is impossible to allow the existence of " +
            "several NetworkMonoBehaviours on the same stage with the same names.")]
        [SerializeField]
        private bool isStationary;

        [SerializeField] private int NetworkMonoBehaviourId;
        public int GetNetworkMonoBehaviourId => NetworkMonoBehaviourId;

        public void SetNetworkMonoBehaviourId(int id)
        {
            NetworkMonoBehaviourId = id;
            foreach (var networkVariablesKey in _networkVariables.Keys)
            {
                _networkVariables[networkVariablesKey].NetworkMonoBehaviourId = NetworkMonoBehaviourId;
            }
        }

        protected bool IsHost => Client.IsHost;

        protected virtual void Awake()
        {
            if (isStationary)
            {
            }

            RegisterAllRPCMethods();
            RegisterAllNetworkVariables();
            NetworkMonoBehavioursManager.AddNetworkMonoBehaviour(this);
            NetworkUpdateHandler.OnSendDataToServer.AddListener(OnSendDataToServerBase);
        }

        protected virtual void OnDestroy()
        {
            NetworkMonoBehavioursManager.RemoveNetworkMonoBehaviour(this);
            NetworkUpdateHandler.OnSendDataToServer.RemoveListener(OnSendDataToServerBase);
        }

        #region VariablesInterpolation

        private void Update()
        {
            foreach (var value in _networkVariables.Values)
            {
                if ((Client.IsHost == true && value.AuthorityMode == AuthorityMode.HostToClient) ||
                    (Client.IsHost == false && value.AuthorityMode == AuthorityMode.ClientToHost))
                    break;
                if (value.InterpolationType is not InterpolationType.None && value.SyncMode is SyncMode.Calm)
                {
                    value.InterpolationUpdate();
                }
            }

            Update2();
        }

        protected virtual void Update2()
        {
        }

        private void OnSendDataToServerBase()
        {
            foreach (var networkVariablesValue in _networkVariables.Values.Where(networkVariablesValue =>
                         networkVariablesValue.InterpolationType is not InterpolationType.None))
            {
                networkVariablesValue.OnSendDataToServer();
            }

            OnSendDataToServer();
        }

        /// <summary>
        /// This method invokes when NetworkUpdateHandler sends data to server.
        /// You can use that method for resetting your data-to-send
        /// </summary>
        protected virtual void OnSendDataToServer()
        {
        }

        #endregion

        #region Variables

        private readonly Dictionary<string, NetworkVariableBase> _networkVariables = new();

        private void RegisterAllNetworkVariables()
        {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var fieldName = field.Name;
                var value = field.GetValue(this);

                if (fieldType.IsGenericType == true)
                {
                    if (field.FieldType.GetGenericTypeDefinition() == typeof(NetworkVariable<>))
                    {
                        var genericType = fieldType.GetGenericArguments()[0];
                        RegisterNetworkVariable(fieldName, value, genericType);
                    }
                }
                else
                {
                    if (fieldType.BaseType == null || fieldType.BaseType.IsGenericType == false) continue;
                    if (fieldType.BaseType.GetGenericTypeDefinition() == typeof(NetworkVariable<>))
                    {
                        var genericType = fieldType.BaseType.GetGenericArguments()[0];
                        RegisterNetworkVariable(fieldName, value, genericType);
                    }
                }
            }
        }

        private void RegisterNetworkVariable(string fieldName, object value, Type genericType)
        {
            var method = GetType().GetMethod(nameof(RegisterNetworkVariableHelper));
            if (method == null)
            {
                Debug.Log($"Method is null! {fieldName} {genericType}");
                return;
            }

            var genericMethod = method.MakeGenericMethod(genericType);
            genericMethod.Invoke(this, new[] { fieldName, value });
        }

        // ReSharper disable once UnusedMember.Local
        public void RegisterNetworkVariableHelper<T>(string vName, NetworkVariable<T> variable)
        {
            _networkVariables[vName] = variable;
            variable.NetworkMonoBehaviourId = NetworkMonoBehaviourId;
            variable.VariableIdentifierName = vName;
            variable.VariableInit();
            variable.OnValueChanged += OnNetworkVariableChanged;
        }

        private void OnNetworkVariableChanged<T>(NetworkVariable<T> variable)
        {
            if ((Client.IsHost == true && variable.AuthorityMode == AuthorityMode.ClientToHost) ||
                (Client.IsHost == false && variable.AuthorityMode == AuthorityMode.HostToClient))
                return;

            switch (variable.SyncMode)
            {
                case SyncMode.Calm:
                    NetworkUpdateHandler.AddDataToSend(variable.ToNetworkVariableData());
                    break;
                case SyncMode.Forced:
                    var serializedValue = variable.ToJson();
                    NetworkVariableHandler.SendVariablesToServer(serializedValue);
                    break;
                default:
                    Debug.LogError("Network variable " + variable.VariableIdentifierName + " sync mode is " +
                                   variable.SyncMode + "!");
                    break;
            }
        }

        public void ReceiveVariableFromServer(NetworkVariableData data)
        {
            if (_networkVariables.TryGetValue(data.variableName, out var variable))
            {
                if ((Client.IsHost == true && variable.AuthorityMode == AuthorityMode.HostToClient) ||
                    (Client.IsHost == false && variable.AuthorityMode == AuthorityMode.ClientToHost))
                    return;
                variable.FromJson(data.variableDataJson, data.serverTime, data.beginChangesServerTime);
            }
            else
            {
                Debug.Log("Can't find your variable " + data.variableName);
            }
        }

        #endregion

        #region RPCs

        private readonly Dictionary<string, RpcMethodInfo> _rpcMethods = new();

        private void RegisterAllRPCMethods()
        {
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var rpcAttribute = method.GetCustomAttribute<RPCAttribute>();
                if (rpcAttribute != null)
                {
                    _rpcMethods[method.Name] = new RpcMethodInfo(method, rpcAttribute);
                }
            }
        }

        protected void InvokeRPC(Action methodDelegate, params object[] parameters)
        {
            var methodInfo = methodDelegate.Method;
            var methodName = methodInfo.Name;

            if (_rpcMethods.TryGetValue(methodName, out var method) == false) return;

            if (method.RPCAttribute.InvokeHereToo == true)
                method.MethodInfo.Invoke(this, parameters);
            var rpcData = new NetworkRpcData
            {
                networkMonoBehaviourId = NetworkMonoBehaviourId,
                RpcMethodName = methodName,
                MethodParams =
                    JsonConvert.SerializeObject(parameters, JsonConverterSettingsRpc.serializerSettings)
            };
            switch (method.RPCAttribute.SyncMode)
            {
                case SyncMode.Calm:
                    NetworkUpdateHandler.AddDataToSend(rpcData);
                    break;
                case SyncMode.Forced:
                    NetworkRpcHandler.SendRpcToServer(rpcData.ToJson());
                    break;
                default:
                    Debug.LogError("RPC " + methodName + " sync mode is " +
                                   method.RPCAttribute.SyncMode + "!");
                    break;
            }
        }

        public void RemoteProcedureCall(string methodName, params object[] parameters)
        {
            if (_rpcMethods.TryGetValue(methodName, out var method))
            {
                if ((Client.IsHost == true && method.RPCAttribute.Authority == AuthorityMode.HostToClient) ||
                    (Client.IsHost == false && method.RPCAttribute.Authority == AuthorityMode.ClientToHost)) return;

                method.MethodInfo.Invoke(this, parameters);
            }
            else
            {
                Debug.LogError($"Method {methodName} not found.");
            }
        }

        #endregion

        #region Components

        public virtual void SetDataFromServer(BaseNetworkData data)
        {
        }

        #endregion
    }
}