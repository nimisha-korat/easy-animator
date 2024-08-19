
using UnityEngine;

namespace EasyAnimator
{
 
    public class MixerParameterTweenFloat : MixerParameterTween<float>
    {
        public MixerParameterTweenFloat() { }
        public MixerParameterTweenFloat(MixerState<float> mixer) : base(mixer) { }

        protected override float CalculateCurrentValue() => Mathf.LerpUnclamped(StartValue, EndValue, Progress);
    }

     

  
    public class MixerParameterTweenVector2 : MixerParameterTween<Vector2>
    {
        public MixerParameterTweenVector2() { }
        public MixerParameterTweenVector2(MixerState<Vector2> mixer) : base(mixer) { }

        protected override Vector2 CalculateCurrentValue() => Vector2.LerpUnclamped(StartValue, EndValue, Progress);
    }

     

    public abstract class MixerParameterTween<TParameter> : Key, IUpdatable
    {
         

        public MixerState<TParameter> Mixer { get; set; }

         

        public TParameter StartValue { get; set; }

        public TParameter EndValue { get; set; }

         

        public float Duration { get; set; }

        public float Time { get; set; }

        public float Progress
        {
            get => Time / Duration;
            set => Time = value * Duration;
        }

         

        public MixerParameterTween() { }

        public MixerParameterTween(MixerState<TParameter> mixer) => Mixer = mixer;

         

        public void Start(TParameter endValue, float duration)
        {
#if UNITY_ASSERTIONS
            EasyAnimatorUtilities.Assert(Mixer != null, nameof(Mixer) + " is null.");
            EasyAnimatorUtilities.Assert(Mixer.Root != null, $"{nameof(Mixer)}.{nameof(Mixer.Root)} is null.");
#endif

            StartValue = Mixer.Parameter;
            EndValue = endValue;
            Duration = duration;
            Time = 0;

            Mixer.Root.RequirePreUpdate(this);
        }

         

        public void Stop() => Mixer?.Root?.CancelPreUpdate(this);

         

        public bool IsActive => IsInList(this);

         

        protected abstract TParameter CalculateCurrentValue();

         

        void IUpdatable.Update()
        {
            Time += EasyAnimatorPlayable.DeltaTime;

            if (Time < Duration)// Tween.
            {
                Mixer.Parameter = CalculateCurrentValue();
            }
            else// End.
            {
                Time = Duration;
                Mixer.Parameter = EndValue;
                Stop();
            }
        }

         
    }
}
