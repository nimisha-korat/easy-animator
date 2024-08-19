

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using EasyAnimator.Editor;
#endif

namespace EasyAnimator
{
  
    public abstract partial class EasyAnimatorState : EasyAnimatorNode, IAnimationClipCollection
    {
         
        #region Graph
         

        public void SetRoot(EasyAnimatorPlayable root)
        {
            if (Root == root)
                return;

            if (Root != null)
            {
                Root.CancelPreUpdate(this);
                Root.States.Unregister(this);

                if (_EventDispatcher != null)
                    Root.CancelPostUpdate(_EventDispatcher);

                if (_Parent != null)
                {
                    _Parent.OnRemoveChild(this);
                    _Parent = null;
                }

                Index = -1;

                DestroyPlayable();
            }

            Root = root;

            if (root != null)
            {
#if UNITY_ASSERTIONS
                GC.SuppressFinalize(this);
#endif

                if (_EventDispatcher != null)
                    root.RequirePostUpdate(_EventDispatcher);

                root.States.Register(_Key, this);
                CreatePlayable();
            }

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i)?.SetRoot(root);

            if (_Parent != null)
                CopyIKFlags(_Parent);
        }

         

        private EasyAnimatorNode _Parent;

        public sealed override IPlayableWrapper Parent => _Parent;

      
        public void SetParent(EasyAnimatorNode parent, int index)
        {
            if (_Parent != null)
            {
                _Parent.OnRemoveChild(this);
                _Parent = null;
            }

            if (parent == null)
            {
                Index = -1;
                return;
            }

            SetRoot(parent.Root);
            Index = index;
            _Parent = parent;
            parent.OnAddChild(this);
            CopyIKFlags(parent);
        }

       
        internal void ClearParent()
        {
            Index = -1;
            _Parent = null;
        }

         
        // Layer.
         

        public override EasyAnimatorLayer Layer => _Parent?.Layer;

       
        public int LayerIndex
        {
            get
            {
                if (_Parent == null)
                    return -1;

                var layer = _Parent.Layer;
                if (layer == null)
                    return -1;

                return layer.Index;
            }
            set
            {
                Root.Layers[value].AddChild(this);
            }
        }

         
        #endregion
         
        #region Key and Clip
         

        internal object _Key;

       
        public object Key
        {
            get => _Key;
            set
            {
                Root.States.Unregister(this);
                Root.States.Register(value, this);
            }
        }

         

       
        public virtual AnimationClip Clip
        {
            get => null;
            set => throw new NotSupportedException($"{GetType()} does not support setting the {nameof(Clip)}.");
        }

       
        public virtual Object MainObject
        {
            get => null;
            set => throw new NotSupportedException($"{GetType()} does not support setting the {nameof(MainObject)}.");
        }

         

       
        protected void ChangeMainObject<T>(ref T currentObject, T newObject) where T : Object
        {
            if (newObject == null)
                throw new ArgumentNullException(nameof(newObject));

            if (ReferenceEquals(currentObject, newObject))
                return;

            if (ReferenceEquals(_Key, currentObject))
                Key = newObject;

            currentObject = newObject;
            RecreatePlayable();
        }

         

        public virtual Vector3 AverageVelocity => default;

         
        #endregion
         
        #region Playing
         

        private bool _IsPlaying;

      
        private bool _IsPlayingDirty = true;

         

        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                if (_IsPlaying == value)
                    return;

                _IsPlaying = value;

                // If it was already dirty then we just returned to the previous state so it is no longer dirty.
                if (_IsPlayingDirty)
                {
                    _IsPlayingDirty = false;
                    // We may still need to be updated for other reasons (such as Weight),
                    // but if not then we will be removed from the update list next update.
                }
                else// Otherwise we are now dirty so we need to be updated.
                {
                    _IsPlayingDirty = true;
                    RequireUpdate();
                }

