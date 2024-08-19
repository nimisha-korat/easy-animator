
using System;

namespace EasyAnimator
{
   
    public sealed class ExitEvent : Key, IUpdatable
    {
         

        private Action _Callback;
        private EasyAnimatorNode _Node;

         

        public static void Register(EasyAnimatorNode node, Action callback)
        {
#if UNITY_ASSERTIONS
            EasyAnimatorUtilities.Assert(node != null, "Node is null.");
            EasyAnimatorUtilities.Assert(node.IsValid, "Node is not valid.");
#endif

            var exit = ObjectPool.Acquire<ExitEvent>();
            exit._Callback = callback;
            exit._Node = node;
            node.Root.RequirePostUpdate(exit);
        }

         

        public static bool Unregister(EasyAnimatorPlayable EasyAnimator)
        {
            for (int i = EasyAnimator.PostUpdatableCount - 1; i >= 0; i--)
            {
                if (EasyAnimator.GetPostUpdatable(i) is ExitEvent exit)
                {
                    EasyAnimator.CancelPostUpdate(exit);
                    exit.Release();
                    return true;
                }
            }

            return false;
        }

        public static bool Unregister(EasyAnimatorNode node)
        {
            var EasyAnimator = node.Root;
            for (int i = EasyAnimator.PostUpdatableCount - 1; i >= 0; i--)
            {
                if (EasyAnimator.GetPostUpdatable(i) is ExitEvent exit &&
                    exit._Node == node)
                {
                    EasyAnimator.CancelPostUpdate(exit);
                    exit.Release();
                    return true;
                }
            }

            return false;
        }

         

        void IUpdatable.Update()
        {
            if (_Node.IsValid() && _Node.EffectiveWeight > 0)
                return;

            _Callback();
            _Node.Root.CancelPostUpdate(this);
            Release();
        }

         

        private void Release()
        {
            _Callback = null;
            _Node = null;
            ObjectPool.Release(this);
        }

         
    }
}
