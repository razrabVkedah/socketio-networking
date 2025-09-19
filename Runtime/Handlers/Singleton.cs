using UnityEngine;

namespace ClientSocketIO.Handlers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object LockObject = new();
        private static bool _isShuttingDown;

        public static T Instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                     "' already destroyed. Returning null.");
                    return null;
                }

                lock (LockObject)
                {
                    if (_instance != null) return _instance;

                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                     "' wasn't initialized yet. Maybe you trying to get it from Awake()!" +
                                     "Trying to find instance on scene or create new instance-GameObject");

                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance != null) return _instance;
                    var singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T) + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (transform.root == transform)
                    DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this as T)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Start()
        {
            if (_instance == this) OnStartInstance();
        }

        protected virtual void OnStartInstance()
        {
        }

        protected virtual void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _isShuttingDown = true;
                OnDestroyInstance();
            }
        }

        protected virtual void OnDestroyInstance()
        {
        }
    }
}