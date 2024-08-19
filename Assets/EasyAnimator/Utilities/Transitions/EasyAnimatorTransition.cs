
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using EasyAnimator.Units;

namespace EasyAnimator
{
    
    [Serializable]
    public abstract class EasyAnimatorTransition<TState> :
        ITransition<TState>, ITransitionDetailed, ITransitionWithEvents
        where TState : EasyAnimatorState
    {
         

        [SerializeField]
        [Tooltip(Strings.Tooltips.FadeDuration)]
        [AnimationTime(AnimationTimeAttribute.Units.Seconds, Rule = Validate.Value.IsNotNegative)]
        [DefaultFadeValue]
        private float _FadeDuration = EasyAnimatorPlayable.DefaultFadeDuration;

        public float FadeDuration
        {
            get => _FadeDuration;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(FadeDuration)} must not be negative");

                _FadeDuration = value;
            }
        }

         

        public virtual bool IsLooping => false;

        public virtual float NormalizedStartTime
        {
            get => float.NaN;
            set { }
        }

        public virtual float Speed
        {
            get => 1;
            set { }
        }

        public abstract float MaximumDuration { get; }

         

        [SerializeField, Tooltip(Strings.ProOnlyTag + "Events which will be triggered as the animation plays")]
        private EasyAnimatorEvent.Sequence.Serializable _Events;

        public EasyAnimatorEvent.Sequence Events
        {
            get
            {
#if UNITY_ASSERTIONS
                if (_Events == null)
                    throw new NullReferenceException(
                        $"{nameof(EasyAnimatorTransition<TState>)}.{nameof(SerializedEvents)} is null.");
#endif

                return _Events.Events;
            }
        }

        public ref EasyAnimatorEvent.Sequence.Serializable SerializedEvents => ref _Events;

         

        public EasyAnimatorState BaseState { get; private set; }

         

        private TState _State;

        public TState State
        {
            get
            {
                if (_State == null)
                    _State = (TState)BaseState;

                return _State;
            }
            protected set
            {
                BaseState = _State = value;
            }
        }

         

        public virtual bool IsValid => true;

        public virtual object Key => this;

        public virtual FadeMode FadeMode => FadeMode.FixedSpeed;

        public abstract TState CreateState();

        EasyAnimatorState ITransition.CreateState() => CreateState();

         

        public virtual void Apply(EasyAnimatorState state)
        {
            state.Events = _Events;

            BaseState = state;

            if (_State != state)
                _State = null;
        }

         

        public virtual Object MainObject { get; }

        public virtual string Name
        {
            get
            {
                var mainObject = MainObject;
                return mainObject != null ? mainObject.name : null;
            }
        }

        public override string ToString()
        {
            var type = GetType().FullName;

            var name = Name;
            if (name != null)
                return $"{name} ({type})";
            else
                return type;
        }

         

        public static void ApplyDetails(EasyAnimatorState state, float speed, float normalizedStartTime)
        {
            if (!float.IsNaN(speed))
                state.Speed = speed;

            if (!float.IsNaN(normalizedStartTime))
                state.NormalizedTime = normalizedStartTime;
            else if (state.Weight == 0)
                state.NormalizedTime = EasyAnimatorEvent.Sequence.GetDefaultNormalizedStartTime(speed);
        }

         
    }
}
