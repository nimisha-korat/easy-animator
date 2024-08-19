
using UnityEngine;

namespace EasyAnimator
{
    
    public partial class CustomFade
    {
         

        public static void Apply(EasyAnimatorComponent EasyAnimator, AnimationCurve curve)
            => Apply(EasyAnimator.States.Current, curve);

        public static void Apply(EasyAnimatorPlayable EasyAnimator, AnimationCurve curve)
            => Apply(EasyAnimator.States.Current, curve);

        public static void Apply(EasyAnimatorState state, AnimationCurve curve)
            => Curve.Acquire(curve).Apply(state);

        public static void Apply(EasyAnimatorNode node, AnimationCurve curve)
            => Curve.Acquire(curve).Apply(node);

         

        private sealed class Curve : CustomFade
        {
             

            private AnimationCurve _Curve;

             

            public static Curve Acquire(AnimationCurve curve)
            {
                if (curve == null)
                {
                    OptionalWarning.CustomFadeNotNull.Log($"{nameof(curve)} is null.");
                    return null;
                }

                var fade = ObjectPool<Curve>.Acquire();
                fade._Curve = curve;
                return fade;
            }

             

            protected override float CalculateWeight(float progress) => _Curve.Evaluate(progress);

             

            protected override void Release() => ObjectPool<Curve>.Release(this);

             
        }
    }
}
