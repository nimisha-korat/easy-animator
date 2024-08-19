
using System;

namespace EasyAnimator.FSM
{
   
    partial class StateMachine<TState>
    {
       
        public class WithDefault : StateMachine<TState>
        {
             

            private TState _DefaultState;

           
            public TState DefaultState
            {
                get => _DefaultState;
                set
                {
                    _DefaultState = value;
                    if (CurrentState == null && value != null)
                        ForceSetState(value);
                }
            }

             

            public readonly Action ForceSetDefaultState;

             

            public WithDefault()
            {
                // Silly C# doesn't allow instance delegates to be assigned using field initializers.
                ForceSetDefaultState = () => ForceSetState(_DefaultState);
            }

             

            public WithDefault(TState defaultState)
                : this()
            {
                _DefaultState = defaultState;
                ForceSetState(defaultState);
            }

             

            public bool TrySetDefaultState() => TrySetState(DefaultState);

             

            public bool TryResetDefaultState() => TryResetState(DefaultState);

             
        }
    }
}
