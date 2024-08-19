

using System;
using System.Collections.Generic;
using EasyAnimator.Units;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EasyAnimator
{
  
    [AddComponentMenu(Strings.MenuPrefix + "Solo Animation")]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(SoloAnimation))]
    public sealed class SoloAnimation : MonoBehaviour, IAnimationClipSource
    {
         
        #region Fields and Properties
         

        public const int DefaultExecutionOrder = -5000;

         

        [SerializeField, Tooltip("The Animator component which this script controls")]
        private Animator _Animator;

        public Animator Animator
        {
            get => _Animator;
            set
            {
                _Animator = value;
                if (IsInitialized)
                    Play();
            }
        }

         

        [SerializeField, Tooltip("The animation that will be played")]
        private AnimationClip _Clip;

     
        public AnimationClip Clip
        {
            get => _Clip;
            set
            {
                _Clip = value;
                if (IsInitialized)
                    Play();
            }
        }

         

      
        public bool StopOnDisable
        {
            get => !_Animator.keepAnimatorStateOnDisable;
            set => _Animator.keepAnimatorStateOnDisable = !value;
        }

         

        private PlayableGraph _Graph;

        private AnimationClipPlayable _Playable;

         

        private bool _IsPlaying;
        public bool IsPlaying
        {
            get => _IsPlaying;
            set
            {
                _IsPlaying = value;

                if (value)
                {
                    if (!IsInitialized)
                        Play();
                    else
                        _Graph.Play();
                }
                else
                {
                    if (IsInitialized)
                        _Graph.Stop();
                }
            }
        }

         

        [SerializeField, Multiplier, Tooltip("The speed at which the animation plays (default 1)")]
        private float _Speed = 1;

       
        public float Speed
        {
            get => _Speed;
            set
            {
                _Speed = value;
                _Playable.SetSpeed(value);
                IsPlaying = value != 0;
            }
        }

         

        [SerializeField, Tooltip("Determines whether Foot IK will be applied to the model (if it is Humanoid)")]
        private bool _FootIK;

     
        public bool FootIK
        {
            get => _FootIK;
            set
            {
                _FootIK = value;
                _Playable.SetApplyFootIK(value);
            }
        }

         

        public float Time
        {
            get => (float)_Playable.GetTime();
            set
            {
                // We need to call SetTime twice to ensure that animation events aren't triggered incorrectly.
                _Playable.SetTime(value);
                _Playable.SetTime(value);

                IsPlaying = true;
            }
        }

        public float NormalizedTime
        {
            get => Time / _Clip.length;
            set => Time = value * _Clip.length;
        }

         

        public bool IsInitialized => _Graph.IsValid();

         
        #endregion
         

#if UNITY_EDITOR
         

        [SerializeField, Tooltip("Should the " + nameof(Clip) + " be automatically applied to the object in Edit Mode?")]
        private bool _ApplyInEditMode = true;

        public ref bool ApplyInEditMode => ref _ApplyInEditMode;

         

        
        private void Reset()
        {
            gameObject.GetComponentInParentOrChildren(ref _Animator);
        }

         

    
        private void OnValidate()
        {
            if (IsInitialized)
            {
                Speed = Speed;
                FootIK = FootIK;
            }
            else if (_ApplyInEditMode && enabled)
            {
                _Clip.EditModeSampleAnimation(_Animator);
            }
        }

         
#endif
         

        public void Play() => Play(_Clip);

        public void Play(AnimationClip clip)
        {
            if (clip == null || _Animator == null)
                return;

            if (_Graph.IsValid())
                _Graph.Destroy();

            _Playable = AnimationPlayableUtilities.PlayClip(_Animator, clip, out _Graph);

            _Playable.SetSpeed(_Speed);

            if (!_FootIK)
                _Playable.SetApplyFootIK(false);

            if (!clip.isLooping)
                _Playable.SetDuration(clip.length);

            _IsPlaying = true;
        }

         

        private void OnEnable()
        {
            IsPlaying = true;
        }

         

        private void Update()
        {
            if (!IsPlaying)
                return;

            if (_Graph.IsDone())
            {
                IsPlaying = false;
            }
            else if (_Speed < 0 && Time <= 0)
            {
                IsPlaying = false;
                Time = 0;
            }
        }

         

        private void OnDisable()
        {
            IsPlaying = false;

            if (IsInitialized && StopOnDisable)
            {
                // Call SetTime twice to ensure that animation events aren't triggered incorrectly.
                _Playable.SetTime(0);
                _Playable.SetTime(0);
            }
        }

         

        private void OnDestroy()
        {
            if (IsInitialized)
                _Graph.Destroy();
        }

         

#if UNITY_EDITOR
        ~SoloAnimation()
        {
            UnityEditor.EditorApplication.delayCall += OnDestroy;
        }
#endif

         

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            if (_Clip != null)
                clips.Add(_Clip);
        }

         
    }
}

 
#if UNITY_EDITOR
 

