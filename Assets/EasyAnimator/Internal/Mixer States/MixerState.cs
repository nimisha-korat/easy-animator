
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    public abstract partial class MixerState : EasyAnimatorState
    {
         

        public interface ITransition2D : ITransition<MixerState<Vector2>> { }

         
        #region Properties
         

        public override bool KeepChildrenConnected => true;

        public override AnimationClip Clip => null;

         

        public abstract IList<EasyAnimatorState> ChildStates { get; }

        public override int ChildCount => ChildStates.Count;

        public override EasyAnimatorState GetChild(int index) => ChildStates[index];

        public override FastEnumerator<EasyAnimatorState> GetEnumerator()
            => new FastEnumerator<EasyAnimatorState>(ChildStates);

         

        protected override void OnSetIsPlaying()
        {
            var childStates = ChildStates;
            for (int i = childStates.Count - 1; i >= 0; i--)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                state.IsPlaying = IsPlaying;
            }
        }

         

        public override bool IsLooping
        {
            get
            {
                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    if (state.IsLooping)
                        return true;
                }

                return false;
            }
        }

         

        protected override float RawTime
        {
            get
            {
                RecalculateWeights();

                if (!GetSynchronizedTimeDetails(out var totalWeight, out var normalizedTime, out var length))
                    GetTimeDetails(out totalWeight, out normalizedTime, out length);

                if (totalWeight == 0)
                    return base.RawTime;

                totalWeight *= totalWeight;
                return normalizedTime * length / totalWeight;
            }
            set
            {
                var states = ChildStates;
                var childCount = states.Count;

                if (value == 0)
                    goto ZeroTime;

                var length = Length;
                if (length == 0)
                    goto ZeroTime;

                value /= length;// Normalize.

                while (--childCount >= 0)
                {
                    var state = states[childCount];
                    if (state != null)
                        state.NormalizedTime = value;
                }

                return;

                // If the value is 0, we can set the child times slightly more efficiently.
                ZeroTime:
                while (--childCount >= 0)
                {
                    var state = states[childCount];
                    if (state != null)
                        state.Time = 0;
                }
            }
        }

         

        public override void MoveTime(float time, bool normalized)
        {
            base.MoveTime(time, normalized);

            var states = ChildStates;
            var count = states.Count;
            for (int i = 0; i < count; i++)
                states[i].MoveTime(time, normalized);
        }

         

        private bool GetSynchronizedTimeDetails(out float totalWeight, out float normalizedTime, out float length)
        {
            totalWeight = 0;
            normalizedTime = 0;
            length = 0;

            if (_SynchronizedChildren != null)
            {
                for (int i = _SynchronizedChildren.Count - 1; i >= 0; i--)
                {
                    var state = _SynchronizedChildren[i];
                    var weight = state.Weight;
                    if (weight == 0)
                        continue;

                    var stateLength = state.Length;
                    if (stateLength == 0)
                        continue;

                    totalWeight += weight;
                    normalizedTime += state.Time / stateLength * weight;
                    length += stateLength * weight;
                }
            }

            return totalWeight > MinimumSynchronizeChildrenWeight;
        }

        private void GetTimeDetails(out float totalWeight, out float normalizedTime, out float length)
        {
            totalWeight = 0;
            normalizedTime = 0;
            length = 0;

            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                var weight = state.Weight;
                if (weight == 0)
                    continue;

                var stateLength = state.Length;
                if (stateLength == 0)
                    continue;

                totalWeight += weight;
                normalizedTime += state.Time / stateLength * weight;
                length += stateLength * weight;
            }
        }

         

        public override float Length
        {
            get
            {
                RecalculateWeights();

                var length = 0f;
                var totalChildWeight = 0f;

                if (_SynchronizedChildren != null)
                {
                    for (int i = _SynchronizedChildren.Count - 1; i >= 0; i--)
                    {
                        var state = _SynchronizedChildren[i];
                        var weight = state.Weight;
                        if (weight == 0)
                            continue;

                        var stateLength = state.Length;
                        if (stateLength == 0)
                            continue;

                        totalChildWeight += weight;
                        length += stateLength * weight;
                    }
                }

                if (totalChildWeight > 0)
                    return length / totalChildWeight;

                var states = ChildStates;
                totalChildWeight = CalculateTotalWeight(states);
                if (totalChildWeight <= 0)
                    return 0;

                for (int i = states.Count - 1; i >= 0; i--)
                {
                    var state = states[i];
                    if (state != null)
                        length += state.Length * state.Weight;
                }

                return length / totalChildWeight;
            }
        }

         
        #endregion
         
        #region Initialisation
         

        protected override void CreatePlayable(out Playable playable)
        {
            playable = AnimationMixerPlayable.Create(Root._Graph, ChildStates.Count, false);
            RecalculateWeights();
        }

         

        public ClipState CreateChild(int index, AnimationClip clip)
        {
            var state = new ClipState(clip);
            state.SetParent(this, index);
            state.IsPlaying = IsPlaying;
            return state;
        }

        public EasyAnimatorState CreateChild(int index, EasyAnimator.ITransition transition)
        {
            var state = transition.CreateStateAndApply(Root);
            state.SetParent(this, index);
            state.IsPlaying = IsPlaying;
            return state;
        }

        public EasyAnimatorState CreateChild(int index, Object state)
        {
            if (state is AnimationClip clip)
            {
                return CreateChild(index, clip);
            }
            else if (state is ITransition transition)
            {
                return CreateChild(index, transition);
            }
            else return null;
        }

         

        public void SetChild(int index, EasyAnimatorState state) => state.SetParent(this, index);

         

        protected internal override void OnAddChild(EasyAnimatorState state)
        {
            OnAddChild(ChildStates, state);

            if (AutoSynchronizeChildren)
                Synchronize(state);

#if UNITY_ASSERTIONS
            if (_IsGeneratedName)
            {
                _IsGeneratedName = false;
                SetDebugName(null);
            }
#endif
        }

         

        protected internal override void OnRemoveChild(EasyAnimatorState state)
        {
            if (_SynchronizedChildren != null)
                _SynchronizedChildren.Remove(state);

            var states = ChildStates;
            Validate.AssertCanRemoveChild(state, states);
            states[state.Index] = null;
            Root?._Graph.Disconnect(_Playable, state.Index);

#if UNITY_ASSERTIONS
            if (_IsGeneratedName)
            {
                _IsGeneratedName = false;
                SetDebugName(null);
            }
#endif
        }

         

        public override void Destroy()
        {
            DestroyChildren();
            base.Destroy();
        }

         

        public void DestroyChildren()
        {
            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state != null)
                    state.Destroy();
            }
        }

         
        #endregion
         
        #region Jobs
         

        public AnimationScriptPlayable CreatePlayable<T>(EasyAnimatorPlayable root, T job, bool processInputs = false)
            where T : struct, IAnimationJob
        {
            SetRoot(null);

            Root = root;
            root.States.Register(Key, this);

            var playable = AnimationScriptPlayable.Create(root._Graph, job, ChildCount);

            if (!processInputs)
                playable.SetProcessInputs(false);

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.SetRoot(root);

            return playable;
        }

         

        protected void CreatePlayable<T>(out Playable playable, T job, bool processInputs = false)
            where T : struct, IAnimationJob
        {
            var scriptPlayable = AnimationScriptPlayable.Create(Root._Graph, job, ChildCount);

            if (!processInputs)
                scriptPlayable.SetProcessInputs(false);

            playable = scriptPlayable;
        }

         

        public T GetJobData<T>()
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).GetJobData<T>();

        public void SetJobData<T>(T value)
            where T : struct, IAnimationJob
            => ((AnimationScriptPlayable)_Playable).SetJobData<T>(value);

         
        #endregion
         
        #region Updates
         

        protected internal override void Update(out bool needsMoreUpdates)
        {
            base.Update(out needsMoreUpdates);

            if (RecalculateWeights())
            {
                // Apply the child weights immediately to ensure they are all in sync. Otherwise some of them might
                // have already updated before the mixer and would not apply it until next frame.
                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    state.ApplyWeight();
                }
            }

            ApplySynchronizeChildren(ref needsMoreUpdates);
        }

         

        public bool WeightsAreDirty { get; set; }

         

        public bool RecalculateWeights()
        {
            if (WeightsAreDirty)
            {
                ForceRecalculateWeights();

                Debug.Assert(!WeightsAreDirty,
                    $"{nameof(MixerState)}.{nameof(WeightsAreDirty)} was not set to false by {nameof(ForceRecalculateWeights)}().");

                return true;
            }
            else return false;
        }

         

        protected virtual void ForceRecalculateWeights() { }

         
        #endregion
         
        #region Synchronize Children
         

        public static bool AutoSynchronizeChildren { get; set; } = true;

        public static float MinimumSynchronizeChildrenWeight { get; set; } = 0.01f;

         

        private List<EasyAnimatorState> _SynchronizedChildren;

        public EasyAnimatorState[] SynchronizedChildren
        {
            get => SynchronizedChildCount > 0 ? _SynchronizedChildren.ToArray() : Array.Empty<EasyAnimatorState>();
            set
            {
                if (_SynchronizedChildren == null)
                    _SynchronizedChildren = new List<EasyAnimatorState>();
                else
                    _SynchronizedChildren.Clear();

                for (int i = 0; i < value.Length; i++)
                    Synchronize(value[i]);
            }
        }

        public int SynchronizedChildCount => _SynchronizedChildren != null ? _SynchronizedChildren.Count : 0;

         

        public bool IsSynchronized(EasyAnimatorState state)
        {
            var synchronizer = GetParentMixer();
            return
                synchronizer._SynchronizedChildren != null &&
                synchronizer._SynchronizedChildren.Contains(state);
        }

         

        public void Synchronize(EasyAnimatorState state)
        {
            if (state == null)
                return;

#if UNITY_ASSERTIONS
            if (!IsChildOf(state, this))
                throw new ArgumentException(
                    $"State is not a child of the mixer." +
                    $"\n - State: {state}" +
                    $"\n - Mixer: {this}",
                    nameof(state));
#endif

            var synchronizer = GetParentMixer();
            synchronizer.SynchronizeDirect(state);
        }

        private void SynchronizeDirect(EasyAnimatorState state)
        {
            if (state == null)
                return;

            if (state is MixerState mixer)
            {
                for (int i = 0; i < mixer._SynchronizedChildren.Count; i++)
                    Synchronize(mixer._SynchronizedChildren[i]);
                mixer._SynchronizedChildren.Clear();
                return;
            }

#if UNITY_ASSERTIONS
            if (OptionalWarning.MixerSynchronizeZeroLength.IsEnabled() && state.Length == 0)
                OptionalWarning.MixerSynchronizeZeroLength.Log(
                    $"Adding a state with zero {nameof(EasyAnimatorState.Length)} to the synchronization list: '{state}'." +
                    $"\n\nSynchronization is based on the {nameof(NormalizedTime)}" +
                    $" which can't be calculated if the {nameof(Length)} is 0." +
                    $" Some state types can change their {nameof(Length)}, in which case you can just disable this warning." +
                    $" But otherwise, the indicated state probably shouldn't be added to the synchronization list.", Root?.Component);
#endif

            if (_SynchronizedChildren == null)
                _SynchronizedChildren = new List<EasyAnimatorState>();

#if UNITY_ASSERTIONS
            if (_SynchronizedChildren.Contains(state))
                Debug.LogError($"{state} is already in the {nameof(SynchronizedChildren)} list.");
#endif

            _SynchronizedChildren.Add(state);
            RequireUpdate();
        }

         

        public void DontSynchronize(EasyAnimatorState state)
        {
            var synchronizer = GetParentMixer();
            if (synchronizer._SynchronizedChildren != null &&
                synchronizer._SynchronizedChildren.Remove(state))
                state._Playable.SetSpeed(state.Speed);
        }

         

        public void DontSynchronizeChildren()
        {
            var synchronizer = GetParentMixer();
            var SynchronizedChildren = synchronizer._SynchronizedChildren;
            if (SynchronizedChildren == null)
                return;

            if (synchronizer == this)
            {
                for (int i = SynchronizedChildren.Count - 1; i >= 0; i--)
                {
                    var state = SynchronizedChildren[i];
                    state._Playable.SetSpeed(state.Speed);
                }

                SynchronizedChildren.Clear();
            }
            else
            {
                for (int i = SynchronizedChildren.Count - 1; i >= 0; i--)
                {
                    var state = SynchronizedChildren[i];
                    if (IsChildOf(state, this))
                    {
                        state._Playable.SetSpeed(state.Speed);
                        SynchronizedChildren.RemoveAt(i);
                    }
                }
            }
        }

         

        public void InitializeSynchronizedChildren(params bool[] synchronizeChildren)
        {
            EasyAnimatorUtilities.Assert(GetParentMixer() == this,
                $"{nameof(InitializeSynchronizedChildren)} cannot be used on a mixer that is a child of another mixer.");
            EasyAnimatorUtilities.Assert(_SynchronizedChildren == null,
                $"{nameof(InitializeSynchronizedChildren)} cannot be used on a mixer already has synchronized children.");

            int flagCount;
            if (synchronizeChildren != null)
            {
                flagCount = synchronizeChildren.Length;
                for (int i = 0; i < flagCount; i++)
                    if (synchronizeChildren[i])
                        SynchronizeDirect(GetChild(i));
            }
            else flagCount = 0;

            for (int i = flagCount; i < ChildCount; i++)
                SynchronizeDirect(GetChild(i));
        }

         

        public MixerState GetParentMixer()
        {
            var mixer = this;

            var parent = Parent;
            while (parent != null)
            {
                if (parent is MixerState parentMixer)
                    mixer = parentMixer;

                parent = parent.Parent;
            }

            return mixer;
        }

        public static MixerState GetParentMixer(IPlayableWrapper node)
        {
            MixerState mixer = null;

            while (node != null)
            {
                if (node is MixerState parentMixer)
                    mixer = parentMixer;

                node = node.Parent;
            }

            return mixer;
        }

         

        public static bool IsChildOf(IPlayableWrapper child, IPlayableWrapper parent)
        {
            while (true)
            {
                child = child.Parent;
                if (child == parent)
                    return true;
                else if (child == null)
                    return false;
            }
        }

         

        protected void ApplySynchronizeChildren(ref bool needsMoreUpdates)
        {
            if (_SynchronizedChildren == null || _SynchronizedChildren.Count <= 1)
                return;

            needsMoreUpdates = true;

            var deltaTime = EasyAnimatorPlayable.DeltaTime * CalculateRealEffectiveSpeed();
            if (deltaTime == 0)
                return;

            var count = _SynchronizedChildren.Count;

            // Calculate the weighted average normalized time and normalized speed of all children.

            var totalWeight = 0f;
            var weightedNormalizedTime = 0f;
            var weightedNormalizedSpeed = 0f;

            for (int i = 0; i < count; i++)
            {
                var state = _SynchronizedChildren[i];

                var weight = state.Weight;
                if (weight == 0)
                    continue;

                var length = state.Length;
                if (length == 0)
                    continue;

                totalWeight += weight;

                weight /= length;

                weightedNormalizedTime += state.Time * weight;
                weightedNormalizedSpeed += state.Speed * weight;
            }

#if UNITY_ASSERTIONS
            if (!(totalWeight >= 0) || totalWeight == float.PositiveInfinity)// Reversed comparison includes NaN.
                throw new ArgumentOutOfRangeException(nameof(totalWeight), totalWeight, "Total weight must be a finite positive value");
            if (!weightedNormalizedTime.IsFinite())
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedTime), weightedNormalizedTime, "Time must be finite");
            if (!weightedNormalizedSpeed.IsFinite())
                throw new ArgumentOutOfRangeException(nameof(weightedNormalizedSpeed), weightedNormalizedSpeed, "Speed must be finite");
