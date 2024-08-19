
using EasyAnimator.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Mixer Transition/Manual", order = Strings.AssetMenuOrder + 2)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ManualMixerTransitionAsset))]
    public class ManualMixerTransitionAsset : EasyAnimatorTransitionAsset<ManualMixerTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<ManualMixerTransitionAsset, ManualMixerTransition, ManualMixerState>,
            ManualMixerState.ITransition
        { }
    }

    
    [Serializable]
    public abstract class ManualMixerTransition<TMixer> : EasyAnimatorTransition<TMixer>, IMotion, IAnimationClipCollection
        where TMixer : ManualMixerState
    {
         

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

         

        [SerializeField, HideInInspector]
        private Object[] _States;

        public ref Object[] States => ref _States;

        public const string StatesField = nameof(_States);

         

        [SerializeField, HideInInspector]
        private float[] _Speeds;

        public ref float[] Speeds => ref _Speeds;

        public const string SpeedsField = nameof(_Speeds);

        public bool HasSpeeds => _Speeds != null && _Speeds.Length >= _States.Length;

         

        [SerializeField, HideInInspector]
        private bool[] _SynchronizeChildren;

        public ref bool[] SynchronizeChildren => ref _SynchronizeChildren;

        public const string SynchronizeChildrenField = nameof(_SynchronizeChildren);

         

        public override bool IsLooping
        {
            get
            {
                for (int i = _States.Length - 1; i >= 0; i--)
                {
                    if (EasyAnimatorUtilities.TryGetIsLooping(_States[i], out var isLooping) &&
                        isLooping)
                        return true;
                }

                return false;
            }
        }

        public override float MaximumDuration
        {
            get
            {
                if (_States == null)
                    return 0;

                var duration = 0f;
                var hasSpeeds = HasSpeeds;

                for (int i = _States.Length - 1; i >= 0; i--)
                {
                    if (!EasyAnimatorUtilities.TryGetLength(_States[i], out var length))
                        continue;

                    if (hasSpeeds)
                        length *= _Speeds[i];

                    if (duration < length)
                        duration = length;
                }

                return duration;
            }
        }

        public virtual float AverageAngularSpeed
        {
            get
            {
                if (_States == null)
                    return default;

                var average = 0f;
                var hasSpeeds = HasSpeeds;

                var count = 0;
                for (int i = _States.Length - 1; i >= 0; i--)
                {
                    if (EasyAnimatorUtilities.TryGetAverageAngularSpeed(_States[i], out var speed))
                    {
                        if (hasSpeeds)
                            speed *= _Speeds[i];

                        average += speed;
                        count++;
                    }
                }

                return average / count;
            }
        }

        public virtual Vector3 AverageVelocity
        {
            get
            {
                if (_States == null)
                    return default;

                var average = new Vector3();
                var hasSpeeds = HasSpeeds;

                var count = 0;
                for (int i = _States.Length - 1; i >= 0; i--)
                {
                    if (EasyAnimatorUtilities.TryGetAverageVelocity(_States[i], out var velocity))
                    {
                        if (hasSpeeds)
                            velocity *= _Speeds[i];

                        average += velocity;
                        count++;
                    }
                }

                return average / count;
            }
        }

         

        public override bool IsValid
        {
            get
            {
                if (_States == null ||
                    _States.Length == 0)
                    return false;

                for (int i = _States.Length - 1; i >= 0; i--)
                    if (_States[i] == null)
                        return false;

                return true;
            }
        }

         

        public virtual void InitializeState()
        {
            var mixer = State;

            var auto = MixerState.AutoSynchronizeChildren;
            try
            {
                MixerState.AutoSynchronizeChildren = false;
                mixer.Initialize(_States);
            }
            finally
            {
                MixerState.AutoSynchronizeChildren = auto;
            }

            mixer.InitializeSynchronizedChildren(_SynchronizeChildren);

            if (_Speeds != null)
            {
#if UNITY_ASSERTIONS
                if (_Speeds.Length != 0 && _Speeds.Length != _States.Length)
                    Debug.LogError(
                        $"The number of serialized {nameof(Speeds)} ({_Speeds.Length})" +
                        $" does not match the number of {nameof(States)} ({_States.Length}).",
                        mixer.Root?.Component as Object);
#endif

                var children = mixer.ChildStates;
                var count = Math.Min(children.Count, _Speeds.Length);
                while (--count >= 0)
                    children[count].Speed = _Speeds[count];
            }
        }

         

        public override void Apply(EasyAnimatorState state)
        {
            base.Apply(state);

            if (!float.IsNaN(_Speed))
                state.Speed = _Speed;

            for (int i = 0; i < _States.Length; i++)
                if (_States[i] is EasyAnimator.ITransition transition)
                    transition.Apply(state.GetChild(i));
        }

         

        void IAnimationClipCollection.GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(_States);

         
    }
}
