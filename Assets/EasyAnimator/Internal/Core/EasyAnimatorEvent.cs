

using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
    
    public partial struct EasyAnimatorEvent : IEquatable<EasyAnimatorEvent>
    {
         
        #region Event
         

        public float normalizedTime;

        public Action callback;

         

       
        public const float AlmostOne = 0.99999994f;

         

       
        public static readonly Action DummyCallback = Dummy;

        
        private static void Dummy() { }

        public static bool IsNullOrDummy(Action callback) => callback == null || callback == DummyCallback;

         

        public EasyAnimatorEvent(float normalizedTime, Action callback)
        {
            this.normalizedTime = normalizedTime;
            this.callback = callback;
        }

         

        public override string ToString()
        {
            var text = ObjectPool.AcquireStringBuilder();
            text.Append($"{nameof(EasyAnimatorEvent)}(");
            AppendDetails(text);
            text.Append(')');
            return text.ReleaseToString();
        }

         

        public void AppendDetails(StringBuilder text)
        {
            text.Append("NormalizedTime: ")
                .Append(normalizedTime)
                .Append(", Callback: ");

            if (callback == null)
            {
                text.Append("null");
            }
            else if (callback.Target == null)
            {
                text.Append(callback.Method.DeclaringType.FullName)
                    .Append('.')
                    .Append(callback.Method.Name);
            }
            else
            {
                text.Append("(Target: '")
                    .Append(callback.Target)
                    .Append("', Method: ")
                    .Append(callback.Method.DeclaringType.FullName)
                    .Append('.')
                    .Append(callback.Method.Name)
                    .Append(')');
            }
        }

         
        #endregion
         
        #region Invocation
         

        public static EasyAnimatorState CurrentState => _CurrentState;
        private static EasyAnimatorState _CurrentState;

         

        public static ref readonly EasyAnimatorEvent CurrentEvent => ref _CurrentEvent;
        private static EasyAnimatorEvent _CurrentEvent;

         

       
        public void Invoke(EasyAnimatorState state)
        {
#if UNITY_ASSERTIONS
            if (IsNullOrDummy(callback))
                OptionalWarning.UselessEvent.Log(
                    $"An {nameof(EasyAnimatorEvent)} that does nothing was invoked." +
                    " Most likely it was not configured correctly." +
                    " Unused events should be removed to avoid wasting performance checking and invoking them.",
                    state?.Root?.Component);
#endif

            var previousState = _CurrentState;
            var previousEvent = _CurrentEvent;

            _CurrentState = state;
            _CurrentEvent = this;

            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, state?.Root?.Component as Object);
            }

            _CurrentState = previousState;
            _CurrentEvent = previousEvent;
        }

         

      
        public static float GetFadeOutDuration(float minDuration)
            => GetFadeOutDuration(CurrentState, minDuration);

        
        public static float GetFadeOutDuration(EasyAnimatorState state, float minDuration)
        {
            if (state == null)
                return minDuration;

            var time = state.Time;
            var speed = state.EffectiveSpeed;
            if (speed == 0)
                return minDuration;

            float remainingDuration;
            if (state.IsLooping)
            {
                var previousTime = time - speed * Time.deltaTime;
                var inverseLength = 1f / state.Length;

                // If we just passed the end of the animation, the remaining duration would technically be the full
                // duration of the animation, so we most likely want to use the minimum duration instead.
                if (Math.Floor(time * inverseLength) != Math.Floor(previousTime * inverseLength))
                    return minDuration;
            }

            if (speed > 0)
            {
                remainingDuration = (state.Length - time) / speed;
            }
            else
            {
                remainingDuration = time / -speed;
            }

            return Math.Max(minDuration, remainingDuration);
        }

         
        #endregion
         
        #region Operators
         

        public static bool operator ==(EasyAnimatorEvent a, EasyAnimatorEvent b) =>
            a.normalizedTime == b.normalizedTime &&
            a.callback == b.callback;

        public static bool operator !=(EasyAnimatorEvent a, EasyAnimatorEvent b) => !(a == b);

         

       
        public bool Equals(EasyAnimatorEvent EasyAnimatorEvent) => this == EasyAnimatorEvent;

        public override bool Equals(object obj) => obj is EasyAnimatorEvent EasyAnimatorEvent && this == EasyAnimatorEvent;

        public override int GetHashCode()
        {
            const int Multiplyer = -1521134295;

            var hashCode = -78069441;
            hashCode = hashCode * Multiplyer + normalizedTime.GetHashCode();

            if (callback != null)
                hashCode = hashCode * Multiplyer + callback.GetHashCode();

            return hashCode;
        }

         
        #endregion
         
    }
}

