
using UnityEngine;

namespace EasyAnimator
{
   
    public interface IEasyAnimatorComponent
    {
       
#pragma warning disable IDE1006 // Naming Styles.
       
        bool enabled { get; }

       
        GameObject gameObject { get; }

       
#pragma warning restore IDE1006 // Naming Styles.
       
        Animator Animator { get; set; }

        EasyAnimatorPlayable Playable { get; }

        bool IsPlayableInitialized { get; }

        bool ResetOnDisable { get; }

        
        AnimatorUpdateMode UpdateMode { get; set; }

         

        object GetKey(AnimationClip clip);

         
#if UNITY_EDITOR
         

        string AnimatorFieldName { get; }

       
        string ActionOnDisableFieldName { get; }

       
        AnimatorUpdateMode? InitialUpdateMode { get; }

         
#endif
         
    }
}