                OnSetIsPlaying();
            }
        }

        protected virtual void OnSetIsPlaying() { }

        public sealed override void CreatePlayable()
        {
            base.CreatePlayable();

            if (_MustSetTime)
            {
                _MustSetTime = false;
                RawTime = _Time;
            }

            if (!_IsPlaying)
                _Playable.Pause();
            _IsPlayingDirty = false;
        }

         

        public bool IsActive => _IsPlaying && TargetWeight > 0;

        public bool IsStopped => !_IsPlaying && Weight == 0;

         

      
        public void Play()
        {
            IsPlaying = true;
            Weight = 1;
            if (AutomaticallyClearEvents)
                EventDispatcher.TryClear(_EventDispatcher);
        }

         

       
        public override void Stop()
        {
            base.Stop();

            IsPlaying = false;
            Time = 0;
            if (AutomaticallyClearEvents)
                EventDispatcher.TryClear(_EventDispatcher);
        }

         

    
        protected internal override void OnStartFade()
        {
            if (AutomaticallyClearEvents)
                EventDispatcher.TryClear(_EventDispatcher);
        }

         
        #endregion
         
        #region Timing
         
        // Time.
         

       
        private float _Time;

        
        private bool _MustSetTime;

       
        private ulong _TimeFrameID;

         

      
        public float Time
        {
            get
            {
                var root = Root;
                if (root == null || _MustSetTime)
                    return _Time;

                var frameID = root.FrameID;
                if (_TimeFrameID != frameID)
                {
                    _TimeFrameID = frameID;
                    _Time = RawTime;
                }

                return _Time;
            }
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Time)} must be finite");
#endif

                _Time = value;

                var root = Root;
                if (root == null)
                {
                    _MustSetTime = true;
                }
                else
                {
                    _TimeFrameID = root.FrameID;
                    if (EasyAnimatorPlayable.IsRunningPostUpdate(root))
                    {
                        _MustSetTime = true;
                        root.RequirePreUpdate(this);
                    }
                    else
                    {
                        RawTime = value;
                    }
                }

                _EventDispatcher?.OnTimeChanged();
            }
        }

         

       
        protected virtual float RawTime
        {
            get
            {
                Validate.AssertPlayable(this);
                return (float)_Playable.GetTime();
            }
            set
            {
                Validate.AssertPlayable(this);
                var time = (double)value;
                _Playable.SetTime(time);
                _Playable.SetTime(time);
            }
        }

         

      
        public float NormalizedTime
        {
            get
            {
                var length = Length;
                if (length != 0)
                    return Time / Length;
                else
                    return 0;
            }
            set => Time = value * Length;
        }

         

        
        public virtual void MoveTime(float time, bool normalized)
        {
#if UNITY_ASSERTIONS
            if (!time.IsFinite())
                throw new ArgumentOutOfRangeException(nameof(time), time, $"{nameof(Time)} must be finite");
#endif

            var root = Root;
            if (root != null)
                _TimeFrameID = root.FrameID;

            if (normalized)
                time *= Length;

            _Time = time;
            _Playable.SetTime(time);
        }

         

        protected void CancelSetTime() => _MustSetTime = false;

         
        // Duration.
         

       
        public float NormalizedEndTime
        {
            get
            {
                if (_EventDispatcher != null)
                {
                    var time = _EventDispatcher.Events.NormalizedEndTime;
                    if (!float.IsNaN(time))
                        return time;
                }

                return EasyAnimatorEvent.Sequence.GetDefaultNormalizedEndTime(EffectiveSpeed);
            }
            set => Events.NormalizedEndTime = value;
        }

         

      
        public float Duration
        {
            get
            {
                var speed = EffectiveSpeed;
                if (_EventDispatcher != null)
                {
                    var endTime = _EventDispatcher.Events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (speed > 0)
                            return Length * endTime / speed;
                        else
                            return Length * (1 - endTime) / -speed;
                    }
                }

                return Length / Math.Abs(speed);
            }
            set
            {
                var length = Length;
                if (_EventDispatcher != null)
                {
                    var endTime = _EventDispatcher.Events.NormalizedEndTime;
                    if (!float.IsNaN(endTime))
                    {
                        if (EffectiveSpeed > 0)
                            length *= endTime;
                        else
                            length *= 1 - endTime;
                    }
                }

                EffectiveSpeed = length / value;
            }
        }

         

       
        public float RemainingDuration
        {
            get => (Length * NormalizedEndTime - Time) / EffectiveSpeed;
            set => EffectiveSpeed = (Length * NormalizedEndTime - Time) / value;
        }

         
        // Length.
         

        public abstract float Length { get; }

        public virtual bool IsLooping => false;

         
        #endregion
         
        #region Methods
         

       
        protected internal override void Update(out bool needsMoreUpdates)
        {
            base.Update(out needsMoreUpdates);

            if (_IsPlayingDirty)
            {
                _IsPlayingDirty = false;

                if (_IsPlaying)
                    _Playable.Play();
                else
                    _Playable.Pause();
            }

            if (_MustSetTime)
            {
                _MustSetTime = false;
                RawTime = _Time;
            }
        }

         

        
        public virtual void Destroy()
        {
            if (_Parent != null)
            {
                _Parent.OnRemoveChild(this);
                _Parent = null;
            }

            Index = -1;
            EventDispatcher.TryClear(_EventDispatcher);

            var root = Root;
            if (root != null)
            {
                root.States.Unregister(this);

                // For some reason this is slightly faster than _Playable.Destroy().
                if (_Playable.IsValid())
                    root._Graph.DestroyPlayable(_Playable);
            }
        }

         

        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            clips.Gather(Clip);

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i).GatherAnimationClips(clips);
        }

         

       
        protected internal override bool IsPlayingAndNotEnding()
        {
            if (!IsPlaying)
                return false;

            var speed = EffectiveSpeed;
            if (speed > 0)
            {
                float endTime;
                if (_EventDispatcher != null)
                {
                    endTime = _EventDispatcher.Events.endEvent.normalizedTime;
                    if (float.IsNaN(endTime))
                        endTime = Length;
                    else
                        endTime *= Length;
                }
                else endTime = Length;

                return Time <= endTime;
            }
            else if (speed < 0)
            {
                float endTime;
                if (_EventDispatcher != null)
                {
                    endTime = _EventDispatcher.Events.endEvent.normalizedTime;
                    if (float.IsNaN(endTime))
                        endTime = 0;
                    else
                        endTime *= Length;
                }
                else endTime = 0;

                return Time >= endTime;
            }
            else return true;
        }

         

       
        public override string ToString()
        {
#if UNITY_ASSERTIONS
            if (DebugName != null)
                return DebugName;
#endif

            var type = GetType().Name;
            var mainObject = MainObject;
            if (mainObject != null)
                return $"{mainObject.name} ({type})";
            else
                return type;
        }

         
        #region Descriptions
         

