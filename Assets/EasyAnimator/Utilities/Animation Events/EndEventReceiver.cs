
using UnityEngine;

namespace EasyAnimator
{
  
    [AddComponentMenu(Strings.MenuPrefix + "End Event Receiver")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(EndEventReceiver))]
    public class EndEventReceiver : MonoBehaviour
    {
         

        [SerializeField]
        private EasyAnimatorComponent _EasyAnimator;

        public ref EasyAnimatorComponent EasyAnimator => ref _EasyAnimator;

         

        public static AnimationEvent CurrentEvent { get; private set; }

         

        private void End(AnimationEvent animationEvent)
        {
            End(_EasyAnimator, animationEvent);
        }

        public static bool End(EasyAnimatorComponent EasyAnimator, AnimationEvent animationEvent)
        {
            if (!EasyAnimator.IsPlayableInitialized)
            {
                // This could only happen if another Animator triggers the event on this object somehow.
                Debug.LogWarning($"{nameof(AnimationEvent)} '{nameof(End)}' was triggered by {animationEvent.animatorClipInfo.clip}" +
                    $", but the {nameof(EasyAnimatorComponent)}.{nameof(EasyAnimatorComponent.Playable)} hasn't been initialized.",
                    EasyAnimator);
                return false;
            }

            var layers = EasyAnimator.Layers;
            var count = layers.Count;

            // Try targeting the current state on each layer first.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i].CurrentState, animationEvent))
                    return true;
            }

            // Otherwise try every state.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i], animationEvent))
                    return true;
            }

            if (animationEvent.messageOptions == SendMessageOptions.RequireReceiver)
            {
                Debug.LogWarning($"{nameof(AnimationEvent)} '{nameof(End)}' was triggered by {animationEvent.animatorClipInfo.clip}" +
                    $", but no state was found with that {nameof(EasyAnimatorState.Key)}.",
                    EasyAnimator);
            }

            return false;
        }

         

        private static bool OnEndEventReceived(EasyAnimatorPlayable EasyAnimator, AnimationEvent animationEvent)
        {
            var layers = EasyAnimator.Layers;
            var count = layers.Count;

            // Try targeting the current state on each layer first.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i].CurrentState, animationEvent))
                    return true;
            }

            // Otherwise try every state.
            for (int i = 0; i < count; i++)
            {
                if (TryInvokeOnEndEventRecursive(layers[i], animationEvent))
                    return true;
            }

            return false;
        }

         

        private static bool TryInvokeOnEndEventRecursive(EasyAnimatorNode node, AnimationEvent animationEvent)
        {
            var childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = node.GetChild(i);
                if (child != null &&
                    (TryInvokeOnEndEvent(child, animationEvent) ||
                    TryInvokeOnEndEventRecursive(child, animationEvent)))
                    return true;
            }

            return false;
        }

         

        private static bool TryInvokeOnEndEvent(EasyAnimatorState state, AnimationEvent animationEvent)
        {
            if (state.Weight != animationEvent.animatorClipInfo.weight ||
                state.Clip != animationEvent.animatorClipInfo.clip ||
                !state.HasEvents)
                return false;

            var endEvent = state.Events.endEvent;
            if (endEvent.callback != null)
            {
                Debug.Assert(CurrentEvent == null, $"Recursive call to {nameof(TryInvokeOnEndEvent)} detected");

                try
                {
                    CurrentEvent = animationEvent;
                    endEvent.Invoke(state);
                }
                finally
                {
                    CurrentEvent = null;
                }
            }

            return true;
        }

         

        public static float GetFadeOutDuration(float minDuration)
        {
            if (CurrentEvent != null && CurrentEvent.floatParameter > 0)
                return CurrentEvent.floatParameter;

            return EasyAnimatorEvent.GetFadeOutDuration(minDuration);
        }

        public static float GetFadeOutDuration()
            => GetFadeOutDuration(EasyAnimatorPlayable.DefaultFadeDuration);

         

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            gameObject.GetComponentInParentOrChildren(ref _EasyAnimator);
        }
#endif

         
    }
}