namespace EasyAnimator.Editor
{
    [UnityEditor.CustomEditor(typeof(SoloAnimation)), UnityEditor.CanEditMultipleObjects]
    internal sealed class SoloAnimationEditor : UnityEditor.Editor
    {
         

        [NonSerialized]
        private Animator[] _Animators;

        [NonSerialized]
        private UnityEditor.SerializedObject _SerializedAnimator;

        [NonSerialized]
        private UnityEditor.SerializedProperty _KeepStateOnDisable;

         

        public override void OnInspectorGUI()
        {
            DoSerializedFieldsGUI();
            RefreshSerializedAnimator();
            DoStopOnDisableGUI();
            DoRuntimeDetailsGUI();
        }

         

        private void DoSerializedFieldsGUI()
        {
            serializedObject.Update();

            var property = serializedObject.GetIterator();

            property.NextVisible(true);

            if (property.name != "m_Script")
                UnityEditor.EditorGUILayout.PropertyField(property, true);

            while (property.NextVisible(false))
            {
                UnityEditor.EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

         

        private void RefreshSerializedAnimator()
        {
            var targets = this.targets;

            EasyAnimatorUtilities.SetLength(ref _Animators, targets.Length);

            var dirty = false;
            var hasAll = true;

            for (int i = 0; i < _Animators.Length; i++)
            {
                var animator = (targets[i] as SoloAnimation).Animator;
                if (_Animators[i] != animator)
                {
                    _Animators[i] = animator;
                    dirty = true;
                }

                if (animator == null)
                    hasAll = false;
            }

            if (!dirty)
                return;

            OnDisable();

            if (!hasAll)
                return;

            _SerializedAnimator = new UnityEditor.SerializedObject(_Animators);
            _KeepStateOnDisable = _SerializedAnimator.FindProperty("m_KeepAnimatorControllerStateOnDisable");
        }

         

       
        private void DoStopOnDisableGUI()
        {
            var area = EasyAnimatorGUI.LayoutSingleLineRect();

            using (ObjectPool.Disposable.AcquireContent(out var label, "Stop On Disable",
                "If true, disabling this object will stop and rewind all animations." +
                " Otherwise they will simply be paused and will resume from their current states when it is re-enabled."))
            {
                if (_KeepStateOnDisable != null)
                {
                    _KeepStateOnDisable.serializedObject.Update();

                    var content = UnityEditor.EditorGUI.BeginProperty(area, label, _KeepStateOnDisable);

                    _KeepStateOnDisable.boolValue = !UnityEditor.EditorGUI.Toggle(area, content, !_KeepStateOnDisable.boolValue);

                    UnityEditor.EditorGUI.EndProperty();

                    _KeepStateOnDisable.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    using (new UnityEditor.EditorGUI.DisabledScope(true))
                        UnityEditor.EditorGUI.Toggle(area, label, false);
                }
            }
        }

         

        private void DoRuntimeDetailsGUI()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ||
                targets.Length != 1)
                return;

            EasyAnimatorGUI.BeginVerticalBox(GUI.skin.box);

            var target = (SoloAnimation)this.target;
            if (!target.IsInitialized)
            {
                GUILayout.Label("Not Initialized");
            }
            else
            {
                UnityEditor.EditorGUI.BeginChangeCheck();
                var isPlaying = UnityEditor.EditorGUILayout.Toggle("Is Playing", target.IsPlaying);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.IsPlaying = isPlaying;

                UnityEditor.EditorGUI.BeginChangeCheck();
                var time = UnityEditor.EditorGUILayout.FloatField("Time", target.Time);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.Time = time;

                time = EasyAnimatorUtilities.Wrap01(target.NormalizedTime);
                if (time == 0 && target.Time != 0)
                    time = 1;

                UnityEditor.EditorGUI.BeginChangeCheck();
                time = UnityEditor.EditorGUILayout.Slider("Normalized Time", time, 0, 1);
                if (UnityEditor.EditorGUI.EndChangeCheck())
                    target.NormalizedTime = time;
            }

            EasyAnimatorGUI.EndVerticalBox(GUI.skin.box);
            Repaint();
        }

         

        private void OnDisable()
        {
            if (_SerializedAnimator != null)
            {
                _SerializedAnimator.Dispose();
                _SerializedAnimator = null;
                _KeepStateOnDisable = null;
            }
        }

         
    }
}

 
#endif
 