#if UNITY_EDITOR
        protected internal virtual IEasyAnimatorNodeDrawer CreateDrawer()
            => new EasyAnimatorStateDrawer<EasyAnimatorState>(this);
#endif

         

        protected override void AppendDetails(StringBuilder text, string separator)
        {
            text.Append(separator).Append($"{nameof(Key)}: ").Append(EasyAnimatorUtilities.ToStringOrNull(_Key));

            var mainObject = MainObject;
            if (mainObject != _Key as Object)
                text.Append(separator).Append($"{nameof(MainObject)}: ").Append(EasyAnimatorUtilities.ToStringOrNull(mainObject));

#if UNITY_EDITOR
            if (mainObject != null)
                text.Append(separator).Append("AssetPath: ").Append(AssetDatabase.GetAssetPath(mainObject));
#endif

            base.AppendDetails(text, separator);

            text.Append(separator).Append($"{nameof(IsPlaying)}: ").Append(IsPlaying);

            try
            {
                text.Append(separator).Append($"{nameof(Time)} (Normalized): ").Append(Time);
                text.Append(" (").Append(NormalizedTime).Append(')');
                text.Append(separator).Append($"{nameof(Length)}: ").Append(Length);
                text.Append(separator).Append($"{nameof(IsLooping)}: ").Append(IsLooping);
            }
            catch (Exception exception)
            {
                text.Append(separator).Append(exception);
            }

            text.Append(separator).Append($"{nameof(Events)}: ");
            if (_EventDispatcher != null && _EventDispatcher.Events != null)
                text.Append(_EventDispatcher.Events.DeepToString(false));
            else
                text.Append("null");
        }

         

        public string GetPath()
        {
            if (_Parent == null)
                return null;

            var path = ObjectPool.AcquireStringBuilder();

            AppendPath(path, _Parent);
            AppendPortAndType(path);

            return path.ReleaseToString();
        }

        private static void AppendPath(StringBuilder path, EasyAnimatorNode parent)
        {
            var parentState = parent as EasyAnimatorState;
            if (parentState != null && parentState._Parent != null)
            {
                AppendPath(path, parentState._Parent);
            }
            else
            {
                path.Append("Layers[")
                    .Append(parent.Layer.Index)
                    .Append("].States");
                return;
            }

            var state = parent as EasyAnimatorState;
            if (state != null)
            {
                state.AppendPortAndType(path);
            }
            else
            {
                path.Append(" -> ")
                    .Append(parent.GetType());
            }
        }

        private void AppendPortAndType(StringBuilder path)
        {
            path.Append('[')
                .Append(Index)
                .Append("] -> ")
                .Append(GetType().Name);
        }

         
        #endregion
         
        #endregion
         
    }
}

