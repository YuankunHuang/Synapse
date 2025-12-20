using Synapse.Shared.Protocol;
using UnityEngine;

namespace Synapse.Client.Core
{
    public static class Mapping
    {
        public static Vec3 ToVec3(this Vector3 vec3)
        {
            return new Vec3()
            {
                X = vec3.x, Y = vec3.y, Z = vec3.z,
            };
        }

        public static Vector3 ToVector3(this Vec3 vec3)
        {
            return new Vector3(vec3.X, vec3.Y, vec3.Z);
        }
    }    
}