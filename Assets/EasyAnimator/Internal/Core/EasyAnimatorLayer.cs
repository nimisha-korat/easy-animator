

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EasyAnimator
{
  
    public sealed class EasyAnimatorLayer : EasyAnimatorNode, IAnimationClipCollection
    {
         
        #region Fields and Properties
         

        internal EasyAnimatorLayer(EasyAnimatorPlayable root, int index)
        {
#if UNITY_ASSERTIONS
            GC.SuppressFinalize(this);
#endif

            Root = root;
            Index = index;
            CreatePlayable();

            if (ApplyParentAnimatorIK)
                _ApplyAnimatorIK = root.ApplyAnimatorIK;
            if (ApplyParentFootIK)
                _ApplyFootIK = root.ApplyFootIK;
        }

         

        protected override void CreatePlayable(out Playable playable) => playable = AnimationMixerPlayable.Create(Root._Graph);

         

        public override EasyAnimatorLayer Layer => this;

        public override IPlayableWrapper Parent => Root;

        public override bool KeepChildrenConnected => Root.KeepChildrenConnected;

         

        private readonly List<EasyAnimatorState> States = new List<EasyAnimatorState>();

         

        private EasyAnimatorState _CurrentState;

       
        public EasyAnimatorState CurrentState
        {
            get => _CurrentState;
            private set
            {
                _CurrentState = value;
                CommandCount++;
            }
        }

       
        public int CommandCount { get; private set; }

#if UNITY_EDITOR
        internal void IncrementCommandCount() => CommandCount++;
#endif

         

       
        public bool IsAdditive
        {
            get => Root.Layers.IsAdditive(Index);
            set => Root.Layers.SetAdditive(Index, value);
        }

         

       
        public void SetMask(AvatarMask mask)
        {
            Root.Layers.SetMask(Index, mask);
        }

#if UNITY_ASSERTIONS
        internal AvatarMask _Mask;
#endif

         

       
        public Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var state = States[i];
                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

         
        #endregion
         
        #region Child States
         

        public override int ChildCount => States.Count;

      
        public override EasyAnimatorState GetChild(int index) => States[index];

        public EasyAnimatorState this[int index] => States[index];

         

      
        public void AddChild(EasyAnimatorState state)
        {
            if (state.Parent == this)
                return;

            state.SetRoot(Root);

            var index = States.Count;
            States.Add(null);// OnAddChild will assign the state.
            _Playable.SetInputCount(index + 1);
            state.SetParent(this, index);
        }

         

        protected internal override void OnAddChild(EasyAnimatorState state) => OnAddChild(States, state);

         

        protected internal override void OnRemoveChild(EasyAnimatorState state)
        {
            var index = state.Index;
            Validate.AssertCanRemoveChild(state, States);

            if (_Playable.GetInput(index).IsValid())
                Root._Graph.Disconnect(_Playable, index);

            // Swap the last state into the place of the one that was just removed.
            var last = States.Count - 1;
            if (index < last)
            {
                state = States[last];
                state.DisconnectFromGraph();

                States[index] = state;
                state.Index = index;

                if (state.Weight != 0 || Root.KeepChildrenConnected)
                    state.ConnectToGraph();
            }

            States.RemoveAt(last);
            _Playable.SetInputCount(last);
        }

         

        public override FastEnumerator<EasyAnimatorState> GetEnumerator()
            => new FastEnumerator<EasyAnimatorState>(States);

         
        #endregion
         
        #region Create State
         

       
        public ClipState CreateState(AnimationClip clip) => CreateState(Root.GetKey(clip), clip);

      
        public ClipState CreateState(object key, AnimationClip clip)
        {
            var state = new ClipState(clip)
            {
                _Key = key,
            };
            AddChild(state);
            return state;
        }

         

      
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
        }

       
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
        }

      
        public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
        {
            GetOrCreateState(clip0);
            GetOrCreateState(clip1);
            GetOrCreateState(clip2);
            GetOrCreateState(clip3);
        }

      
        public void CreateIfNew(params AnimationClip[] clips)
        {
            if (clips == null)
                return;

            var count = clips.Length;
            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    GetOrCreateState(clip);
            }
        }

         

      
        public EasyAnimatorState GetOrCreateState(AnimationClip clip, bool allowSetClip = false)
        {
            return GetOrCreateState(Root.GetKey(clip), clip, allowSetClip);
        }

       
        public EasyAnimatorState GetOrCreateState(ITransition transition)
        {
            var state = Root.States.GetOrCreate(transition);
            state.LayerIndex = Index;
            return state;
        }

      
        public EasyAnimatorState GetOrCreateState(object key, AnimationClip clip, bool allowSetClip = false)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (Root.States.TryGet(key, out var state))
            {
                // If a state exists with the 'key' but has the wrong clip, either change it or complain.
                if (!ReferenceEquals(state.Clip, clip))
                {
                    if (allowSetClip)
                    {
                        state.Clip = clip;
                    }
                    else
                    {
                        throw new ArgumentException(EasyAnimatorPlayable.StateDictionary.GetClipMismatchError(key, state.Clip, clip));
                    }
                }
                else// Otherwise make sure it is on the correct layer.
                {
                    AddChild(state);
                }
            }
            else
            {
                state = CreateState(key, clip);
            }

            return state;
        }

         

        public void DestroyStates()
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                States[i].Destroy();
            }

            States.Clear();
        }

         
        #endregion
         
        #region Play Management
         

        protected internal override void OnStartFade()
        {
            for (int i = States.Count - 1; i >= 0; i--)
                States[i].OnStartFade();
        }

         
        // Play Immediately.
         

       
        public EasyAnimatorState Play(AnimationClip clip)
            => Play(GetOrCreateState(clip));

       
        public EasyAnimatorState Play(EasyAnimatorState state)
        {
            if (Weight == 0 && TargetWeight == 0)
                Weight = 1;

            AddChild(state);

            CurrentState = state;

            state.Play();

            for (int i = States.Count - 1; i >= 0; i--)
            {
                var otherState = States[i];
                if (otherState != state)
                    otherState.Stop();
            }

            return state;
        }

         
        // Cross Fade.
         

       
        public EasyAnimatorState Play(AnimationClip clip, float fadeDuration, FadeMode mode = default)
            => Play(Root.States.GetOrCreate(clip), fadeDuration, mode);

       
        public EasyAnimatorState Play(EasyAnimatorState state, float fadeDuration, FadeMode mode = default)
        {
            // Skip the fade if:
            if (fadeDuration <= 0 ||// There is no duration.
                (Root.SkipFirstFade && Index == 0 && Weight == 0))// Or this is Layer 0 and it has no weight.
            {
                if (mode == FadeMode.FromStart || mode == FadeMode.NormalizedFromStart)
                    state.Time = 0;

                Weight = 1;
                return Play(state);
            }

            EvaluateFadeMode(mode, ref state, ref fadeDuration);

            StartFade(1, fadeDuration);
            if (Weight == 0)
                return Play(state);

            AddChild(state);

            CurrentState = state;

            // If the state is already playing or will finish fading in faster than this new fade,
            // continue the existing fade but still pretend it was restarted.
            if (state.IsPlaying && state.TargetWeight == 1 &&
                (state.Weight == 1 || state.FadeSpeed * fadeDuration > Math.Abs(1 - state.Weight)))
            {
                OnStartFade();
            }
            else// Otherwise fade in the target state and fade out all others.
            {
                state.IsPlaying = true;
                state.StartFade(1, fadeDuration);

                for (int i = States.Count - 1; i >= 0; i--)
                {
                    var otherState = States[i];
                    if (otherState != state)
                        otherState.StartFade(0, fadeDuration);
                }
            }

            return state;
        }

         
        // Transition.
         

       
        public EasyAnimatorState Play(ITransition transition)
            => Play(transition, transition.FadeDuration, transition.FadeMode);

       
        public EasyAnimatorState Play(ITransition transition, float fadeDuration, FadeMode mode = default)
        {
            var state = Root.States.GetOrCreate(transition);
            state = Play(state, fadeDuration, mode);
            transition.Apply(state);
            return state;
        }

         
        // Try Play.
         

       
        public EasyAnimatorState TryPlay(object key)
            => Root.States.TryGet(key, out var state) ? Play(state) : null;

       
        public EasyAnimatorState TryPlay(object key, float fadeDuration, FadeMode mode = default)
            => Root.States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

         

       
        private void EvaluateFadeMode(FadeMode mode, ref EasyAnimatorState state, ref float fadeDuration)
        {
            switch (mode)
            {
                case FadeMode.FixedSpeed:
                    fadeDuration *= Math.Abs(1 - state.Weight);
                    break;

                case FadeMode.FixedDuration:
                    break;

                case FadeMode.FromStart:
                    {
#if UNITY_ASSERTIONS
                        if (!(state is ClipState))
                            throw new ArgumentException(
                                $"{nameof(FadeMode)}.{nameof(FadeMode.FromStart)} can only be used on {nameof(ClipState)}s." +
                                $" State = {state}");
#endif

                        var previousState = state;
                        state = GetOrCreateWeightlessState(state);
                        if (previousState != state)
                        {
                            var previousLayer = previousState.Layer;
                            if (previousLayer != this && previousLayer.CurrentState == previousState)
                                previousLayer.StartFade(0, fadeDuration);
                        }

                        break;
                    }

                case FadeMode.NormalizedSpeed:
                    fadeDuration *= Math.Abs(1 - state.Weight) * state.Length;
                    break;

                case FadeMode.NormalizedDuration:
                    fadeDuration *= state.Length;
                    break;

                case FadeMode.NormalizedFromStart:
                    {
#if UNITY_ASSERTIONS
                        if (!(state is ClipState))
                            throw new ArgumentException(
                                $"{nameof(FadeMode)}.{nameof(FadeMode.NormalizedFromStart)} can only be used on {nameof(ClipState)}s." +
                                $" State = {state}");
#endif

                        var previousState = state;
                        state = GetOrCreateWeightlessState(state);
                        fadeDuration *= state.Length;// This block is identical to FromStart except for this line.
                        if (previousState != state)
                        {
                            var previousLayer = previousState.Layer;
                            if (previousLayer != this && previousLayer.CurrentState == previousState)
                                previousLayer.StartFade(0, fadeDuration);
                        }

                        break;
                    }

                default:
                    throw new ArgumentException($"Invalid {nameof(FadeMode)}: {mode}", nameof(mode));
            }
        }

         

