
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
 
    [AddComponentMenu(Strings.MenuPrefix + "Named EasyAnimator Component")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(NamedEasyAnimatorComponent))]
    public class NamedEasyAnimatorComponent : EasyAnimatorComponent
    {
         
        #region Fields and Properties
         

        [SerializeField, Tooltip("If true, the 'Default Animation' will be automatically played by OnEnable")]
        private bool _PlayAutomatically = true;

        public ref bool PlayAutomatically => ref _PlayAutomatically;

         

        [SerializeField, Tooltip("Animations in this array will be automatically registered by Awake" +
            " as states that can be retrieved using their name")]
        private AnimationClip[] _Animations;

        public AnimationClip[] Animations
        {
            get => _Animations;
            set
            {
                _Animations = value;
                States.CreateIfNew(value);
            }
        }

         

        public AnimationClip DefaultAnimation
        {
            get => _Animations.IsNullOrEmpty() ? null : _Animations[0];
            set
            {
                if (_Animations.IsNullOrEmpty())
                    _Animations = new AnimationClip[] { value };
                else
                    _Animations[0] = value;
            }
        }

         
        #endregion
         
        #region Methods
         

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (_Animations == null)
                return;

            for (int i = 0; i < _Animations.Length; i++)
            {
                var clip = _Animations[i];
                if (clip == null)
                    continue;

                try
                {
                    Validate.AssertNotLegacy(clip);
                    continue;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, clip);
                }

                Array.Copy(_Animations, i + 1, _Animations, i, _Animations.Length - (i + 1));
                Array.Resize(ref _Animations, _Animations.Length - 1);
                i--;
            }
        }
#endif

         

        protected virtual void Awake()
        {
            States.CreateIfNew(_Animations);
        }

         

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_PlayAutomatically && !_Animations.IsNullOrEmpty())
            {
                var clip = _Animations[0];
                if (clip != null)
                    Play(clip);
            }
        }

         

        public override object GetKey(AnimationClip clip) => clip.name;

         

        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            base.GatherAnimationClips(clips);
            clips.Gather(_Animations);
        }

         
        #endregion
         
    }
}
