
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace EasyAnimator
{
   
    [AddComponentMenu(Strings.MenuPrefix + "EasyAnimator Component")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(EasyAnimatorComponent))]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public class EasyAnimatorComponent : MonoBehaviour,
        IEasyAnimatorComponent, IEnumerator, IAnimationClipSource, IAnimationClipCollection
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
                // It doesn't seem to be possible to stop the old Animator from playing the graph.

                _Animator = value;
                if (IsPlayableInitialized)
                    _Playable.SetOutput(value, this);
            }
        }

#if UNITY_EDITOR
        string IEasyAnimatorComponent.AnimatorFieldName => nameof(_Animator);
#endif

         

        private EasyAnimatorPlayable _Playable;

        public EasyAnimatorPlayable Playable
        {
            get
            {
                InitializePlayable();
                return _Playable;
            }
        }

        public bool IsPlayableInitialized => _Playable != null && _Playable.IsValid;

         

        public EasyAnimatorPlayable.StateDictionary States => Playable.States;

        public EasyAnimatorPlayable.LayerList Layers => Playable.Layers;

        public static implicit operator EasyAnimatorPlayable(EasyAnimatorComponent EasyAnimator) => EasyAnimator.Playable;

        public static implicit operator EasyAnimatorLayer(EasyAnimatorComponent EasyAnimator) => EasyAnimator.Playable.Layers[0];

         

        [SerializeField, Tooltip("Determines what happens when this component is disabled" +
            " or its " + nameof(GameObject) + " becomes inactive (i.e. in " + nameof(OnDisable) + "):" +
            "\n• " + nameof(DisableAction.Stop) + " all animations" +
            "\n• " + nameof(DisableAction.Pause) + " all animations" +
            "\n• " + nameof(DisableAction.Continue) + " playing" +
            "\n• " + nameof(DisableAction.Reset) + " to the original values" +
            "\n• " + nameof(DisableAction.Destroy) + " all layers and states")]
        private DisableAction _ActionOnDisable;

#if UNITY_EDITOR
        string IEasyAnimatorComponent.ActionOnDisableFieldName => nameof(_ActionOnDisable);
#endif

        public ref DisableAction ActionOnDisable => ref _ActionOnDisable;

        bool IEasyAnimatorComponent.ResetOnDisable => _ActionOnDisable == DisableAction.Reset;

        public enum DisableAction
        {
            Stop,

            Pause,

            Continue,

            Reset,

            Destroy,
        }

         
        #region Update Mode
         

        public AnimatorUpdateMode UpdateMode
        {
            get => _Animator.updateMode;
            set
            {
                _Animator.updateMode = value;

                if (!IsPlayableInitialized)
                    return;

                // UnscaledTime on the Animator is actually identical to Normal when using the Playables API so we need
                // to set the graph's DirectorUpdateMode to determine how it gets its delta time.
                _Playable.UpdateMode = value == AnimatorUpdateMode.UnscaledTime ?
                    DirectorUpdateMode.UnscaledGameTime :
                    DirectorUpdateMode.GameTime;

#if UNITY_EDITOR
                if (InitialUpdateMode == null)
                {
                    InitialUpdateMode = value;
                }
                else if (UnityEditor.EditorApplication.isPlaying)
                {
                    if (EasyAnimatorPlayable.HasChangedToOrFromAnimatePhysics(InitialUpdateMode, value))
                        Debug.LogWarning($"Changing the {nameof(Animator)}.{nameof(Animator.updateMode)}" +
                            $" to or from {nameof(AnimatorUpdateMode.AnimatePhysics)} at runtime will have no effect." +
                            " You must set it in the Unity Editor or on startup.", this);
                }
#endif
            }
        }

         

#if UNITY_EDITOR
        public AnimatorUpdateMode? InitialUpdateMode { get; private set; }
#endif

         
        #endregion
         
        #endregion
         
        #region Initialisation
         

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            OnDestroy();
            gameObject.GetComponentInParentOrChildren(ref _Animator);
        }
