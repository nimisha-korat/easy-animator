

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{

    public abstract class EasyAnimatorNode : Key, IUpdatable, IEnumerable<EasyAnimatorState>, IEnumerator, IPlayableWrapper
    {
         
        #region Playable
         

       
        protected internal Playable _Playable;

        Playable IPlayableWrapper.Playable => _Playable;

      
        public bool IsValid => _Playable.IsValid();

         

#if UNITY_EDITOR
        internal bool _IsInspectorExpanded;
#endif

         

      
        public virtual void CreatePlayable()
        {
#if UNITY_ASSERTIONS
            if (Root == null)
                throw new InvalidOperationException($"{nameof(EasyAnimatorNode)}.{nameof(Root)}" +
                    $" is null when attempting to create its {nameof(Playable)}: {this}" +
                    $"\nThe {nameof(Root)} is generally set when you first play a state," +
                    " so you probably just need to play it before trying to access it.");

            if (_Playable.IsValid())
                Debug.LogWarning($"{nameof(EasyAnimatorNode)}.{nameof(CreatePlayable)}" +
                    $" was called before destroying the previous {nameof(Playable)}: {this}", Root?.Component as Object);
#endif

            CreatePlayable(out _Playable);

#if UNITY_ASSERTIONS
            if (!_Playable.IsValid())
                throw new InvalidOperationException(
                    $"{nameof(EasyAnimatorNode)}.{nameof(CreatePlayable)} did not create a valid {nameof(Playable)}:" + this);
#endif

            if (_Speed != 1)
                _Playable.SetSpeed(_Speed);

            var parent = Parent;
            if (parent != null)
                ApplyConnectedState(parent);
        }

        protected abstract void CreatePlayable(out Playable playable);

         

        public void DestroyPlayable()
        {
            if (_Playable.IsValid())
                Root._Graph.DestroyPlayable(_Playable);
        }

         

        public virtual void RecreatePlayable()
        {
            DestroyPlayable();
            CreatePlayable();
        }

        public void RecreatePlayableRecursive()
        {
            RecreatePlayable();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.RecreatePlayableRecursive();
        }

         
        #endregion
         
        #region Graph
         

        public EasyAnimatorPlayable Root { get; internal set; }

         

        public abstract EasyAnimatorLayer Layer { get; }

        public abstract IPlayableWrapper Parent { get; }

         

        
        public int Index { get; internal set; } = int.MinValue;

         

        protected EasyAnimatorNode()
        {
#if UNITY_ASSERTIONS
            if (TraceConstructor)
                _ConstructorStackTrace = new System.Diagnostics.StackTrace(true);
#endif
        }

         
#if UNITY_ASSERTIONS
         

       
        public static bool TraceConstructor { get; set; }

         

       
        private System.Diagnostics.StackTrace _ConstructorStackTrace;

         

        ~EasyAnimatorNode()
        {
            if (Root != null ||
                OptionalWarning.UnusedNode.IsDisabled())
                return;

            var name = DebugName;
            if (name == null)
            {
                // ToString will likely throw an exception since finalizers are not run on the main thread.
                try { name = ToString(); }
                catch { name = GetType().FullName; }
            }

            var message = $"The {nameof(Root)} {nameof(EasyAnimatorPlayable)} of '{name}'" +
                $" is null during finalization (garbage collection)." +
                $" This probably means that it was never used for anything and should not have been created.";

            if (_ConstructorStackTrace != null)
                message += "\n\nThis node was created at:\n" + _ConstructorStackTrace;
            else
                message += $"\n\nEnable {nameof(EasyAnimatorNode)}.{nameof(TraceConstructor)} on startup to allow" +
                    $" this warning to include the {nameof(System.Diagnostics.StackTrace)} of when the node was constructed.";

            OptionalWarning.UnusedNode.Log(message);
        }

         
#endif
         

        internal void ConnectToGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException(
                    $"Invalid {nameof(EasyAnimatorNode)}.{nameof(Index)}" +
                    " when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);

            Validate.AssertPlayable(this);
#endif

            var parentPlayable = parent.Playable;
            Root._Graph.Connect(_Playable, 0, parentPlayable, Index);
            parentPlayable.SetInputWeight(Index, _Weight);
            _IsWeightDirty = false;
        }

        internal void DisconnectFromGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

            var parentPlayable = parent.Playable;
            if (parentPlayable.GetInput(Index).IsValid())
                Root._Graph.Disconnect(parentPlayable, Index);
        }

         

        private void ApplyConnectedState(IPlayableWrapper parent)
        {
#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException(
                    $"Invalid {nameof(EasyAnimatorNode)}.{nameof(Index)}" +
                    " when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);
#endif

            _IsWeightDirty = true;

            if (_Weight != 0 || parent.KeepChildrenConnected)
            {
                ConnectToGraph();
            }
            else
            {
                Root.RequirePreUpdate(this);
            }
        }

         

        protected void RequireUpdate()
        {
            Root?.RequirePreUpdate(this);
        }

         

        void IUpdatable.Update()
        {
            if (_Playable.IsValid())
            {
                Update(out var needsMoreUpdates);
                if (needsMoreUpdates)
                    return;
            }

            Root.CancelPreUpdate(this);
        }

       
        protected internal virtual void Update(out bool needsMoreUpdates)
        {
            UpdateFade(out needsMoreUpdates);

            ApplyWeight();

        }

         
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
         

       
        protected internal abstract bool IsPlayingAndNotEnding();

        bool IEnumerator.MoveNext() => IsPlayingAndNotEnding();

        object IEnumerator.Current => null;

        void IEnumerator.Reset() { }

         
        #endregion
         
        #region Children
         

       
        public virtual int ChildCount => 0;

       
        EasyAnimatorNode IPlayableWrapper.GetChild(int index) => GetChild(index);

    
        public virtual EasyAnimatorState GetChild(int index)
            => throw new NotSupportedException(this + " can't have children.");

      
        protected internal virtual void OnAddChild(EasyAnimatorState state)
        {
            state.ClearParent();
            throw new NotSupportedException(this + " can't have children.");
        }

      
        protected internal virtual void OnRemoveChild(EasyAnimatorState state)
        {
            state.ClearParent();
            throw new NotSupportedException(this + " can't have children.");
        }

         

       
        protected void OnAddChild(IList<EasyAnimatorState> states, EasyAnimatorState state)
        {
            var index = state.Index;

            if (states[index] != null)
            {
                state.ClearParent();
                throw new InvalidOperationException(
                    $"Tried to add a state to an already occupied port on {this}:" +
                    $"\n    {nameof(Index)}: {index}" +
                    $"\n    Old State: {states[index]} " +
                    $"\n    New State: {state}");
            }

#if UNITY_ASSERTIONS
            if (state.Root != Root)
                Debug.LogError(
                    $"{nameof(EasyAnimatorNode)}.{nameof(Root)} mismatch:" +
                    $"\n    {nameof(state)}: {state}" +
                    $"\n    {nameof(state)}.{nameof(state.Root)}: {state.Root}" +
                    $"\n    {nameof(Parent)}.{nameof(Root)}: {Root}", Root?.Component as Object);
#endif

            states[index] = state;

            if (Root != null)
                state.ApplyConnectedState(this);
        }

         

       
        public virtual bool KeepChildrenConnected => false;

     
        internal void ConnectAllChildrenToGraph()
        {
            if (!Parent.Playable.GetInput(Index).IsValid())
                ConnectToGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.ConnectAllChildrenToGraph();
        }

       
        internal void DisconnectWeightlessChildrenFromGraph()
        {
            if (Weight == 0)
                DisconnectFromGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.DisconnectWeightlessChildrenFromGraph();
        }

         
        // IEnumerable for 'foreach' statements.
         

        public virtual FastEnumerator<EasyAnimatorState> GetEnumerator() => default;

        IEnumerator<EasyAnimatorState> IEnumerable<EasyAnimatorState>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

         
        #endregion
         
        #region Weight
         

        private float _Weight;

        private bool _IsWeightDirty = true;

         

      
        public float Weight
        {
            get => _Weight;
            set
            {
                SetWeight(value);
                TargetWeight = value;
                FadeSpeed = 0;
            }
        }

       
        public void SetWeight(float value)
        {
            if (_Weight == value)
                return;

#if UNITY_ASSERTIONS
            if (!(value >= 0) || value == float.PositiveInfinity)// Reversed comparison includes NaN.
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Weight)} must be a finite positive value");
#endif

            _Weight = value;
            SetWeightDirty();
        }

        protected internal void SetWeightDirty()
        {
            _IsWeightDirty = true;
            RequireUpdate();
        }

         

       
        internal void ApplyWeight()
        {
            if (!_IsWeightDirty)
                return;

            _IsWeightDirty = false;

            var parent = Parent;
            if (parent == null)
                return;

            Playable parentPlayable;

            if (!parent.KeepChildrenConnected)
            {
                if (_Weight == 0)
                {
                    DisconnectFromGraph();
                    return;
                }

                parentPlayable = parent.Playable;
                if (!parentPlayable.GetInput(Index).IsValid())
                    ConnectToGraph();
            }
            else parentPlayable = parent.Playable;

            parentPlayable.SetInputWeight(Index, _Weight);
        }

         

       
        public float EffectiveWeight
        {
            get
            {
                var weight = Weight;

                var parent = Parent;
                while (parent != null)
                {
                    weight *= parent.Weight;
                    parent = parent.Parent;
                }

                return weight;
            }
        }

         
        #endregion
         
        #region Fading
         

       
        public float TargetWeight { get; set; }

     
        public float FadeSpeed { get; set; }

         

       
        public void StartFade(float targetWeight)
            => StartFade(targetWeight, EasyAnimatorPlayable.DefaultFadeDuration);

       
        public void StartFade(float targetWeight, float fadeDuration)
        {

            TargetWeight = targetWeight;

            if (targetWeight == Weight)
            {
                if (targetWeight == 0)
                {
                    Stop();
                }
                else
                {
                    FadeSpeed = 0;
                    OnStartFade();
                }

                return;
            }

            // Duration 0 = Instant.
            if (fadeDuration <= 0)
            {
                FadeSpeed = float.PositiveInfinity;
            }
            else// Otherwise determine how fast we need to go to cover the distance in the specified time.
            {
                FadeSpeed = Math.Abs(Weight - targetWeight) / fadeDuration;
            }

            OnStartFade();
            RequireUpdate();
        }

         

        protected internal abstract void OnStartFade();

         

       
        public virtual void Stop()
        {
            Weight = 0;
        }

         

        
        private void UpdateFade(out bool needsMoreUpdates)
        {
            var fadeSpeed = FadeSpeed;
            if (fadeSpeed == 0)
            {
                needsMoreUpdates = false;
                return;
            }

            _IsWeightDirty = true;

            fadeSpeed *= ParentEffectiveSpeed * EasyAnimatorPlayable.DeltaTime;
            if (fadeSpeed < 0)
                fadeSpeed = -fadeSpeed;

            var target = TargetWeight;
            var current = _Weight;

            var delta = target - current;
            if (delta > 0)
            {
                if (delta > fadeSpeed)
                {
                    _Weight = current + fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }
            else
            {
                if (-delta > fadeSpeed)
                {
                    _Weight = current - fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }

            _Weight = target;
            needsMoreUpdates = false;

            if (target == 0)
            {
                Stop();
            }
            else
            {
                FadeSpeed = 0;
            }
        }

         
        #endregion
         
        #region Inverse Kinematics
         

      
        public static bool ApplyParentAnimatorIK { get; set; } = true;

       
        public static bool ApplyParentFootIK { get; set; } = true;

         

       
        public virtual void CopyIKFlags(EasyAnimatorNode node)
        {
            if (Root == null)
                return;

            if (ApplyParentAnimatorIK)
            {
                ApplyAnimatorIK = node.ApplyAnimatorIK;
                if (ApplyParentFootIK)
                    ApplyFootIK = node.ApplyFootIK;
            }
            else if (ApplyParentFootIK)
            {
                ApplyFootIK = node.ApplyFootIK;
            }
        }

         

        public virtual bool ApplyAnimatorIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyAnimatorIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyAnimatorIK = value;
                }
            }
        }

         

        public virtual bool ApplyFootIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyFootIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyFootIK = value;
                }
            }
        }

         
        #endregion
         
        #region Speed
         

        private float _Speed = 1;

        
        public float Speed
        {
            get => _Speed;
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Speed)} must be finite");

                OptionalWarning.UnsupportedSpeed.Log(UnsupportedSpeedMessage, Root?.Component);
