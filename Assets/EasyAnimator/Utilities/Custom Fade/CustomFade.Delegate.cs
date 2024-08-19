
using System;

namespace EasyAnimator
{
   
    public partial class CustomFade
    {
         

        public static void Apply(EasyAnimatorComponent EasyAnimator, Func<float, float> calculateWeight)
            => Apply(EasyAnimator.States.Current, calculateWeight);

        public static void Apply(EasyAnimatorPlayable EasyAnimator, Func<float, float> calculateWeight)
            => Apply(EasyAnimator.States.Current, calculateWeight);

        public static void Apply(EasyAnimatorState state, Func<float, float> calculateWeight)
            => Delegate.Acquire(calculateWeight).Apply(state);

        public static void Apply(EasyAnimatorNode node, Func<float, float> calculateWeight)
            => Delegate.Acquire(calculateWeight).Apply(node);

         

        public static void Apply(EasyAnimatorComponent EasyAnimator, Easing.Function function)
            => Apply(EasyAnimator.States.Current, function);

        public static void Apply(EasyAnimatorPlayable EasyAnimator, Easing.Function function)
            => Apply(EasyAnimator.States.Current, function);

        public static void Apply(EasyAnimatorState state, Easing.Function function)
            => Delegate.Acquire(function.GetDelegate()).Apply(state);

        public static void Apply(EasyAnimatorNode node, Easing.Function function)
            => Delegate.Acquire(function.GetDelegate()).Apply(node);

         

        private sealed class Delegate : CustomFade
        {
             

            private Func<float, float> _CalculateWeight;

             

            public static Delegate Acquire(Func<float, float> calculateWeight)
            {
                if (calculateWeight == null)
                {
                    OptionalWarning.CustomFadeNotNull.Log($"{nameof(calculateWeight)} is null.");
                    return null;
                }

                var fade = ObjectPool<Delegate>.Acquire();
                fade._CalculateWeight = calculateWeight;
                return fade;
            }

             

            protected override float CalculateWeight(float progress) => _CalculateWeight(progress);

             

            protected override void Release() => ObjectPool<Delegate>.Release(this);

             
        }
    }
}
