

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
     
    public static partial class EasyAnimatorUtilities
    {
         
        #region Misc
         

        public const bool IsEasyAnimatorPro = true;

         

        public static float Wrap01(float value)
        {
            var valueAsDouble = (double)value;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble));
            return value < 1 ? value : 0;
        }

        public static float Wrap(float value, float length)
        {
            var valueAsDouble = (double)value;
            var lengthAsDouble = (double)length;
            value = (float)(valueAsDouble - Math.Floor(valueAsDouble / lengthAsDouble) * lengthAsDouble);
            return value < length ? value : 0;
        }

         

        public static float Round(float value)
            => (float)Math.Round(value, MidpointRounding.AwayFromZero);

        public static float Round(float value, float multiple)
            => Round(value / multiple) * multiple;

         

        public static bool IsFinite(this float value) => !float.IsNaN(value) && !float.IsInfinity(value);

         

        public static string ToStringOrNull(object obj)
        {
            if (obj is null)
                return "Null";

            if (obj is Object unityObject && unityObject == null)
                return $"Null ({obj.GetType()})";

            return obj.ToString();
        }

         

        public static void Swap<T>(this T[] array, int a, int b)
        {
            var temp = array[a];
            array[a] = array[b];
            array[b] = temp;
        }

         

        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;

         

      
        public static bool SetLength<T>(ref T[] array, int length)
        {
            if (array == null || array.Length != length)
            {
                array = new T[length];
                return true;
            }
            else return false;
        }

         

        public static bool IsValid(this EasyAnimatorNode node) => node != null && node.IsValid;

        public static bool IsValid(this ITransitionDetailed transition) => transition != null && transition.IsValid;

         

        public static EasyAnimatorState CreateStateAndApply(this ITransition transition, EasyAnimatorPlayable root = null)
        {
            var state = transition.CreateState();
            state.SetRoot(root);
            transition.Apply(state);
            return state;
        }

         

        public static void RemovePlayable(Playable playable, bool destroy = true)
        {
            if (!playable.IsValid())
                return;

            Assert(playable.GetInputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 input.");
            Assert(playable.GetOutputCount() == 1,
                $"{nameof(RemovePlayable)} can only be used on playables with 1 output.");

            var input = playable.GetInput(0);
            if (!input.IsValid())
            {
                if (destroy)
                    playable.Destroy();
                return;
            }

            var graph = playable.GetGraph();
            var output = playable.GetOutput(0);

            if (output.IsValid())// Connected to another Playable.
            {
                if (destroy)
                {
                    playable.Destroy();
                }
                else
                {
                    Assert(output.GetInputCount() == 1,
                        $"{nameof(RemovePlayable)} can only be used on playables connected to a playable with 1 input.");
                    graph.Disconnect(output, 0);
                    graph.Disconnect(playable, 0);
                }

                graph.Connect(input, 0, output, 0);
            }
            else// Connected to the graph output.
            {
                Assert(graph.GetOutput(0).GetSourcePlayable().Equals(playable),
                    $"{nameof(RemovePlayable)} can only be used on playables connected to another playable or to the graph output.");

                if (destroy)
                    playable.Destroy();
                else
                    graph.Disconnect(playable, 0);

                graph.GetOutput(0).SetSourcePlayable(input);
            }
        }

         

       
        public static bool HasEvent(IAnimationClipCollection source, string functionName)
        {
            var clips = ObjectPool.AcquireSet<AnimationClip>();
            source.GatherAnimationClips(clips);

            foreach (var clip in clips)
            {
                if (HasEvent(clip, functionName))
                {
                    ObjectPool.Release(clips);
                    return true;
                }
            }

            ObjectPool.Release(clips);
            return false;
        }

        public static bool HasEvent(AnimationClip clip, string functionName)
        {
            var events = clip.events;
            for (int i = events.Length - 1; i >= 0; i--)
            {
                if (events[i].functionName == functionName)
                    return true;
            }

            return false;
        }

         

      
        public static void CalculateThresholdsFromAverageVelocityXZ(this MixerState<Vector2> mixer)
        {
            mixer.ValidateThresholdCount();

            for (int i = mixer.ChildCount - 1; i >= 0; i--)
            {
                var state = mixer.GetChild(i);
                if (state == null)
                    continue;

                var averageVelocity = state.AverageVelocity;
                mixer.SetThreshold(i, new Vector2(averageVelocity.x, averageVelocity.z));
            }
        }

         

        public static object GetParameterValue(Animator animator, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return animator.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return animator.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return animator.GetBool(parameter.nameHash);

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }
        public static object GetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    return playable.GetFloat(parameter.nameHash);

                case AnimatorControllerParameterType.Int:
                    return playable.GetInteger(parameter.nameHash);

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    return playable.GetBool(parameter.nameHash);

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

         

        public static void SetParameterValue(Animator animator, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        animator.SetTrigger(parameter.nameHash);
                    else
                        animator.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

        public static void SetParameterValue(AnimatorControllerPlayable playable, AnimatorControllerParameter parameter, object value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    playable.SetFloat(parameter.nameHash, (float)value);
                    break;

                case AnimatorControllerParameterType.Int:
                    playable.SetInteger(parameter.nameHash, (int)value);
                    break;

                case AnimatorControllerParameterType.Bool:
                    playable.SetBool(parameter.nameHash, (bool)value);
                    break;

                case AnimatorControllerParameterType.Trigger:
                    if ((bool)value)
                        playable.SetTrigger(parameter.nameHash);
                    else
                        playable.ResetTrigger(parameter.nameHash);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(AnimatorControllerParameterType)}: " + parameter.type);
            }
        }

         

      
        public static NativeArray<T> CreateNativeReference<T>() where T : struct
        {
            return new NativeArray<T>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

         
        #endregion
         
        #region Components
         

        
        public static T AddEasyAnimatorComponent<T>(this Animator animator) where T : Component, IEasyAnimatorComponent
        {
            var EasyAnimator = animator.gameObject.AddComponent<T>();
            EasyAnimator.Animator = animator;
            return EasyAnimator;
        }

         

        
        public static T GetOrAddEasyAnimatorComponent<T>(this Animator animator) where T : Component, IEasyAnimatorComponent
        {
            var EasyAnimator = animator.GetComponent<T>();
            if (EasyAnimator != null)
                return EasyAnimator;
            else
                return animator.AddEasyAnimatorComponent<T>();
        }

         

        public static T GetComponentInParentOrChildren<T>(this GameObject gameObject) where T : class
        {
            var component = gameObject.GetComponentInParent<T>();
            if (component != null)
                return component;

            return gameObject.GetComponentInChildren<T>();
        }

      
        public static bool GetComponentInParentOrChildren<T>(this GameObject gameObject, ref T component) where T : class
        {
            if (component != null &&
                (!(component is Object obj) || obj != null))
                return false;

            component = gameObject.GetComponentInParentOrChildren<T>();
            return !(component is null);
        }

         
        #endregion
         
        #region Editor
         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Assert(bool condition, object message)
        {
#if UNITY_ASSERTIONS
            if (!condition)
                throw new UnityEngine.Assertions.AssertionException(message != null ? message.ToString() : "Assertion failed.", null);
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

         

      
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModeSampleAnimation(this AnimationClip clip, Component component, float time = 0)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            var gameObject = component.gameObject;
            component = gameObject.GetComponentInParentOrChildren<Animator>();
            if (component == null)
            {
                component = gameObject.GetComponentInParentOrChildren<Animation>();
                if (component == null)
                    return;
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (!ShouldEditModeSample(clip, component))
                    return;

                clip.SampleAnimation(component.gameObject, time);
            };
        }

        private static bool ShouldEditModeSample(AnimationClip clip, Component component)
        {
            return
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
                clip != null &&
                component != null &&
                !UnityEditor.EditorUtility.IsPersistent(component);
#endif
        }

         

       
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void EditModePlay(this AnimationClip clip, Component component)
        {
#if UNITY_EDITOR
            if (!ShouldEditModeSample(clip, component))
                return;

            var EasyAnimator = component as IEasyAnimatorComponent;
            if (EasyAnimator == null)
                EasyAnimator = component.gameObject.GetComponentInParentOrChildren<IEasyAnimatorComponent>();

            if (!ShouldEditModePlay(EasyAnimator, clip))
                return;

            // If it's already initialized, play immediately.
            if (EasyAnimator.IsPlayableInitialized)
            {
                EasyAnimator.Playable.Play(clip);
                return;
            }

            // Otherwise, delay it in case this was called at a bad time (such as during OnValidate).
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (ShouldEditModePlay(EasyAnimator, clip))
                    EasyAnimator.Playable.Play(clip);
            };
        }

        private static bool ShouldEditModePlay(IEasyAnimatorComponent EasyAnimator, AnimationClip clip)
        {
            return
                ShouldEditModeSample(clip, EasyAnimator?.Animator) &&
                (!(EasyAnimator is Object obj) || obj != null);
#endif
        }

         
#if UNITY_ASSERTIONS
         

        private static System.Reflection.FieldInfo _DelegatesField;
        private static bool _GotDelegatesField;

      
        public static bool TryGetInvocationListNonAlloc(MulticastDelegate multicast, out Delegate[] delegates)
        {
            if (multicast == null)
            {
                delegates = null;
                return false;
            }

            if (!_GotDelegatesField)
            {
                const string FieldName = "delegates";

                _GotDelegatesField = true;
                _DelegatesField = typeof(MulticastDelegate).GetField("delegates",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (_DelegatesField != null && _DelegatesField.FieldType != typeof(Delegate[]))
                    _DelegatesField = null;

                if (_DelegatesField == null)
                    Debug.LogError($"Unable to find {nameof(MulticastDelegate)}.{FieldName} field.");
            }

            if (_DelegatesField == null)
            {
                delegates = null;
                return false;
            }
            else
            {
                delegates = (Delegate[])_DelegatesField.GetValue(multicast);
                return true;
            }
        }

         
#endif
         
        #endregion
         
    }
}