#endif
                _Speed = value;

                if (_Playable.IsValid())
                    _Playable.SetSpeed(value);
            }
        }

#if UNITY_ASSERTIONS
      
        protected virtual string UnsupportedSpeedMessage => null;
#endif

         

      
        private float ParentEffectiveSpeed
        {
            get
            {
                var parent = Parent;
                if (parent == null)
                    return 1;

                var speed = parent.Speed;

                while ((parent = parent.Parent) != null)
                {
                    speed *= parent.Speed;
                }

                return speed;
            }
        }

       
        public float EffectiveSpeed
        {
            get => Speed * ParentEffectiveSpeed;
            set => Speed = value / ParentEffectiveSpeed;
        }

         
        #endregion
         
        #region Descriptions
         

#if UNITY_ASSERTIONS
        public string DebugName { get; private set; }
#endif

        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            return base.ToString();
        }

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public void SetDebugName(string name)
        {
#if UNITY_ASSERTIONS
            DebugName = name;
#endif
        }

         

        public string GetDescription(string separator = "\n")
        {
            var text = ObjectPool.AcquireStringBuilder();
            AppendDescription(text, separator);
            return text.ReleaseToString();
        }

         

        public void AppendDescription(StringBuilder text, string separator = "\n")
        {
            text.Append(ToString());

            AppendDetails(text, separator);

            if (ChildCount > 0)
            {
                text.Append(separator).Append($"{nameof(ChildCount)}: ").Append(ChildCount);
                var indentedSeparator = separator + Strings.Indent;

                var i = 0;
                foreach (var child in this)
                {
                    text.Append(separator).Append('[').Append(i++).Append("] ");
                    child.AppendDescription(text, indentedSeparator);
                }
            }
        }

         

        protected virtual void AppendDetails(StringBuilder text, string separator)
        {
            text.Append(separator).Append("Playable: ");
            if (_Playable.IsValid())
                text.Append(_Playable.GetPlayableType());
            else
                text.Append("Invalid");

            text.Append(separator).Append($"{nameof(Index)}: ").Append(Index);

            var realSpeed = _Playable.IsValid() ? _Playable.GetSpeed() : _Speed;
            if (realSpeed == _Speed)
            {
                text.Append(separator).Append($"{nameof(Speed)}: ").Append(_Speed);
            }
            else
            {
                text.Append(separator).Append($"{nameof(Speed)} (Real): ").Append(_Speed)
                    .Append(" (").Append(realSpeed).Append(')');
            }

            text.Append(separator).Append($"{nameof(Weight)}: ").Append(Weight);

            if (Weight != TargetWeight)
            {
                text.Append(separator).Append($"{nameof(TargetWeight)}: ").Append(TargetWeight);
                text.Append(separator).Append($"{nameof(FadeSpeed)}: ").Append(FadeSpeed);
            }

            AppendIKDetails(text, separator, this);
        }

         

        public static void AppendIKDetails(StringBuilder text, string separator, IPlayableWrapper node)
        {
            text.Append(separator).Append("InverseKinematics: ");
            if (node.ApplyAnimatorIK)
            {
                text.Append("OnAnimatorIK");
                if (node.ApplyFootIK)
                    text.Append(", FootIK");
            }
            else if (node.ApplyFootIK)
            {
                text.Append("FootIK");
            }
            else
            {
                text.Append("None");
            }
        }

         
        #endregion
         
    }
}