#endif

         

        protected virtual void OnEnable()
        {
            if (IsPlayableInitialized)
                _Playable.UnpauseGraph();
        }

        protected virtual void OnDisable()
        {
            if (!IsPlayableInitialized)
                return;

            switch (_ActionOnDisable)
            {
                case DisableAction.Stop:
                    Stop();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Pause:
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Continue:
                    break;

                case DisableAction.Reset:
                    Debug.Assert(_Animator.isActiveAndEnabled,
                        $"{nameof(DisableAction)}.{nameof(DisableAction.Reset)} failed because the {nameof(Animator)} is not enabled." +
                        $" This most likely means you are disabling the {nameof(GameObject)} and the {nameof(Animator)} is above the" +
                        $" {nameof(EasyAnimatorComponent)} in the Inspector so it got disabled right before this method was called." +
                        $" See the Inspector of {this} to fix the issue" +
                        $" or use {nameof(DisableAction)}.{nameof(DisableAction.Stop)}" +
                        $" and call {nameof(Animator)}.{nameof(Animator.Rebind)} manually" +
                        $" before disabling the {nameof(GameObject)}.",
                        this);

                    Stop();
                    _Animator.Rebind();
                    _Playable.PauseGraph();
                    break;

                case DisableAction.Destroy:
                    _Playable.Destroy();
                    _Playable = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(ActionOnDisable));
            }
        }

         

        public void InitializePlayable()
        {
            if (IsPlayableInitialized)
                return;

#if UNITY_ASSERTIONS
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
            {
                if (!gameObject.activeInHierarchy)
                    OptionalWarning.CreateGraphWhileDisabled.Log($"An {nameof(EasyAnimatorPlayable)} is being created for '{this}'" +
                        $" which is attached to an inactive {nameof(GameObject)}." +
                        $" If that object is never activated then Unity will not call {nameof(OnDestroy)}" +
                        $" so {nameof(EasyAnimatorPlayable)}.{nameof(EasyAnimatorPlayable.Destroy)} will need to be called manually.", this);
            }

#if UNITY_EDITOR
            if (OptionalWarning.CreateGraphDuringGuiEvent.IsEnabled())
            {
                var currentEvent = Event.current;
                if (currentEvent != null && (currentEvent.type == EventType.Layout || currentEvent.type == EventType.Repaint))
                    OptionalWarning.CreateGraphDuringGuiEvent.Log(
                        $"Creating an {nameof(EasyAnimatorPlayable)} during a {currentEvent.type} event is likely undesirable.", this);
            }
#endif
#endif

            if (_Animator == null)
                _Animator = GetComponent<Animator>();

#if UNITY_ASSERTIONS
            if (_Animator != null && _Animator.isHuman && _Animator.runtimeAnimatorController != null)
                OptionalWarning.NativeControllerHumanoid.Log($"An Animator Controller is assigned to the" +
                    $" {nameof(Animator)} component but the Rig is Humanoid so it can't be blended with EasyAnimator." +
                    $" See the documentation for more information: {Strings.DocsURLs.AnimatorControllersNative}", this);
#endif

            EasyAnimatorPlayable.SetNextGraphName(name + " (EasyAnimator)");
            _Playable = EasyAnimatorPlayable.Create();
            _Playable.SetOutput(_Animator, this);

#if UNITY_EDITOR
            if (_Animator != null)
                InitialUpdateMode = UpdateMode;
#endif
        }

         

        protected virtual void OnDestroy()
        {
            if (IsPlayableInitialized)
            {
                _Playable.Destroy();
                _Playable = null;
            }
        }

         

#if UNITY_EDITOR
        ~EasyAnimatorComponent()
        {
            if (_Playable != null)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                        OnDestroy();
                };
            }
        }
