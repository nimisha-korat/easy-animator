

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
     
    [Serializable]
    public class ClipTransitionSequence : ClipTransition, ISerializationCallbackReceiver
    {
         

        [DrawAfterEvents]
        [SerializeField]
        [Tooltip("The other transitions to play in order after the first one.")]
        private ClipTransition[] _Others = Array.Empty<ClipTransition>();

        public ref ClipTransition[] Others => ref _Others;

         

        public ref EasyAnimatorEvent EndEvent
        {
            get
            {
                if (_Others.Length > 0)
                    return ref _Others[_Others.Length - 1].Events.endEvent;
                else
                    return ref Events.endEvent;
            }
        }

         

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_Others.Length == 0)
                return;

            // Assign each of the other end events, but this first one will be set by Apply.

            var previous = _Others[0];
            for (int i = 1; i < _Others.Length; i++)
            {
                var next = _Others[i];
                previous.Events.OnEnd = () => EasyAnimatorEvent.CurrentState.Layer.Play(next);
                previous = next;
            }
        }

         

        private Action _OnEnd;

        public override void Apply(EasyAnimatorState state)
        {
            // If an end event is assigned other than the one to play the next transition,
            // replace it and move it to be the end event of the last transition instead.
            if (_Others.Length > 0)
            {
                if (_OnEnd == null)
                    _OnEnd = () => EasyAnimatorEvent.CurrentState.Layer.Play(_Others[0]);

                var onEnd = Events.OnEnd;
                if (onEnd != _OnEnd)
                {
                    Events.OnEnd = _OnEnd;
                    onEnd -= _OnEnd;
                    _Others[_Others.Length - 1].Events.OnEnd = onEnd;
                }
            }

            base.Apply(state);
        }

         

        public override bool IsValid
        {
            get
            {
                if (!base.IsValid)
                    return false;

                for (int i = 0; i < _Others.Length; i++)
                    if (!_Others[i].IsValid)
                        return false;

                return true;
            }
        }

         

        public override bool IsLooping => _Others.Length != 0 ? _Others[_Others.Length - 1].IsLooping : base.IsLooping;

         

        public override float MaximumDuration
        {
            get
            {
                var value = base.MaximumDuration;
                for (int i = 0; i < _Others.Length; i++)
                    value += _Others[i].MaximumDuration;
                return value;
            }
        }

         

        public override float AverageAngularSpeed
        {
            get
            {
                var speed = base.AverageAngularSpeed;
                if (_Others.Length == 0)
                    return speed;

                var duration = base.MaximumDuration;
                speed *= duration;

                for (int i = 0; i < _Others.Length; i++)
                {
                    var other = _Others[i];
                    var otherSpeed = other.AverageAngularSpeed;
                    var otherDuration = other.MaximumDuration;
                    speed += otherSpeed * otherDuration;
                    duration += otherDuration;
                }

                return speed / duration;
            }
        }

         

        public override Vector3 AverageVelocity
        {
            get
            {
                var velocity = base.AverageVelocity;

                if (_Others.Length == 0)
                    return velocity;

                var duration = base.MaximumDuration;
                velocity *= duration;

                for (int i = 0; i < _Others.Length; i++)
                {
                    var other = _Others[i];
                    var otherVelocity = other.AverageVelocity;
                    var otherDuration = other.MaximumDuration;
                    velocity += otherVelocity * otherDuration;
                    duration += otherDuration;
                }

                return velocity / duration;
            }
        }

         

        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            base.GatherAnimationClips(clips);
            for (int i = 0; i < _Others.Length; i++)
                _Others[i].GatherAnimationClips(clips);
        }

         
    }
}
