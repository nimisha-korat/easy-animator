
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
    
    public sealed class PlayableAssetState : EasyAnimatorState
    {
         

        public interface ITransition : ITransition<PlayableAssetState> { }

         
        #region Fields and Properties
         

        private PlayableAsset _Asset;

        public PlayableAsset Asset
        {
            get => _Asset;
            set => ChangeMainObject(ref _Asset, value);
        }

        public override Object MainObject
        {
            get => _Asset;
            set => _Asset = (PlayableAsset)value;
        }

         

        private float _Length;

        public override float Length => _Length;

         

        protected override void OnSetIsPlaying()
        {
            var inputCount = _Playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var playable = _Playable.GetInput(i);
                if (!playable.IsValid())
                    continue;

                if (IsPlaying)
                    playable.Play();
                else
                    playable.Pause();
            }
        }

         

        public override void CopyIKFlags(EasyAnimatorNode node) { }

         

        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

         

        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log(
                        $"IK cannot be dynamically enabled on a {nameof(PlayableAssetState)}.", Root?.Component);
#endif
            }
        }

         
        #endregion
         
        #region Methods
         

        public PlayableAssetState(PlayableAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            _Asset = asset;
        }

         

        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Asset.CreatePlayable(Root._Graph, Root.Component.gameObject);
            playable.SetDuration(9223372.03685477);

            _Length = (float)_Asset.duration;

            if (!_HasInitializedBindings)
                InitializeBindings();
        }

         

        private IList<Object> _Bindings;
        private bool _HasInitializedBindings;

         

        public IList<Object> Bindings
        {
            get => _Bindings;
            set
            {
                _Bindings = value;
                InitializeBindings();
            }
        }

         

        public void SetBindings(params Object[] bindings)
        {
            Bindings = bindings;
        }

         

        private void InitializeBindings()
        {
            if (_Bindings == null || Root == null)
                return;

            _HasInitializedBindings = true;

            var bindingCount = _Bindings.Count;
            if (bindingCount == 0)
                return;

            var output = _Asset.outputs.GetEnumerator();
            var graph = Root._Graph;

            for (int i = 0; i < bindingCount; i++)
            {
                if (!output.MoveNext())
                    return;

                if (ShouldSkipBinding(output.Current, out var name, out var type))
                {
                    i--;
                    continue;
                }

                var binding = _Bindings[i];
                if (binding == null && type != null)
                    continue;

#if UNITY_ASSERTIONS
                if (type != null && !type.IsAssignableFrom(binding.GetType()))
                {
                    Debug.LogError(
                        $"Binding Type Mismatch: bindings[{i}] is '{binding}' but should be a {type.FullName} for {name}",
                        Root?.Component as Object);
                    continue;
                }

                Validate.AssertPlayable(this);
#endif

                var playable = _Playable.GetInput(i);

                if (type == typeof(Animator))
                {
                    var playableOutput = AnimationPlayableOutput.Create(graph, name, (Animator)binding);
                    playableOutput.SetSourcePlayable(playable);
                }
                else if (type == typeof(AudioSource))
                {
                    var playableOutput = AudioPlayableOutput.Create(graph, name, (AudioSource)binding);
                    playableOutput.SetSourcePlayable(playable);
                }
                else// ActivationTrack, SignalTrack, ControlTrack, PlayableTrack.
                {
                    var playableOutput = ScriptPlayableOutput.Create(graph, name);
                    playableOutput.SetUserData(binding);
                    playableOutput.SetSourcePlayable(playable);
                }
            }
        }

         

        public static bool ShouldSkipBinding(PlayableBinding binding, out string name, out Type type)
        {
            name = binding.streamName;
            type = binding.outputTargetType;

            if (type == typeof(GameObject) && name == "Markers")
                return true;

            return false;
        }

         

        public override void Destroy()
        {
            _Asset = null;
            base.Destroy();
        }

         
        #endregion
         
    }
}

