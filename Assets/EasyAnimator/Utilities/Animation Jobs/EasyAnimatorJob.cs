

using UnityEngine.Animations;

namespace EasyAnimator
{
   
    public abstract class EasyAnimatorJob<T> where T : struct, IAnimationJob
    {
         

        protected T _Job;

        protected AnimationScriptPlayable _Playable;

         

        protected void CreatePlayable(EasyAnimatorPlayable EasyAnimator)
        {
            _Playable = EasyAnimator.InsertOutputJob(_Job);
        }

         

        public virtual void Destroy()
        {
            EasyAnimatorUtilities.RemovePlayable(_Playable);
        }

         
    }
}
