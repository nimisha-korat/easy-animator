

using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    public interface ITransition : IHasKey, IPolymorphic
    {
         

       
        EasyAnimatorState CreateState();

        float FadeDuration { get; }

        FadeMode FadeMode { get; }

       
        void Apply(EasyAnimatorState state);

         
    }

    
    public interface ITransition<TState> : ITransition where TState : EasyAnimatorState
    {
         

       
        TState State { get; }

         

        new TState CreateState();

         
    }
}

