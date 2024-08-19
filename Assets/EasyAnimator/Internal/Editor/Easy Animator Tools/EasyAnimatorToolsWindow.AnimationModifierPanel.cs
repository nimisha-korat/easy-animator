

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
    partial class EasyAnimatorToolsWindow
    {
        
        [Serializable]
        public abstract class AnimationModifierPanel : Panel
        {
             

            [SerializeField]
            private AnimationClip _Animation;

            public AnimationClip Animation => _Animation;

             

            public override void OnEnable(int index)
            {
                base.OnEnable(index);
                OnAnimationChanged();
            }

             

            public override void OnSelectionChanged()
            {
                if (Selection.activeObject is AnimationClip animation)
                {
                    _Animation = animation;
                    OnAnimationChanged();
                }
            }

             

            protected virtual void OnAnimationChanged() { }

             

            public override void DoBodyGUI()
            {
                BeginChangeCheck();
                var animation = (AnimationClip)EditorGUILayout.ObjectField("Animation", _Animation, typeof(AnimationClip), false);
                if (EndChangeCheck(ref _Animation, animation))
                    OnAnimationChanged();
            }

             

            protected bool SaveAs()
            {
                EasyAnimatorGUI.Deselect();

                if (SaveModifiedAsset(
                    "Save Modified Animation",
                    "Where would you like to save the new animation?",
                    _Animation,
                    Modify))
                {
                    _Animation = null;
                    OnAnimationChanged();
                    return true;
                }
                else return false;
            }

             

            protected virtual void Modify(AnimationClip animation) { }

             
        }
    }
}

#endif

