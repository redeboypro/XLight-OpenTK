using Assimp;
using OpenTK;

namespace XLight_OpenTK
{
    public static class Common
    {
        public static Vector3 ToOpenTK(this Vector3D vector) {
            Vector3 tmpVector;
            tmpVector.X = vector.X;
            tmpVector.Y = vector.Y;
            tmpVector.Z = vector.Z;
            return tmpVector;
        }
    }
}