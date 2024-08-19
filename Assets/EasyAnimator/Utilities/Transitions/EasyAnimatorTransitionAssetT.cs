
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace EasyAnimator
{
   
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(EasyAnimatorTransitionAsset<ITransition>) + "_1")]
    public class EasyAnimatorTransitionAsset<TTransition> : ScriptableObject, ITransition, IWrapper, IAnimationClipSource
        where TTransition : ITransition
    {
         

        [SerializeReference]
        private TTransition _Transition;

        public ref TTransition Transition => ref _Transition;

        public virtual ITransition GetTransition() => _Transition;

        object IWrapper.WrappedObject => GetTransition();

         

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _Transition = Editor.TypeSelectionButton.CreateDefaultInstance<TTransition>();
        }
#endif

         

        public virtual bool IsValid => GetTransition().IsValid();

        public virtual float FadeDuration => GetTransition().FadeDuration;

        public virtual object Key => GetTransition().Key;

        public virtual FadeMode FadeMode => GetTransition().FadeMode;

        public virtual EasyAnimatorState CreateState() => GetTransition().CreateState();

        public virtual void Apply(EasyAnimatorState state)
        {
            GetTransition().Apply(state);
            state.SetDebugName(name);
        }

         

        public virtual void GetAnimationClips(List<AnimationClip> clips) => clips.GatherFromSource(GetTransition());

         
    }
}

 

#if UNITY_EDITOR
namespace EasyAnimator.Editor
{
    [CustomEditor(typeof(EasyAnimatorTransitionAsset<>), true), CanEditMultipleObjects]
    internal class EasyAnimatorTransitionAssetEditor : ScriptableObjectEditor
    {
         

        [MenuItem("CONTEXT/" + nameof(AnimatorState) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(BlendTree) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateTransition) + "/Generate Transition")]
        [MenuItem("CONTEXT/" + nameof(AnimatorStateMachine) + "/Generate Transitions")]
        private static void GenerateTransition(MenuCommand command)
        {
            var context = command.context;
            if (context is AnimatorState state)
            {
                Selection.activeObject = GenerateTransition(state);
            }
            else if (context is BlendTree blendTree)
            {
                Selection.activeObject = GenerateTransition(null, blendTree);
            }
            else if (context is AnimatorStateTransition transition)
            {
                Selection.activeObject = GenerateTransition(transition);
            }
            else if (context is AnimatorStateMachine stateMachine)// Layer or Sub-State Machine.
            {
                Selection.activeObject = GenerateTransitions(stateMachine);
            }
        }

         

        private static Object GenerateTransition(AnimatorState state)
            => GenerateTransition(state, state.motion);

         

        private static Object GenerateTransition(Object originalAsset, Motion motion)
        {
            if (motion is BlendTree blendTree)
            {
                return GenerateTransition(originalAsset as AnimatorState, blendTree);
            }
            else if (motion is AnimationClip || motion == null)
            {
                var asset = CreateInstance<ClipTransitionAsset>();
                asset.Transition = new ClipTransition
                {
                    Clip = (AnimationClip)motion,
                };

                GetDetailsFromState(originalAsset as AnimatorState, asset.Transition);
                SaveTransition(originalAsset, asset);
                return asset;
            }
            else
            {
                Debug.LogError($"Unsupported {nameof(Motion)} Type: {motion.GetType()}");
                return null;
            }
        }

         