#if UNITY_ASSERTIONS
       
        public static int MaxStateDepth { get; private set; } = 5;
#endif

     
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void SetMaxStateDepth(int depth)
        {
#if UNITY_ASSERTIONS
            MaxStateDepth = depth;
#endif
        }

       
        public EasyAnimatorState GetOrCreateWeightlessState(EasyAnimatorState state)
        {
            if (state.Weight != 0)
            {
                var clip = state.Clip;
                if (clip == null)
                {
                    // We could probably support any state type by giving them a Clone method, but that would take a
                    // lot of work for something that might never get used.
                    throw new InvalidOperationException(
                        $"{nameof(GetOrCreateWeightlessState)} can only be used on {nameof(ClipState)}s. State = " + state);
                }

                // Use any earlier state that is weightless.
                var keyState = state;
                while (true)
                {
                    keyState = keyState.Key as EasyAnimatorState;
                    if (keyState == null)
                    {
                        break;
                    }
                    else if (keyState.Weight == 0)
                    {
                        state = keyState;
                        goto GotWeightlessState;
                    }
                }

#if UNITY_ASSERTIONS
                int depth = 0;
#endif

                do
                {
                    // Explicitly cast the state to an object to avoid the overload that warns about using a state as a key.
                    state = Root.States.GetOrCreate((object)state, clip);

#if UNITY_ASSERTIONS
                    if (++depth == MaxStateDepth)
                    {
                        throw new ArgumentOutOfRangeException(nameof(depth),
                            $"{nameof(EasyAnimatorLayer)}.{nameof(GetOrCreateWeightlessState)}" +
                            $" has created {MaxStateDepth} states for a single clip." +
                            $" This is most likely a result of calling the method repeatedly on consecutive frames." +
                            $" This can be avoided by using a different {nameof(FadeMode)} or calling" +
                            $" {nameof(EasyAnimatorLayer)}.{nameof(SetMaxStateDepth)} to increase the threshold for this warning.");
                    }
#endif
                }
                while (state.Weight != 0);
            }

            GotWeightlessState:

            // Make sure it is on this layer and at time 0.
            AddChild(state);
            state.Time = 0;

            return state;
        }

         
        // Stopping
         

      
        public override void Stop()
        {
            base.Stop();

            CurrentState = null;

            for (int i = States.Count - 1; i >= 0; i--)
                States[i].Stop();
        }

         
        // Checking
         

      
        public bool IsPlayingClip(AnimationClip clip)
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                var state = States[i];
                if (state.Clip == clip && state.IsPlaying)
                    return true;
            }

            return false;
        }

       
        public bool IsAnyStatePlaying()
        {
            for (int i = States.Count - 1; i >= 0; i--)
            {
                if (States[i].IsPlaying)
                    return true;
            }

            return false;
        }

       
        protected internal override bool IsPlayingAndNotEnding() => _CurrentState != null && _CurrentState.IsPlayingAndNotEnding();

         

      
        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = States.Count - 1; i >= 0; i--)
            {
                weight += States[i].Weight;
            }

            return weight;
        }

         
        #endregion
         
        #region Inverse Kinematics
         

        private bool _ApplyAnimatorIK;

        public override bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set => base.ApplyAnimatorIK = _ApplyAnimatorIK = value;
        }

         

        private bool _ApplyFootIK;

        public override bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set => base.ApplyFootIK = _ApplyFootIK = value;
        }

         
        #endregion
         
        #region Inspector
         

        
        public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(States);

         

        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName == null)
            {
                if (_Mask != null)
                    return _Mask.name;

                SetDebugName(Index == 0 ? "Base Layer" : "Layer " + Index);
            }

            return base.ToString();
#else
            return "Layer " + Index;
#endif
        }

         

        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator).Append($"{nameof(CurrentState)}: ").Append(CurrentState);
            text.Append(separator).Append($"{nameof(CommandCount)}: ").Append(CommandCount);
            text.Append(separator).Append($"{nameof(IsAdditive)}: ").Append(IsAdditive);

#if UNITY_ASSERTIONS
            text.Append(separator).Append($"{nameof(AvatarMask)}: ").Append(EasyAnimatorUtilities.ToStringOrNull(_Mask));
#endif
        }

         
        #endregion
         
    }
}

