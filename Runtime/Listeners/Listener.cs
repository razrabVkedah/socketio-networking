using System;
using System.Collections.Generic;

namespace ClientSocketIO.Listeners
{
    public class Listener<T>: IEventNameHandler
    {
        public readonly string EventName;
        public readonly List<Action<T>> Actions;

        public void On(T data)
        {
            foreach (var action in Actions)
            {
                action(data);
            }
        }

        public Listener(string eventName)
        {
            EventName = eventName;
            Actions = new List<Action<T>>();
        }

        public string GetEventName()
        {
            return EventName;
        }

        public Type GetImplementerType()
        {
            return typeof(T);
        }
    }
    
    public class Listener
    {
        public readonly string EventName;
        public readonly List<Action> Actions;

        public void On()
        {
            var actionsCopy = Actions.ToArray();
            foreach (var action in actionsCopy)
            {
                action();
            }
        }

        public Listener(string eventName)
        {
            EventName = eventName;
            Actions = new List<Action>();
        }
    }
}