        private static void GetDetailsFromState(AnimatorState state, ITransitionDetailed transition)
        {
            if (state == null ||
                transition == null)
                return;

            transition.Speed = state.speed;

            var isForwards = state.speed >= 0;
            var defaultEndTime = EasyAnimatorEvent.Sequence.GetDefaultNormalizedEndTime(state.speed);
            var endTime = defaultEndTime;

            var exitTransitions = state.transitions;
            for (int i = 0; i < exitTransitions.Length; i++)
            {
                var exitTransition = exitTransitions[i];
                if (exitTransition.hasExitTime)
                {
                    if (isForwards)
                    {
                        if (endTime > exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                    else
                    {
                        if (endTime < exitTransition.exitTime)
                            endTime = exitTransition.exitTime;
                    }
                }
            }

            if (endTime != defaultEndTime && EasyAnimatorUtilities.TryGetWrappedObject(transition, out IHasEvents events))
            {
                if (events.SerializedEvents == null)
                    events.SerializedEvents = new EasyAnimatorEvent.Sequence.Serializable();
                events.SerializedEvents.SetNormalizedEndTime(endTime);
            }
        }

         

        private static Object GenerateTransition(AnimatorState state, BlendTree blendTree)
        {
            var asset = CreateTransition(blendTree);
            if (asset == null)
                return null;

            if (state != null)
                asset.name = state.name;

            EasyAnimatorUtilities.TryGetWrappedObject(asset, out ITransitionDetailed detailed);
            GetDetailsFromState(state, detailed);
            SaveTransition(blendTree, asset);
            return asset;
        }

         

        private static Object GenerateTransition(AnimatorStateTransition transition)
        {
            Object EasyAnimatorTransition = null;

            if (transition.destinationStateMachine != null)
                EasyAnimatorTransition = GenerateTransitions(transition.destinationStateMachine);

            if (transition.destinationState != null)
                EasyAnimatorTransition = GenerateTransition(transition.destinationState);

            return EasyAnimatorTransition;
        }

         

        private static Object GenerateTransitions(AnimatorStateMachine stateMachine)
        {
            Object transition = null;

            foreach (var child in stateMachine.stateMachines)
                transition = GenerateTransitions(child.stateMachine);

            foreach (var child in stateMachine.states)
                transition = GenerateTransition(child.state);

            return transition;
        }

         

        private static Object CreateTransition(BlendTree blendTree)
        {
            switch (blendTree.blendType)
            {
                case BlendTreeType.Simple1D:
                    var linearAsset = CreateInstance<LinearMixerTransitionAsset>();
                    InitializeChildren(ref linearAsset.Transition, blendTree);
                    return linearAsset;

                case BlendTreeType.SimpleDirectional2D:
                case BlendTreeType.FreeformDirectional2D:
                    var directionalAsset = CreateInstance<MixerTransition2DAsset>();
                    directionalAsset.Transition = new MixerTransition2D
                    {
                        Type = MixerTransition2D.MixerType.Directional
                    };
                    InitializeChildren(ref directionalAsset.Transition, blendTree);
                    return directionalAsset;

                case BlendTreeType.FreeformCartesian2D:
                    var cartesianAsset = CreateInstance<MixerTransition2DAsset>();
                    cartesianAsset.Transition = new MixerTransition2D
                    {
                        Type = MixerTransition2D.MixerType.Cartesian
                    };
                    InitializeChildren(ref cartesianAsset.Transition, blendTree);
                    return cartesianAsset;

                case BlendTreeType.Direct:
                    var manualAsset = CreateInstance<ManualMixerTransitionAsset>();
                    InitializeChildren<ManualMixerTransition, ManualMixerState>(ref manualAsset.Transition, blendTree);
                    return manualAsset;

                default:
                    Debug.LogError($"Unsupported {nameof(BlendTreeType)}: {blendTree.blendType}");
                    return null;
            }
        }

         

        private static void InitializeChildren(ref LinearMixerTransition transition, BlendTree blendTree)
        {
            var children = InitializeChildren<LinearMixerTransition, LinearMixerState>(ref transition, blendTree);
            transition.Thresholds = new float[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].threshold;
        }

        private static void InitializeChildren(ref MixerTransition2D transition, BlendTree blendTree)
        {
            var children = InitializeChildren<MixerTransition2D, MixerState<Vector2>>(ref transition, blendTree);
            transition.Thresholds = new Vector2[children.Length];
            for (int i = 0; i < children.Length; i++)
                transition.Thresholds[i] = children[i].position;
        }

        private static ChildMotion[] InitializeChildren<TTransition, TState>(ref TTransition transition, BlendTree blendTree)
            where TTransition : ManualMixerTransition<TState>, new()
            where TState : ManualMixerState
        {
            transition = new TTransition();

            var children = blendTree.children;
            transition.States = new Object[children.Length];
            float[] speeds = new float[children.Length];
            var hasCustomSpeeds = false;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                transition.States[i] = child.motion is AnimationClip ?
                    child.motion :
                    (Object)GenerateTransition(blendTree, child.motion);

                if ((speeds[i] = child.timeScale) != 1)
                    hasCustomSpeeds = true;
            }

            if (hasCustomSpeeds)
                transition.Speeds = speeds;

            return children;
        }

         

        private static void SaveTransition(Object originalAsset, Object transition)
        {
            if (string.IsNullOrEmpty(transition.name))
                transition.name = originalAsset.name;

            var path = AssetDatabase.GetAssetPath(originalAsset);
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, transition.name + ".asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(transition, path);

            Debug.Log($"Saved {path}", transition);
        }

         
    }
}
#endif
