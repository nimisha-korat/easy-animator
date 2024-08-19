
using System;

namespace EasyAnimator.FSM
{
    
    public struct KeyChange<TKey> : IDisposable
    {
         

        [ThreadStatic]
        private static KeyChange<TKey> _Current;

        private IKeyedStateMachine<TKey> _StateMachine;
        private TKey _PreviousKey;
        private TKey _NextKey;

         

        public static bool IsActive => _Current._StateMachine != null;

        public static IKeyedStateMachine<TKey> StateMachine => _Current._StateMachine;

         

        public static TKey PreviousKey
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TKey), typeof(StateMachine<,>), "Key"));
#endif
                return _Current._PreviousKey;
            }
        }

         

        public static TKey NextKey
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TKey), typeof(StateMachine<,>), "Key"));
#endif
                return _Current._NextKey;
            }
        }

         

        internal KeyChange(IKeyedStateMachine<TKey> stateMachine, TKey previousKey, TKey nextKey)
        {
            this = _Current;

            _Current._StateMachine = stateMachine;
            _Current._PreviousKey = previousKey;
            _Current._NextKey = nextKey;
        }

         

        public void Dispose()
        {
            _Current = this;
        }

         

        public override string ToString() => IsActive ?
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}" +
            $">({nameof(PreviousKey)}={PreviousKey}" +
            $", {nameof(NextKey)}={NextKey})" :
            $"{nameof(KeyChange<TKey>)}<{typeof(TKey).FullName}(Not Currently Active)";

        public static string CurrentToString() => _Current.ToString();

         
    }
}
