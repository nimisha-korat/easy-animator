

using UnityEngine.Animations;
using Unity.Collections;

namespace EasyAnimator
{
  
    public sealed class AnimatedBool : AnimatedProperty<AnimatedBool.Job, bool>
    {
         

        public AnimatedBool(IEasyAnimatorComponent EasyAnimator, int propertyCount,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            : base(EasyAnimator, propertyCount, options)
        { }

        public AnimatedBool(IEasyAnimatorComponent EasyAnimator, string propertyName)
            : base(EasyAnimator, propertyName)
        { }

        public AnimatedBool(IEasyAnimatorComponent EasyAnimator, params string[] propertyNames)
            : base(EasyAnimator, propertyNames)
        { }

         

        protected override void CreateJob()
        {
            _Job = new Job() { properties = _Properties, values = _Values };
        }

         

      
        public struct Job : IAnimationJob
        {
            public NativeArray<PropertyStreamHandle> properties;
            public NativeArray<bool> values;

            public void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream)
            {
                for (int i = properties.Length - 1; i >= 0; i--)
                    values[i] = properties[i].GetBool(stream);
            }
        }

         
    }
}
