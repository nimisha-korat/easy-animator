
using UnityEngine;

namespace EasyAnimator.FSM
{
    public partial class StateMachine<TState>
    {
    
        public class InputBuffer : InputBuffer<StateMachine<TState>>
        {
             

            public InputBuffer() { }

            public InputBuffer(StateMachine<TState> stateMachine) : base(stateMachine) { }

             
        }

        public class InputBuffer<TStateMachine> where TStateMachine : StateMachine<TState>
        {
             

            private TStateMachine _StateMachine;

            public TStateMachine StateMachine
            {
                get => _StateMachine;
                set
                {
                    _StateMachine = value;
                    Clear();
                }
            }

            public TState State { get; set; }

            public float TimeOut { get; set; }

             

            public bool IsActive => State != null;

             

            public InputBuffer() { }

            public InputBuffer(TStateMachine stateMachine) => _StateMachine = stateMachine;

             

            public void Buffer(TState state, float timeOut)
            {
                State = state;
                TimeOut = timeOut;
            }

             

            protected virtual bool TryEnterState() => StateMachine.TryResetState(State);

             

            public bool Update() => Update(Time.deltaTime);

            public bool Update(float deltaTime)
            {
                if (IsActive)
                {
                    if (TryEnterState())
                    {
                        Clear();
                        return true;
                    }
                    else
                    {
                        TimeOut -= deltaTime;

                        if (TimeOut < 0)
                            Clear();
                    }
                }

                return false;
            }

             

            public virtual void Clear()
            {
                State = null;
                TimeOut = default;
            }

             
        }
    }
}
