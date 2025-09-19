using System;

namespace ClientSocketIO.Listeners
{
    public interface IEventNameHandler
    {
        public string GetEventName();

        public Type GetImplementerType();
    }
}