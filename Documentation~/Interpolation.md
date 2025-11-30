# Полный пример класса интерполяции

```C#
using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayer : MonoBehaviour {
    public class State {
        public Vector3 position;
        public Quaternion rotation;
        public float timestamp;
    }

    private Queue<State> stateBuffer = new Queue<State>();
    private float interpolationDelay = 0.1f; // 100 ms delay

    private void Update() {
        float renderTime = Time.time - interpolationDelay;

        // Удаляем старые состояния
        while (stateBuffer.Count >= 2 && stateBuffer.Peek().timestamp <= renderTime) {
            stateBuffer.Dequeue();
        }

        // Интерполируем между двумя последними состояниями
        if (stateBuffer.Count >= 2) {
            State start = stateBuffer.Dequeue();
            State end = stateBuffer.Peek();

            float t = (renderTime - start.timestamp) / (end.timestamp - start.timestamp);
            transform.position = Vector3.Lerp(start.position, end.position, t);
            transform.rotation = Quaternion.Slerp(start.rotation, end.rotation, t);
        }
    }

    public void OnReceiveState(State state) {
        stateBuffer.Enqueue(state);
    }

    // Example method for receiving data from the server
    private void OnEnable() {
        // Replace with your socket.io setup
        SocketIOComponent socket = GetComponent<SocketIOComponent>();
        socket.On("updateState", OnReceiveStateFromServer);
    }

    private void OnReceiveStateFromServer(SocketIOEvent evt) {
        State state = JsonUtility.FromJson<State>(evt.data.ToString());
        OnReceiveState(state);
    }
}
```

# Пример реализации мгновенного события

```C#
using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayer : MonoBehaviour {
    public class State {
        public Vector3 position;
        public Quaternion rotation;
        public float timestamp;
    }

    private Queue<State> stateBuffer = new Queue<State>();
    private float interpolationDelay = 0.1f; // 100 ms delay

    private void Update() {
        float renderTime = Time.time - interpolationDelay;

        // Удаляем старые состояния
        while (stateBuffer.Count >= 2 && stateBuffer.Peek().timestamp <= renderTime) {
            stateBuffer.Dequeue();
        }

        // Интерполируем между двумя последними состояниями
        if (stateBuffer.Count >= 2) {
            State start = stateBuffer.Dequeue();
            State end = stateBuffer.Peek();

            float t = (renderTime - start.timestamp) / (end.timestamp - start.timestamp);
            transform.position = Vector3.Lerp(start.position, end.position, t);
            transform.rotation = Quaternion.Slerp(start.rotation, end.rotation, t);
        }
    }

    public void OnReceiveState(State state) {
        stateBuffer.Enqueue(state);
    }

    // Example method for receiving data from the server
    private void OnEnable() {
        // Replace with your socket.io setup
        SocketIOComponent socket = GetComponent<SocketIOComponent>();
        socket.On("updateState", OnReceiveStateFromServer);
        socket.On("instantEvent", OnReceiveInstantEventFromServer);
    }

    private void OnReceiveStateFromServer(SocketIOEvent evt) {
        State state = JsonUtility.FromJson<State>(evt.data.ToString());
        OnReceiveState(state);
    }

    // Example of handling an instant event like a button press
    private void OnReceiveInstantEventFromServer(SocketIOEvent evt) {
        string eventType = evt.data["type"].str;
        if (eventType == "buttonPress") {
            HandleButtonPress(evt.data);
        }
    }

    private void HandleButtonPress(JSONObject data) {
        // Handle the button press event immediately
        // This might involve playing an animation, triggering a sound, etc.
        Debug.Log("Button pressed by another player");
    }

    // Method to send instant event to server
    public void SendInstantEvent(string eventType) {
        JSONObject eventData = new JSONObject();
        eventData.AddField("type", eventType);
        // Replace with your socket.io setup
        SocketIOComponent socket = GetComponent<SocketIOComponent>();
        socket.Emit("instantEvent", eventData);

        // Optionally, handle the event locally for immediate feedback
        if (eventType == "buttonPress") {
            HandleButtonPress(eventData);
        }
    }
}
```