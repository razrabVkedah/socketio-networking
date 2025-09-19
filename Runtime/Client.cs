using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Best.SocketIO;
using Best.SocketIO.Events;
using ClientSocketIO.Authentication;
using ClientSocketIO.Handlers;
using ClientSocketIO.Listeners;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine.Events;

public class Client : Singleton<Client>
{
    #region ClientEvents

    public static readonly UnityEvent OnConnectingEvent = new();
    public static readonly UnityEvent<ConnectResponse> OnConnectedEvent = new();
    public static readonly UnityEvent<Error> OnErrorEvent = new();
    public static readonly UnityEvent OnDisconnectEvent = new();
    public static readonly UnityEvent OnReconnectingEvent = new();
    public static readonly UnityEvent OnReconnectEvent = new();

    #endregion

    #region PublicFields

    public static bool IsConnected { get; private set; }
    public static bool IsHost;

    #endregion

    private static SocketManager _manager;

    protected override void Awake()
    {
        base.Awake();

        if (IsConnected) return;
        ConnectSocketIO(0, new Uri(Config.MainServerUrl));
    }

    public void ConnectSocketIO(int clientId, Uri uri)
    {
        var options = new SocketOptions
        {
            AutoConnect = false,
            AdditionalQueryParams = new ObservableDictionary<string, string>
            {
                { "guid", AuthenticationManager.GetGuid() }
            }
        };

        _manager = new SocketManager(uri, options);

        _manager.Socket.On("connecting", () =>
        {
            Debug.Log("connecting");
            OnConnectingEvent.Invoke();
        });
        _manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        _manager.Socket.On("reconnecting", () =>
        {
            Debug.Log("reconnecting");
            OnReconnectingEvent.Invoke();
        });
        _manager.Socket.On("reconnect", () =>
        {
            Debug.Log("reconnect");
            OnReconnectEvent.Invoke();
        });
        _manager.Socket.On<Error>(SocketIOEventTypes.Error, OnError);
        _manager.Socket.On(SocketIOEventTypes.Disconnect, OnDisconnect);
        _manager.Open();
    }

    public void Disconnect()
    {
        _manager.Socket.Disconnect();
    }

    private void OnDisconnect()
    {
        IsConnected = false;
        Debug.Log("Disconnect");
        OnDisconnectEvent.Invoke();
    }

    private void OnError(Error error)
    {
        Debug.Log(error);
        OnErrorEvent.Invoke(error);
    }

    private void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Подключение по SocketIO успешно установлено");
        _manager.Socket.Emit("message", "msg 1");
        IsConnected = true;
        OnConnectedEvent.Invoke(resp);
    }

    public static void ClientEmitForce(string eventName, string emitParams)
    {
        _manager.Socket.Emit(eventName, emitParams);
    }

    public static void ClientEmitForce(string eventName)
    {
        _manager.Socket.Emit(eventName);
    }

    #region Listeners

    private static readonly List<IEventNameHandler> AllGenericListeners = new();
    private static readonly List<Listener> AllListeners = new();

    public static void ClientAddListener(string eventName, Action action)
    {
        var existing = AllListeners.FirstOrDefault(l =>
            string.Equals(l.EventName, eventName, StringComparison.CurrentCultureIgnoreCase));
        if (existing == null)
        {
            var listener = new Listener(eventName);
            listener.Actions.Add(action);
            _manager.Socket.On(eventName, listener.On);
            AllListeners.Add(listener);
        }
        else
        {
            existing.Actions.Add(action);
        }
    }

    public static void ClientAddListener<T>(string eventName, Action<T> action)
    {
        var existing = AllGenericListeners.FirstOrDefault(l =>
            string.Equals(l.GetEventName(), eventName, StringComparison.CurrentCultureIgnoreCase));
        if (existing == null)
        {
            var listener = new Listener<T>(eventName);
            listener.Actions.Add(action);
            _manager.Socket.On<T>(eventName, listener.On);
            AllGenericListeners.Add(listener);
        }
        else if (existing is not Listener<T> existingTyped)
        {
            Debug.LogError(
                $"Socket Event ({eventName}) already has listeners. Buy they have other input parameter type! " +
                $"You have to use {existing.GetImplementerType()}, but you're trying to add {typeof(T)}. " +
                "Your last try to add listener was ignored!!!");
        }
        else
        {
            existingTyped.Actions.Add(action);
        }
    }

    public static void ClientRemoveListener(string eventName, Action action)
    {
        var existing = AllListeners.FirstOrDefault(l =>
            string.Equals(l.EventName, eventName, StringComparison.CurrentCultureIgnoreCase));
        if (existing == null) return;

        existing.Actions.Remove(action);
        if (existing.Actions.Count > 0) return;

        AllListeners.Remove(existing);
        if (AllGenericListeners.FirstOrDefault(l =>
                string.Equals(l.GetEventName(), eventName, StringComparison.CurrentCultureIgnoreCase)) == null)
        {
            _manager?.Socket.Off(eventName);
        }
    }

    public static void ClientRemoveListener<T>(string eventName, Action<T> action)
    {
        var existing = AllGenericListeners.FirstOrDefault(l =>
            string.Equals(l.GetEventName(), eventName, StringComparison.CurrentCultureIgnoreCase));
        if (existing == null) return;

        if (existing is not Listener<T> existingTyped)
        {
            Debug.LogError(
                $"Socket Event ({eventName}) already has listeners. Buy they have other input parameter type! " +
                $"Type: {existing.GetImplementerType()}");
            return;
        }

        existingTyped.Actions.Remove(action);
        if (existingTyped.Actions.Count > 0) return;

        AllGenericListeners.Remove(existing);

        if (AllListeners.FirstOrDefault(l =>
                string.Equals(l.EventName, eventName, StringComparison.CurrentCultureIgnoreCase)) == null)
        {
            _manager?.Socket.Off(eventName);
        }
    }

    public static void ClientRemoveAllListeners(string eventName)
    {
        var existing = AllListeners.FirstOrDefault(l =>
            string.Equals(l.EventName, eventName, StringComparison.CurrentCultureIgnoreCase));
        AllListeners.Remove(existing);
        var existingGeneric = AllGenericListeners.FirstOrDefault(l =>
            string.Equals(l.GetEventName(), eventName, StringComparison.CurrentCultureIgnoreCase));
        AllGenericListeners.Remove(existingGeneric);

        _manager.Socket.Off(eventName);
    }

    #endregion Listeners

    protected override void OnDestroyInstance()
    {
        _manager?.Close();
        _manager = null;
    }
}