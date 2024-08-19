

namespace EasyAnimator
{
  
    public interface IHasEvents
    {
         

        EasyAnimatorEvent.Sequence Events { get; }

        ref EasyAnimatorEvent.Sequence.Serializable SerializedEvents { get; }

         
    }
 
    public interface ITransitionWithEvents : ITransition, IHasEvents { }
}

