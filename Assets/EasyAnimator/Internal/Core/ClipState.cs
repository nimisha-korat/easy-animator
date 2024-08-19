

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    public sealed class ClipState : EasyAnimatorState
    {
         

        public interface ITransition : ITransition<ClipState> { }

         
        #region Fields and Properties
         

        private AnimationClip _Clip;

        public override AnimationClip Clip
        {
            get => _Clip;
            set
            {
                Validate.AssertNotLegacy(value);
                ChangeMainObject(ref _Clip, value);
            }
        }

        public override Object MainObject
        {
            get => _Clip;
            set => Clip = (AnimationClip)value;
        }

         

        public override float Length => _Clip.length;

         

        public override bool IsLooping => _Clip.isLooping;

         

        public override Vector3 AverageVelocity => _Clip.averageSpeed;

         
        #region Inverse Kinematics
         

        public override bool ApplyAnimatorIK
        {
            get
            {
                Validate.AssertPlayable(this);
                return ((AnimationClipPlayable)_Playable).GetApplyPlayableIK();
            }
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyPlayableIK(value);
            }
        }

         

        public override bool ApplyFootIK
        {
            get
            {
                Validate.AssertPlayable(this);
                return ((AnimationClipPlayable)_Playable).GetApplyFootIK();
            }
            set
            {
                Validate.AssertPlayable(this);
                ((AnimationClipPlayable)_Playable).SetApplyFootIK(value);
            }
        }

         
        #endregion
         
        #endregion
         
        #region Methods
         

      
        public ClipState(AnimationClip clip)
        {
            Validate.AssertNotLegacy(clip);
            _Clip = clip;
        }

         

      
        protected override void CreatePlayable(out Playable playable)
        {
            var clipPlayable = AnimationClipPlayable.Create(Root._Graph, _Clip);
            playable = clipPlayable;
        }

         

      
        public override void Destroy()
        {
            _Clip = null;
            base.Destroy();
        }

         
        #endregion
         
        #region Inspector
#if UNITY_EDITOR
         

        protected internal override Editor.IEasyAnimatorNodeDrawer CreateDrawer() => new Drawer(this);

         

        public sealed class Drawer : Editor.EasyAnimatorStateDrawer<ClipState>
        {
             

            public Drawer(ClipState state) : base(state) { }

             

            protected override void AddContextMenuFunctions(UnityEditor.GenericMenu menu)
            {
                menu.AddDisabledItem(new GUIContent(DetailsPrefix + "Animation Type: " +
                    Editor.AnimationBindings.GetAnimationType(Target._Clip)));

                base.AddContextMenuFunctions(menu);

                Editor.EasyAnimatorEditorUtilities.AddContextMenuIK(menu, Target);
            }

             
        }

         
#endif
        #endregion
         
    }
}