#endif

            // If the total weight is too small, pretend they are all at Weight = 1.
            if (totalWeight < MinimumSynchronizeChildrenWeight)
            {
                weightedNormalizedTime = 0;
                weightedNormalizedSpeed = 0;

                var nonZeroCount = 0;
                for (int i = 0; i < count; i++)
                {
                    var state = _SynchronizedChildren[i];

                    var length = state.Length;
                    if (length == 0)
                        continue;

                    length = 1f / length;

                    weightedNormalizedTime += state.Time * length;
                    weightedNormalizedSpeed += state.Speed * length;

                    nonZeroCount++;
                }

                totalWeight = nonZeroCount;
            }

            // Increment that time value according to delta time.
            weightedNormalizedTime += deltaTime * weightedNormalizedSpeed;
            weightedNormalizedTime /= totalWeight;

            var inverseDeltaTime = 1f / deltaTime;

            // Modify the speed of all children to go from their current normalized time to the average in one frame.
            for (int i = 0; i < count; i++)
            {
                var state = _SynchronizedChildren[i];
                var length = state.Length;
                if (length == 0)
                    continue;

                var normalizedTime = state.Time / length;
                var speed = (weightedNormalizedTime - normalizedTime) * length * inverseDeltaTime;
                state._Playable.SetSpeed(speed);
            }

            // After this, all the playables will update and advance according to their new speeds this frame.
        }

         

        public float CalculateRealEffectiveSpeed()
        {
            var speed = _Playable.GetSpeed() * Root.Speed;

            var parent = Parent;
            while (parent != null)
            {
                speed *= parent.Playable.GetSpeed();
                parent = parent.Parent;
            }

            return (float)speed;
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
         
        #region Other Methods
         

        public float CalculateTotalWeight(IList<EasyAnimatorState> states)
        {
            var total = 0f;

            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state != null)
                    total += state.Weight;
            }

            return total;
        }

         

        public void SetChildrenTime(float value, bool normalized = false)
        {
            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                if (normalized)
                    state.NormalizedTime = value;
                else
                    state.Time = value;
            }
        }

         

        protected void DisableRemainingStates(int previousIndex)
        {
            var states = ChildStates;
            var childCount = states.Count;
            while (++previousIndex < childCount)
            {
                var state = states[previousIndex];
                if (state == null)
                    continue;

                state.Weight = 0;
            }
        }

         

        protected EasyAnimatorState GetNextState(ref int index)
        {
            var states = ChildStates;
            var childCount = states.Count;
            while (index < childCount)
            {
                var state = states[index];
                if (state != null)
                    return state;

                index++;
            }

            return null;
        }

         

        public void NormalizeWeights(float totalWeight)
        {
            if (totalWeight == 1)
                return;

            totalWeight = 1f / totalWeight;

            var states = ChildStates;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                var state = states[i];
                if (state == null)
                    continue;

                state.Weight *= totalWeight;
            }
        }

         

        public virtual string GetDisplayKey(EasyAnimatorState state) => $"[{state.Index}]";

         

        public override Vector3 AverageVelocity
        {
            get
            {
                var velocity = default(Vector3);

                RecalculateWeights();

                var childStates = ChildStates;
                for (int i = childStates.Count - 1; i >= 0; i--)
                {
                    var state = childStates[i];
                    if (state == null)
                        continue;

                    velocity += state.AverageVelocity * state.Weight;
                }

                return velocity;
            }
        }

         

        public void NormalizeDurations()
        {
            var childStates = ChildStates;

            int divideBy = 0;
            float totalDuration = 0f;

            // Count the number of states that exist and their total duration.
            var count = childStates.Count;
            for (int i = 0; i < count; i++)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                divideBy++;
                totalDuration += state.Duration;
            }

            // Calculate the average duration.
            totalDuration /= divideBy;

            // Set all states to that duration.
            for (int i = 0; i < count; i++)
            {
                var state = childStates[i];
                if (state == null)
                    continue;

                state.Duration = totalDuration;
            }
        }

         

