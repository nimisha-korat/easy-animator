
using System.Collections.Generic;

namespace EasyAnimator.FSM
{
    
    public interface IPrioritizable : IState
    {
        float Priority { get; }
    }

     

    public partial class StateMachine<TState>
    {
      
        public class StateSelector : SortedList<float, TState>
        {
            public StateSelector() : base(ReverseComparer<float>.Instance) { }

            public void Add<TPrioritizable>(TPrioritizable state)
                where TPrioritizable : TState, IPrioritizable
                => Add(state.Priority, state);
        }
    }

     

    
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        public static readonly ReverseComparer<T> Instance = new ReverseComparer<T>();

        private ReverseComparer() { }

        public int Compare(T x, T y) => Comparer<T>.Default.Compare(y, x);
    }
}
