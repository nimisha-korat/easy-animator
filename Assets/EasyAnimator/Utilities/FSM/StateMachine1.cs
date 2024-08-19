
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator.FSM
{
   
    [HelpURL(StateExtensions.APIDocumentationURL + nameof(StateMachine<TState>) + "_1")]
    public partial class StateMachine<TState> where TState : class, IState
    {
         

        public TState CurrentState { get; private set; }

         

        public TState PreviousState => StateChange<TState>.PreviousState;

        public TState NextState => StateChange<TState>.NextState;

         

        public StateMachine() { }

        public StateMachine(TState state)
        {
#if UNITY_ASSERTIONS
            if (state == null)// AllowNullStates won't be true yet since this is the constructor.
                throw new ArgumentNullException(nameof(state), NullNotAllowed);
#endif

            using (new StateChange<TState>(this, null, state))
            {
                CurrentState = state;
                state.OnEnterState();
            }
        }

         

        public bool CanSetState(TState state)
        {
#if UNITY_ASSERTIONS
            if (state == null && !AllowNullStates)
                throw new ArgumentNullException(nameof(state), NullNotAllowed);
#endif

            using (new StateChange<TState>(this, CurrentState, state))
            {
                if (CurrentState != null && !CurrentState.CanExitState)
                    return false;

                if (state != null && !state.CanEnterState)
                    return false;

                return true;
            }
        }

        public TState CanSetState(IList<TState> states)
        {
            // We call CanSetState so that it will check CanExitState for each individual pair in case it does
            // something based on the next state.

            var count = states.Count;
            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (CanSetState(state))
                    return state;
            }

            return null;
        }

         

        public bool TrySetState(TState state)
        {
            if (CurrentState == state)
            {
#if UNITY_ASSERTIONS
                if (state == null && !AllowNullStates)
                    throw new ArgumentNullException(nameof(state), NullNotAllowed);
#endif

                return true;
            }

            return TryResetState(state);
        }

        public bool TrySetState(IList<TState> states)
        {
            var count = states.Count;
            for (int i = 0; i < count; i++)
                if (TrySetState(states[i]))
                    return true;

            return false;
        }

         

        public bool TryResetState(TState state)
        {
            if (!CanSetState(state))
                return false;

            ForceSetState(state);
            return true;
        }

        public bool TryResetState(IList<TState> states)
        {
            var count = states.Count;
            for (int i = 0; i < count; i++)
                if (TryResetState(states[i]))
                    return true;

            return false;
        }

         

        public void ForceSetState(TState state)
        {
#if UNITY_ASSERTIONS
            if (state == null)
            {
                if (!AllowNullStates)
                    throw new ArgumentNullException(nameof(state), NullNotAllowed);
            }
            else if (state is IOwnedState<TState> owned && owned.OwnerStateMachine != this)
            {
                throw new InvalidOperationException(
                    $"Attempted to use a state in a machine that is not its owner." +
                    $"\n    State: {state}" +
                    $"\n    Machine: {this}");
            }
#endif

            using (new StateChange<TState>(this, CurrentState, state))
            {
                CurrentState?.OnExitState();

                CurrentState = state;

                state?.OnEnterState();
            }
        }

         

        public override string ToString() => $"{GetType().Name} -> {CurrentState}";

         

#if UNITY_ASSERTIONS
        public bool AllowNullStates { get; private set; }

        private const string NullNotAllowed =
            "This " + nameof(StateMachine<TState>) + " does not allow its state to be set to null." +
            " Use " + nameof(SetAllowNullStates) + " to allow it if this is intentional.";
#endif

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public void SetAllowNullStates(bool allow = true)
        {
#if UNITY_ASSERTIONS
            AllowNullStates = allow;
#endif
        }

         
    }
}
