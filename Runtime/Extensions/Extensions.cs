using ClientSocketIO.NetworkData;
using UnityEngine;

public static class Extensions
{
    public static Vector3Sync ToVector3Sync(this Vector3 vector3)
    {
        return new Vector3Sync { x = vector3.x, y = vector3.y, z = vector3.z };
    }

    public static Vector2Sync ToVector2Sync(this Vector2 vector2)
    {
        return new Vector2Sync { x = vector2.x, y = vector2.y };
    }

    public static QuaternionSync ToQuaternionSync(this Quaternion quaternion)
    {
        return new QuaternionSync { x = quaternion.x, y = quaternion.y, z = quaternion.z, w = quaternion.w };
    }
}