#endif

         
        #endregion
         
        #region Play Management
         

        public virtual object GetKey(AnimationClip clip) => clip;

         
        // Play Immediately.
         

        public EasyAnimatorState Play(AnimationClip clip)
            => Playable.Play(States.GetOrCreate(clip));

        public EasyAnimatorState Play(EasyAnimatorState state)
            => Playable.Play(state);

         
        // Cross Fade.
         

        public EasyAnimatorState Play(AnimationClip clip, float fadeDuration, FadeMode mode = default)
            => Playable.Play(States.GetOrCreate(clip), fadeDuration, mode);

        public EasyAnimatorState Play(EasyAnimatorState state, float fadeDuration, FadeMode mode = default)
            => Playable.Play(state, fadeDuration, mode);

         
        // Transition.
         

        public EasyAnimatorState Play(ITransition transition)
            => Playable.Play(transition);

        public EasyAnimatorState Play(ITransition transition, float fadeDuration, FadeMode mode = default)
            => Playable.Play(transition, fadeDuration, mode);

         
        // Try Play.
         

        public EasyAnimatorState TryPlay(object key)
            => Playable.TryPlay(key);

        public EasyAnimatorState TryPlay(object key, float fadeDuration, FadeMode mode = default)
            => Playable.TryPlay(key, fadeDuration, mode);

         

        public EasyAnimatorState Stop(AnimationClip clip) => Stop(GetKey(clip));

        public EasyAnimatorState Stop(IHasKey hasKey) => _Playable?.Stop(hasKey);

        public EasyAnimatorState Stop(object key) => _Playable?.Stop(key);

        public void Stop()
        {
            if (_Playable != null)
                _Playable.Stop();
        }

         

        public bool IsPlaying(AnimationClip clip) => IsPlaying(GetKey(clip));

        public bool IsPlaying(IHasKey hasKey) => _Playable != null && _Playable.IsPlaying(hasKey);

        public bool IsPlaying(object key) => _Playable != null && _Playable.IsPlaying(key);

        public bool IsPlaying() => _Playable != null && _Playable.IsPlaying();

         

        public bool IsPlayingClip(AnimationClip clip) => _Playable != null && _Playable.IsPlayingClip(clip);

         

        public void Evaluate() => Playable.Evaluate();

        public void Evaluate(float deltaTime) => Playable.Evaluate(deltaTime);

         
        #region Key Error Methods
#if UNITY_EDITOR
         
        // These are overloads of other methods that take a System.Object key to ensure the user doesn't try to use an
        // EasyAnimatorState as a key, since the whole point of a key is to identify a state in the first place.
         

        [Obsolete("You should not use an EasyAnimatorState as a key. Just call EasyAnimatorState.Stop().", true)]
        public EasyAnimatorState Stop(EasyAnimatorState key)
        {
            key.Stop();
            return key;
        }

        [Obsolete("You should not use an EasyAnimatorState as a key. Just check EasyAnimatorState.IsPlaying.", true)]
        public bool IsPlaying(EasyAnimatorState key) => key.IsPlaying;

         
#endif
        #endregion
         
        #endregion
         
        #region Enumeration
         
        // IEnumerator for yielding in a coroutine to wait until all animations have stopped.
         

        bool IEnumerator.MoveNext()
        {
            if (!IsPlayableInitialized)
                return false;

            return ((IEnumerator)_Playable).MoveNext();
        }

        object IEnumerator.Current => null;

#pragma warning disable UNT0006 // Incorrect message signature.
        void IEnumerator.Reset() { }
#pragma warning restore UNT0006 // Incorrect message signature.

         

        public void GetAnimationClips(List<AnimationClip> clips)
        {
            var set = ObjectPool.AcquireSet<AnimationClip>();
            set.UnionWith(clips);

            GatherAnimationClips(set);

            clips.Clear();
            clips.AddRange(set);

            ObjectPool.Release(set);
        }

         

        public virtual void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (IsPlayableInitialized)
                _Playable.GatherAnimationClips(clips);

#if UNITY_EDITOR
            Editor.AnimationGatherer.GatherFromGameObject(gameObject, clips);

            if (_Animator != null && _Animator.gameObject != gameObject)
                Editor.AnimationGatherer.GatherFromGameObject(_Animator.gameObject, clips);
#endif
        }

         
        #endregion
         
    }
}
