namespace ClientSocketIO.Types.NetworkUpdate
{
    public class TransformData : BaseNetworkData
    {
        public float? PositionX;
        public float? PositionY;
        public float? PositionZ;
        public float? RotationX;
        public float? RotationY;
        public float? RotationZ;
        public float? ScaleX;
        public float? ScaleY;
        public float? ScaleZ;
        public float BeginChangesServerTime = -1f;
        
        public TransformData(int monoId)
        {
            networkMonoBehaviourId = monoId;
            dataType = NetworkDataType.Transform;
        }

        public TransformData()
        {
            dataType = NetworkDataType.Transform;
        }
    }
}