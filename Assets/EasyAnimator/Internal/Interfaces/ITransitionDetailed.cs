

using UnityEngine;

namespace EasyAnimator
{
    
    public interface ITransitionDetailed : ITransition
    {
         

        bool IsValid { get; }

        bool IsLooping { get; }

        float NormalizedStartTime { get; set; }

        float MaximumDuration { get; }

        float Speed { get; set; }

         
    }

    public static partial class EasyAnimatorUtilities
    {

        public static bool IsValid(this ITransition transition)
        {
            if (transition == null)
                return false;

            if (TryGetWrappedObject(transition, out ITransitionDetailed detailed))
                return detailed.IsValid;

            return true;
        }

         

       
        public static bool TryGetIsLooping(object motionOrTransition, out bool isLooping)
        {
            if (motionOrTransition is Motion motion)
            {
                isLooping = motion.isLooping;
                return true;
            }
            else if (TryGetWrappedObject(motionOrTransition, out ITransitionDetailed transition))
            {
                isLooping = transition.IsLooping;
                return true;
            }
            else
            {
                isLooping = false;
                return false;
            }
        }

         

        public static bool TryGetLength(object motionOrTransition, out float length)
        {
            if (motionOrTransition is AnimationClip clip)
            {
                length = clip.length;
                return true;
            }
            else if (TryGetWrappedObject(motionOrTransition, out ITransitionDetailed transition))
            {
                length = transition.MaximumDuration;
                return true;
            }
            else
            {
                length = 0;
                return false;
            }
        }

         
    }
}

