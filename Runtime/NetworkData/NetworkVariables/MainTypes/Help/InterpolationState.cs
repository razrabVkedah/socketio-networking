namespace ClientSocketIO.NetworkData.NetworkVariables.Help
{
    public class InterpolationState<T>
    {
        public readonly float BeginChangesTime;
        public readonly float Time;
        public readonly T Value;

        public InterpolationState(T value, float beginChangesTime, float t)
        {
            Value = value;
            BeginChangesTime = beginChangesTime;
            Time = t;
        }
    }
}