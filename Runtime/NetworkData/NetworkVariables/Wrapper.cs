namespace ClientSocketIO.NetworkData.NetworkVariables
{
    public class Wrapper<T>
    {
        // ReSharper disable once InconsistentNaming
        public T value;

        public Wrapper(T value)
        {
            this.value = value;
        }
    }
}