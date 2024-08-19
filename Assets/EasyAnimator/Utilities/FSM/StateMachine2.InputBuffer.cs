
namespace EasyAnimator.FSM
{
    public partial class StateMachine<TKey, TState>
    {
        
        public new class InputBuffer : InputBuffer<StateMachine<TKey, TState>>
        {
             

            public TKey Key { get; set; }

             

            public InputBuffer() { }

            public InputBuffer(StateMachine<TKey, TState> stateMachine) : base(stateMachine) { }

             

            public bool Buffer(TKey key, float timeOut)
            {
                if (StateMachine.TryGetValue(key, out var state))
                {
                    Buffer(key, state, timeOut);
                    return true;
                }
                else return false;
            }

            public void Buffer(TKey key, TState state, float timeOut)
            {
                Key = key;
                Buffer(state, timeOut);
            }

             

            protected override bool TryEnterState()
                => StateMachine.TryResetState(Key, State);

             

            public override void Clear()
            {
                base.Clear();
                Key = default;
            }

             
        }
    }
}
