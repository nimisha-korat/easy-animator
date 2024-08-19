

using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
  
    [AddComponentMenu(Strings.MenuPrefix + "Hybrid EasyAnimator Component")]
    [HelpURL(Strings.DocsURLs.APIDocumentation + "/" + nameof(HybridEasyAnimatorComponent))]
    public class HybridEasyAnimatorComponent : NamedEasyAnimatorComponent
    {
         
        #region Fields and Properties
         

        [SerializeField, Tooltip("The main Animator Controller that this object will play")]
        private ControllerTransition _Controller;

        public ref ControllerTransition Controller => ref _Controller;

         
        #endregion
         
        #region Initialisation
         

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            if (Animator != null)
            {
                Controller = Animator.runtimeAnimatorController;
                Animator.runtimeAnimatorController = null;
            }

            PlayAutomatically = false;
        }
#endif

         

        protected override void OnEnable()
        {
            PlayController();
            base.OnEnable();

#if UNITY_ASSERTIONS
            if (Animator != null && Animator.runtimeAnimatorController != null)
                OptionalWarning.NativeControllerHybrid.Log($"An Animator Controller is assigned to the" +
                    $" {nameof(Animator)} component while also using a {nameof(HybridEasyAnimatorComponent)}." +
                    $" Most likely only one of them is being used so the other should be removed." +
                    $" See the documentation for more information: {Strings.DocsURLs.AnimatorControllers}", this);
#endif
        }

         

        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            base.GatherAnimationClips(clips);
            clips.GatherFromSource(_Controller);
        }

         
        #endregion
         
        #region Animator Controller Wrappers
         

        public ControllerState PlayController()
        {
            if (!_Controller.IsValid())
                return null;

            // Don't just return the result of Transition because it is an EasyAnimatorState which we would need to cast.
            Play(_Controller);
            return _Controller.State;
        }

         
        #region Cross Fade
         

        public void CrossFade(
            int stateNameHash,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);
            var controllerState = PlayController();
            controllerState.Playable.CrossFade(stateNameHash, fadeDuration, layer, normalizedTime);
        }

         

        public EasyAnimatorState CrossFade(
            string stateName,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);

            if (States.TryGet(name, out var state))
            {
                Play(state, fadeDuration);

                if (layer >= 0)
                    state.LayerIndex = layer;

                if (normalizedTime != float.NegativeInfinity)
                    state.NormalizedTime = normalizedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.CrossFade(stateName, fadeDuration, layer, normalizedTime);
                return controllerState;
            }
        }

         

        public void CrossFadeInFixedTime(
            int stateNameHash,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);
            var controllerState = PlayController();
            controllerState.Playable.CrossFadeInFixedTime(stateNameHash, fadeDuration, layer, fixedTime);
        }

         

        public EasyAnimatorState CrossFadeInFixedTime(
            string stateName,
            float fadeDuration = ControllerState.DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
        {
            fadeDuration = ControllerState.GetFadeDuration(fadeDuration);

            if (States.TryGet(name, out var state))
            {
                Play(state, fadeDuration);

                if (layer >= 0)
                    state.LayerIndex = layer;

                state.Time = fixedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.CrossFadeInFixedTime(stateName, fadeDuration, layer, fixedTime);
                return controllerState;
            }
        }

         
        #endregion
         
        #region Play
         

        public void Play(
            int stateNameHash,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            var controllerState = PlayController();
            controllerState.Playable.Play(stateNameHash, layer, normalizedTime);
        }

         

        public EasyAnimatorState Play(
            string stateName,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
        {
            if (States.TryGet(name, out var state))
            {
                Play(state);

                if (layer >= 0)
                    state.LayerIndex = layer;

                if (normalizedTime != float.NegativeInfinity)
                    state.NormalizedTime = normalizedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.Play(stateName, layer, normalizedTime);
                return controllerState;
            }
        }

         

        public void PlayInFixedTime(
            int stateNameHash,
            int layer = -1,
            float fixedTime = 0)
        {
            var controllerState = PlayController();
            controllerState.Playable.PlayInFixedTime(stateNameHash, layer, fixedTime);
        }

         

        public EasyAnimatorState PlayInFixedTime(
            string stateName,
            int layer = -1,
            float fixedTime = 0)
        {
            if (States.TryGet(name, out var state))
            {
                Play(state);

                if (layer >= 0)
                    state.LayerIndex = layer;

                state.Time = fixedTime;

                return state;
            }
            else
            {
                var controllerState = PlayController();
                controllerState.Playable.PlayInFixedTime(stateName, layer, fixedTime);
                return controllerState;
            }
        }

         
        #endregion
         
        #region Parameters
         

        public bool GetBool(int id) => _Controller.State.Playable.GetBool(id);
        public bool GetBool(string name) => _Controller.State.Playable.GetBool(name);
        public void SetBool(int id, bool value) => _Controller.State.Playable.SetBool(id, value);
        public void SetBool(string name, bool value) => _Controller.State.Playable.SetBool(name, value);

        public float GetFloat(int id) => _Controller.State.Playable.GetFloat(id);
        public float GetFloat(string name) => _Controller.State.Playable.GetFloat(name);
        public void SetFloat(int id, float value) => _Controller.State.Playable.SetFloat(id, value);
        public void SetFloat(string name, float value) => _Controller.State.Playable.SetFloat(name, value);

        public int GetInteger(int id) => _Controller.State.Playable.GetInteger(id);
        public int GetInteger(string name) => _Controller.State.Playable.GetInteger(name);
        public void SetInteger(int id, int value) => _Controller.State.Playable.SetInteger(id, value);
        public void SetInteger(string name, int value) => _Controller.State.Playable.SetInteger(name, value);

        public void SetTrigger(int id) => _Controller.State.Playable.SetTrigger(id);
        public void SetTrigger(string name) => _Controller.State.Playable.SetTrigger(name);
        public void ResetTrigger(int id) => _Controller.State.Playable.ResetTrigger(id);
        public void ResetTrigger(string name) => _Controller.State.Playable.ResetTrigger(name);

        public AnimatorControllerParameter GetParameter(int index) => _Controller.State.Playable.GetParameter(index);
        public int GetParameterCount() => _Controller.State.Playable.GetParameterCount();

        public bool IsParameterControlledByCurve(int id) => _Controller.State.Playable.IsParameterControlledByCurve(id);
        public bool IsParameterControlledByCurve(string name) => _Controller.State.Playable.IsParameterControlledByCurve(name);

         
        #endregion
         
        #region Misc
         
        // Layers.
         

        public float GetLayerWeight(int layerIndex) => _Controller.State.Playable.GetLayerWeight(layerIndex);
        public void SetLayerWeight(int layerIndex, float weight) => _Controller.State.Playable.SetLayerWeight(layerIndex, weight);

        public int GetLayerCount() => _Controller.State.Playable.GetLayerCount();

        public int GetLayerIndex(string layerName) => _Controller.State.Playable.GetLayerIndex(layerName);
        public string GetLayerName(int layerIndex) => _Controller.State.Playable.GetLayerName(layerIndex);

         
        // States.
         

        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0) => _Controller.State.Playable.GetCurrentAnimatorStateInfo(layerIndex);
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex = 0) => _Controller.State.Playable.GetNextAnimatorStateInfo(layerIndex);

        public bool HasState(int layerIndex, int stateID) => _Controller.State.Playable.HasState(layerIndex, stateID);

         
        // Transitions.
         

        public bool IsInTransition(int layerIndex = 0) => _Controller.State.Playable.IsInTransition(layerIndex);

        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex = 0) => _Controller.State.Playable.GetAnimatorTransitionInfo(layerIndex);

         
        // Clips.
         

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex = 0) => _Controller.State.Playable.GetCurrentAnimatorClipInfo(layerIndex);
        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => _Controller.State.Playable.GetCurrentAnimatorClipInfo(layerIndex, clips);
        public int GetCurrentAnimatorClipInfoCount(int layerIndex = 0) => _Controller.State.Playable.GetCurrentAnimatorClipInfoCount(layerIndex);

        public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex = 0) => _Controller.State.Playable.GetNextAnimatorClipInfo(layerIndex);
        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => _Controller.State.Playable.GetNextAnimatorClipInfo(layerIndex, clips);
        public int GetNextAnimatorClipInfoCount(int layerIndex = 0) => _Controller.State.Playable.GetNextAnimatorClipInfoCount(layerIndex);

         
        #endregion
         
        #endregion
         
    }
}
