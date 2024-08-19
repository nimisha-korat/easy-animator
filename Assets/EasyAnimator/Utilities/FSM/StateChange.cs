

using System;

namespace EasyAnimator.FSM
{

    public struct StateChange<TState> : IDisposable where TState : class, IState
    {
         

        [ThreadStatic]
        private static StateChange<TState> _Current;

        private StateMachine<TState> _StateMachine;
        private TState _PreviousState;
        private TState _NextState;

         

        public static bool IsActive => _Current._StateMachine != null;

        public static StateMachine<TState> StateMachine => _Current._StateMachine;

         

        public static TState PreviousState
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TState), typeof(StateMachine<>)));
#endif
                return _Current._PreviousState;
            }
        }

         

        public static TState NextState
        {
            get
            {
#if UNITY_ASSERTIONS
                if (!IsActive)
                    throw new InvalidOperationException(StateExtensions.GetChangeError(typeof(TState), typeof(StateMachine<>)));
#endif
                return _Current._NextState;
            }
        }

         

        internal StateChange(StateMachine<TState> stateMachine, TState previousState, TState nextState)
        {
            this = _Current;

            _Current._StateMachine = stateMachine;
            _Current._PreviousState = previousState;
            _Current._NextState = nextState;
        }

         

        public void Dispose()
        {
            _Current = this;
        }

         

        public override string ToString() => IsActive ?
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}" +
            $">({nameof(PreviousState)}='{_PreviousState}'" +
            $", {nameof(NextState)}='{_NextState}')" :
            $"{nameof(StateChange<TState>)}<{typeof(TState).FullName}(Not Currently Active)";

        public static string CurrentToString() => _Current.ToString();

         
    }
}
