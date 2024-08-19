

using UnityEngine;

namespace EasyAnimator
{
    
    public interface IMotion
    {
         

        float AverageAngularSpeed { get; }

        Vector3 AverageVelocity { get; }

         
    }


    public static partial class EasyAnimatorUtilities
    {
         

        public static bool TryGetAverageAngularSpeed(object motion, out float averageAngularSpeed)
        {
            if (motion is Motion unityMotion)
            {
                averageAngularSpeed = unityMotion.averageAngularSpeed;
                return true;
            }
            else if (TryGetWrappedObject(motion, out IMotion iMotion))
            {
                averageAngularSpeed = iMotion.AverageAngularSpeed;
                return true;
            }
            else
            {
                averageAngularSpeed = default;
                return false;
            }
        }

         

        public static bool TryGetAverageVelocity(object motion, out Vector3 averageVelocity)
        {
            if (motion is Motion unityMotion)
            {
                averageVelocity = unityMotion.averageSpeed;
                return true;
            }
            else if (TryGetWrappedObject(motion, out IMotion iMotion))
            {
                averageVelocity = iMotion.AverageVelocity;
                return true;
            }
            else
            {
                averageVelocity = default;
                return false;
            }
        }

         
    }
}

