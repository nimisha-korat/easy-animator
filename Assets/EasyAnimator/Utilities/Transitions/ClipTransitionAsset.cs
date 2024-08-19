
using EasyAnimator.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
    
    [CreateAssetMenu(menuName = Strings.MenuPrefix + "Clip Transition", order = Strings.AssetMenuOrder + 1)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(ClipTransitionAsset))]
    public class ClipTransitionAsset : EasyAnimatorTransitionAsset<ClipTransition>
    {
        [Serializable]
        public class UnShared :
            EasyAnimatorTransitionAsset.UnShared<ClipTransitionAsset, ClipTransition, ClipState>,
            ClipState.ITransition
        { }
    }

   
    [Serializable]
    public class ClipTransition : EasyAnimatorTransition<ClipState>, ClipState.ITransition, IMotion, IAnimationClipCollection
    {
         

        [SerializeField, Tooltip("The animation to play")]
        private AnimationClip _Clip;

        public AnimationClip Clip
        {
            get => _Clip;
            set
            {
#if UNITY_ASSERTIONS
                if (value != null)
                    Validate.AssertNotLegacy(value);
#endif

                _Clip = value;
            }
        }

        public override Object MainObject => _Clip;

        public override object Key => _Clip;

         

        [SerializeField]
        [Tooltip(Strings.Tooltips.OptionalSpeed)]
        [AnimationSpeed]
        [DefaultValue(1f, -1f)]
        private float _Speed = 1;

        public override float Speed
        {
            get => _Speed;
            set => _Speed = value;
        }

         

        [SerializeField]
        [Tooltip(Strings.Tooltips.NormalizedStartTime)]
        [AnimationTime(AnimationTimeAttribute.Units.Normalized)]
        [DefaultValue(0, float.NaN)]
        private float _NormalizedStartTime;

        public override float NormalizedStartTime
        {
            get => _NormalizedStartTime;
            set => _NormalizedStartTime = value;
        }

        public override FadeMode FadeMode => float.IsNaN(_NormalizedStartTime) ? FadeMode.FixedSpeed : FadeMode.FromStart;

         

        public override bool IsValid => _Clip != null && !_Clip.legacy;

        public override bool IsLooping => _Clip != null && _Clip.isLooping;

        public override float MaximumDuration => _Clip != null ? _Clip.length : 0;

        public virtual float AverageAngularSpeed => _Clip != null ? _Clip.averageAngularSpeed : default;

        public virtual Vector3 AverageVelocity => _Clip != null ? _Clip.averageSpeed : default;

         

        public override ClipState CreateState() => State = new ClipState(_Clip);

         

        public override void Apply(EasyAnimatorState state)
        {
            base.Apply(state);
            ApplyDetails(state, _Speed, _NormalizedStartTime);
        }

         

        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.Gather(_Clip);

         
#if UNITY_EDITOR
         

        [UnityEditor.CustomPropertyDrawer(typeof(ClipTransition), true)]
        public class Drawer : Editor.TransitionDrawer
        {
             

            public Drawer() : base(nameof(_Clip)) { }

             
        }

         
#endif
         
    }
}