#if UNITY_ASSERTIONS
        private bool _IsGeneratedName;
#endif

        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            // Gather child names.
            var childNames = ObjectPool.AcquireList<string>();
            var allSimple = true;
            var states = ChildStates;
            var count = states.Count;
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (state == null)
                    continue;

                if (state.MainObject != null)
                {
                    childNames.Add(state.MainObject.name);
                }
                else
                {
                    childNames.Add(state.ToString());
                    allSimple = false;
                }
            }

            // If they all have a main object, check if they all have the same prefix so it doesn't need to be repeated.
            int prefixLength = 0;
            count = childNames.Count;
            if (count <= 1 || !allSimple)
            {
                prefixLength = 0;
            }
            else
            {
                var prefix = childNames[0];
                var shortest = prefixLength = prefix.Length;

                for (int iName = 0; iName < count; iName++)
                {
                    var childName = childNames[iName];

                    if (shortest > childName.Length)
                    {
                        shortest = prefixLength = childName.Length;
                    }

                    for (int iCharacter = 0; iCharacter < prefixLength; iCharacter++)
                    {
                        if (childName[iCharacter] != prefix[iCharacter])
                        {
                            prefixLength = iCharacter;
                            break;
                        }
                    }
                }

                if (prefixLength < 3 ||// Less than 3 characters probably isn't an intentional prefix.
                    prefixLength >= shortest)
                    prefixLength = 0;
            }

            // Build the mixer name.
            var mixerName = ObjectPool.AcquireStringBuilder();

            if (count > 0)
            {
                if (prefixLength > 0)
                    mixerName.Append(childNames[0], 0, prefixLength).Append('[');

                for (int i = 0; i < count; i++)
                {
                    if (i > 0)
                        mixerName.Append(", ");

                    var childName = childNames[i];
                    mixerName.Append(childName, prefixLength, childName.Length - prefixLength);
                }

                mixerName.Append(prefixLength > 0 ? "] (" : " (");
            }

            ObjectPool.Release(childNames);

            var type = GetType().FullName;
            if (type.EndsWith("State"))
                mixerName.Append(type, 0, type.Length - 5);
            else
                mixerName.Append(type);

            if (count > 0)
                mixerName.Append(')');

            var result = mixerName.ReleaseToString();

#if UNITY_ASSERTIONS
            _IsGeneratedName = true;
            SetDebugName(result);
#endif

            return result;
        }

         

        protected override void AppendDetails(StringBuilder text, string separator)
        {
            base.AppendDetails(text, separator);

            text.Append(separator).Append("SynchronizedChildren: ");
            if (SynchronizedChildCount == 0)
            {
                text.Append("0");
            }
            else
            {
                text.Append(_SynchronizedChildren.Count);
                separator += Strings.Indent;
                for (int i = 0; i < _SynchronizedChildren.Count; i++)
                {
                    text.Append(separator)
                        .Append(_SynchronizedChildren[i]);
                }
            }
        }

         

        public override void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(ChildStates);

         
        #endregion
         
    }
}

