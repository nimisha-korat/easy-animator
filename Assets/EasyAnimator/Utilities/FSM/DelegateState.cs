
using System;

namespace EasyAnimator.FSM
{
   
    public class DelegateState : IState
    {
         

        public Func<bool> canEnter;

        public virtual bool CanEnterState => canEnter == null || canEnter();

         

        public Func<bool> canExit;

        public virtual bool CanExitState => canExit == null || canExit();

         

        public Action onEnter;

        public virtual void OnEnterState() => onEnter?.Invoke();

         

        public Action onExit;

        public virtual void OnExitState() => onExit?.Invoke();

         
    }
}
