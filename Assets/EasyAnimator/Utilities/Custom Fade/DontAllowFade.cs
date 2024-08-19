
using UnityEngine;

namespace EasyAnimator
{
    public sealed class DontAllowFade : Key, IUpdatable
    {
         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Assert(EasyAnimatorPlayable EasyAnimator)
        {
#if UNITY_ASSERTIONS
            EasyAnimator.RequirePreUpdate(new DontAllowFade());
#endif
        }

         

        private static void Validate(EasyAnimatorNode node)
        {
            if (node != null && node.FadeSpeed != 0)
            {
#if UNITY_ASSERTIONS
                Debug.LogWarning($"The following {node.GetType().Name} is fading even though " +
                    $"{nameof(DontAllowFade)} is active: {node.GetDescription()}",
                    node.Root.Component as Object);
#endif

                node.Weight = node.TargetWeight;
            }
        }

         

        void IUpdatable.Update()
        {
            var layers = EasyAnimatorPlayable.Current.Layers;
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                Validate(layer);
                Validate(layer.CurrentState);
            }
        }

         
    }
}
