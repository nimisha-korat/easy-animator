

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
   
    public interface ITransitionGUI
    {
         

        void OnPreviewSceneGUI(TransitionPreviewDetails details);

       
        void OnTimelineBackgroundGUI();

       
        void OnTimelineForegroundGUI();

         
    }
}

namespace EasyAnimator.Editor
{
    
    public readonly struct TransitionPreviewDetails
    {
         

        public readonly EasyAnimatorPlayable EasyAnimator;

        public Transform Transform => EasyAnimator.Component.Animator.transform;

         

        public static SerializedProperty Property => TransitionDrawer.Context.Property;

        public static ITransitionDetailed Transition => TransitionDrawer.Context.Transition;

         

        public TransitionPreviewDetails(EasyAnimatorPlayable Animator)
        {
            EasyAnimator = Animator;
        }

         
    }
}

#endif

