

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace EasyAnimator
{
   
    public class ControllerState : EasyAnimatorState
    {
         

        public interface ITransition : ITransition<ControllerState> { }

         
        #region Fields and Properties
         

        private RuntimeAnimatorController _Controller;

        public RuntimeAnimatorController Controller
        {
            get => _Controller;
            set => ChangeMainObject(ref _Controller, value);
        }

        public override Object MainObject
        {
            get => Controller;
            set => Controller = (RuntimeAnimatorController)value;
        }

        public AnimatorControllerPlayable Playable
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable;
            }
        }

        private new AnimatorControllerPlayable _Playable;

         

        private bool _KeepStateOnStop;

       
        public bool KeepStateOnStop
        {
            get => _KeepStateOnStop;
            set
            {
                _KeepStateOnStop = value;
                if (!value && DefaultStateHashes == null && _Playable.IsValid())
                    GatherDefaultStates();
            }
        }

       
        public int[] DefaultStateHashes { get; set; }

         

#if UNITY_ASSERTIONS
        protected override string UnsupportedEventsMessage =>
            "EasyAnimator Events on " + nameof(ControllerState) + "s will probably not work as expected." +
            " The events will be associated with the entire Animator Controller and be triggered by any of the" +
            " states inside it. If you want to use events in an Animator Controller you will likely need to use" +
            " Unity's regular Animation Event system.";

        protected override string UnsupportedSpeedMessage =>
            nameof(PlayableExtensions) + "." + nameof(PlayableExtensions.SetSpeed) + " does nothing on " + nameof(ControllerState) +
            "s so there is no way to directly control their speed." +
            " The Animator Controller Speed page explains a possible workaround for this issue:" +
            "";
#endif

         

        public override void CopyIKFlags(EasyAnimatorNode node) { }

         

        public override bool ApplyAnimatorIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log($"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired layer inside the Animator Controller.", _Controller);
#endif
            }
        }

         

        public override bool ApplyFootIK
        {
            get => false;
            set
            {
#if UNITY_ASSERTIONS
                if (value)
                    OptionalWarning.UnsupportedIK.Log($"IK cannot be dynamically enabled on a {nameof(ControllerState)}." +
                        " You must instead enable it on the desired state inside the Animator Controller.", _Controller);
#endif
            }
        }

         
        #endregion
         
        #region Public API
         

        public ControllerState(RuntimeAnimatorController controller, bool keepStateOnStop = false)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            _Controller = controller;
            _KeepStateOnStop = keepStateOnStop;
        }

         

        protected override void CreatePlayable(out Playable playable)
        {
            playable = _Playable = AnimatorControllerPlayable.Create(Root._Graph, _Controller);

            if (!_KeepStateOnStop)
                GatherDefaultStates();
        }

         

       
        public override void RecreatePlayable()
        {
            if (!_Playable.IsValid())
            {
                CreatePlayable();
                return;
            }

            var parameterCount = _Playable.GetParameterCount();
            var values = new object[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                values[i] = EasyAnimatorUtilities.GetParameterValue(_Playable, _Playable.GetParameter(i));
            }

            base.RecreatePlayable();

            for (int i = 0; i < parameterCount; i++)
            {
                EasyAnimatorUtilities.SetParameterValue(_Playable, _Playable.GetParameter(i), values[i]);
            }
        }

         

       
        public AnimatorStateInfo StateInfo
        {
            get
            {
                Validate.AssertPlayable(this);
                return _Playable.IsInTransition(0) ?
                    _Playable.GetNextAnimatorStateInfo(0) :
                    _Playable.GetCurrentAnimatorStateInfo(0);
            }
        }

         

       
        protected override float RawTime
        {
            get
            {
                var info = StateInfo;
                return info.normalizedTime * info.length;
            }
            set
            {
                Validate.AssertPlayable(this);
                _Playable.PlayInFixedTime(0, 0, value);
            }
        }

         

        public override float Length => StateInfo.length;

         

        public override bool IsLooping => StateInfo.loop;

         

        public void GatherDefaultStates()
        {
            Validate.AssertPlayable(this);
            var layerCount = _Playable.GetLayerCount();
            if (DefaultStateHashes == null || DefaultStateHashes.Length != layerCount)
                DefaultStateHashes = new int[layerCount];

            while (--layerCount >= 0)
                DefaultStateHashes[layerCount] = _Playable.GetCurrentAnimatorStateInfo(layerCount).shortNameHash;
        }

        
        public override void Stop()
        {
            if (_KeepStateOnStop)
            {
                base.Stop();
            }
            else
            {
                ResetToDefaultStates();

                // Don't call base.Stop(); because it sets Time = 0; which uses PlayInFixedTime and interferes with
                // resetting to the default states.
                Weight = 0;
                IsPlaying = false;
                Events = null;
            }
        }

       
        public void ResetToDefaultStates()
        {
            Validate.AssertPlayable(this);
            for (int i = DefaultStateHashes.Length - 1; i >= 0; i--)
                _Playable.Play(DefaultStateHashes[i], i, 0);

            CancelSetTime();
        }

         

        public override void GatherAnimationClips(ICollection<AnimationClip> clips)
        {
            if (_Controller != null)
                clips.Gather(_Controller.animationClips);
        }

         

        public override void Destroy()
        {
            _Controller = null;
            base.Destroy();
        }

         
        #endregion
         
        #region Animator Controller Wrappers
         
        #region Cross Fade
         

        
        public const float DefaultFadeDuration = -1;

         

       
        public static float GetFadeDuration(float fadeDuration)
            => fadeDuration >= 0 ? fadeDuration : EasyAnimatorPlayable.DefaultFadeDuration;

         

        
        public void CrossFade(int stateNameHash,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.CrossFade(stateNameHash, GetFadeDuration(fadeDuration), layer, normalizedTime);

         

       
        public void CrossFade(string stateName,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.CrossFade(stateName, GetFadeDuration(fadeDuration), layer, normalizedTime);

         

       
        public void CrossFadeInFixedTime(int stateNameHash,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
            => Playable.CrossFadeInFixedTime(stateNameHash, GetFadeDuration(fadeDuration), layer, fixedTime);

         

      
        public void CrossFadeInFixedTime(string stateName,
            float fadeDuration = DefaultFadeDuration,
            int layer = -1,
            float fixedTime = 0)
            => Playable.CrossFadeInFixedTime(stateName, GetFadeDuration(fadeDuration), layer, fixedTime);

         
        #endregion
         
        #region Play
         

        public void Play(int stateNameHash,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.Play(stateNameHash, layer, normalizedTime);

         

        public void Play(string stateName,
            int layer = -1,
            float normalizedTime = float.NegativeInfinity)
            => Playable.Play(stateName, layer, normalizedTime);

         

        public void PlayInFixedTime(int stateNameHash,
            int layer = -1,
            float fixedTime = 0)
            => Playable.PlayInFixedTime(stateNameHash, layer, fixedTime);

         

        public void PlayInFixedTime(string stateName,
            int layer = -1,
            float fixedTime = 0)
            => Playable.PlayInFixedTime(stateName, layer, fixedTime);

         
        #endregion
         
        #region Parameters
         

        public bool GetBool(int id) => Playable.GetBool(id);
        public bool GetBool(string name) => Playable.GetBool(name);
        public void SetBool(int id, bool value) => Playable.SetBool(id, value);
        public void SetBool(string name, bool value) => Playable.SetBool(name, value);

        public float GetFloat(int id) => Playable.GetFloat(id);
        public float GetFloat(string name) => Playable.GetFloat(name);
        public void SetFloat(int id, float value) => Playable.SetFloat(id, value);
        public void SetFloat(string name, float value) => Playable.SetFloat(name, value);

        public int GetInteger(int id) => Playable.GetInteger(id);
        public int GetInteger(string name) => Playable.GetInteger(name);
        public void SetInteger(int id, int value) => Playable.SetInteger(id, value);
        public void SetInteger(string name, int value) => Playable.SetInteger(name, value);

        public void SetTrigger(int id) => Playable.SetTrigger(id);
        public void SetTrigger(string name) => Playable.SetTrigger(name);
        public void ResetTrigger(int id) => Playable.ResetTrigger(id);
        public void ResetTrigger(string name) => Playable.ResetTrigger(name);

        public AnimatorControllerParameter GetParameter(int index) => Playable.GetParameter(index);
        public int GetParameterCount() => Playable.GetParameterCount();

        public bool IsParameterControlledByCurve(int id) => Playable.IsParameterControlledByCurve(id);
        public bool IsParameterControlledByCurve(string name) => Playable.IsParameterControlledByCurve(name);

         
        #endregion
         
        #region Misc
         
        // Layers.
         

        public float GetLayerWeight(int layerIndex) => Playable.GetLayerWeight(layerIndex);
        public void SetLayerWeight(int layerIndex, float weight) => Playable.SetLayerWeight(layerIndex, weight);

        public int GetLayerCount() => Playable.GetLayerCount();

        public int GetLayerIndex(string layerName) => Playable.GetLayerIndex(layerName);
        public string GetLayerName(int layerIndex) => Playable.GetLayerName(layerIndex);

         
        // States.
         

        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0) => Playable.GetCurrentAnimatorStateInfo(layerIndex);
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex = 0) => Playable.GetNextAnimatorStateInfo(layerIndex);

        public bool HasState(int layerIndex, int stateID) => Playable.HasState(layerIndex, stateID);

         
        // Transitions.
         

        public bool IsInTransition(int layerIndex = 0) => Playable.IsInTransition(layerIndex);

        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex = 0) => Playable.GetAnimatorTransitionInfo(layerIndex);

         
        // Clips.
         

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex = 0) => Playable.GetCurrentAnimatorClipInfo(layerIndex);
        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => Playable.GetCurrentAnimatorClipInfo(layerIndex, clips);
        public int GetCurrentAnimatorClipInfoCount(int layerIndex = 0) => Playable.GetCurrentAnimatorClipInfoCount(layerIndex);

        public AnimatorClipInfo[] GetNextAnimatorClipInfo(int layerIndex = 0) => Playable.GetNextAnimatorClipInfo(layerIndex);
        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => Playable.GetNextAnimatorClipInfo(layerIndex, clips);
        public int GetNextAnimatorClipInfoCount(int layerIndex = 0) => Playable.GetNextAnimatorClipInfoCount(layerIndex);

         
        #endregion
         
        #endregion
         
        #region Parameter IDs
         

        public readonly struct ParameterID
        {
             

            public readonly string Name;

            public readonly int Hash;

             

           
            public ParameterID(string name)
            {
                Name = name;
                Hash = Animator.StringToHash(name);
            }

          
            public ParameterID(int hash)
            {
                Name = null;
                Hash = hash;
            }

            public ParameterID(string name, int hash)
            {
                Name = name;
                Hash = hash;
            }

             

            
            public static implicit operator ParameterID(string name) => new ParameterID(name);

            
            public static implicit operator ParameterID(int hash) => new ParameterID(hash);

             

            public static implicit operator int(ParameterID parameter) => parameter.Hash;

             

#if UNITY_EDITOR
            private static Dictionary<RuntimeAnimatorController, Dictionary<int, AnimatorControllerParameterType>>
                _ControllerToParameterHashAndType;
#endif

           
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void ValidateHasParameter(RuntimeAnimatorController controller, AnimatorControllerParameterType type)
            {
#if UNITY_EDITOR
                Editor.EasyAnimatorEditorUtilities.InitializeCleanDictionary(ref _ControllerToParameterHashAndType);

                // Get the parameter details.
                if (!_ControllerToParameterHashAndType.TryGetValue(controller, out var parameterDetails))
                {
                    var editorController = (AnimatorController)controller;
                    var parameters = editorController.parameters;
                    var count = parameters.Length;

                    // Animator Controllers loaded from Asset Bundles only contain their RuntimeAnimatorController data
                    // but not the editor AnimatorController data which we need to perform this validation.
                    if (count == 0 &&
                        editorController.layers.Length == 0)// Double check that the editor data is actually empty.
                    {
                        _ControllerToParameterHashAndType.Add(controller, null);
                        return;
                    }

                    parameterDetails = new Dictionary<int, AnimatorControllerParameterType>();

                    for (int i = 0; i < count; i++)
                    {
                        var parameter = parameters[i];
                        parameterDetails.Add(parameter.nameHash, parameter.type);
                    }

                    _ControllerToParameterHashAndType.Add(controller, parameterDetails);
                }

                if (parameterDetails == null)
                    return;

                // Check that there is a parameter with the correct hash and type.

                if (!parameterDetails.TryGetValue(Hash, out var parameterType))
                {
                    throw new ArgumentException($"{controller} has no {type} parameter matching {this}");
                }

                if (type != parameterType)
                {
                    throw new ArgumentException($"{controller} has a parameter matching {this}, but it is not a {type}");
                }
#endif
            }

             

            public override string ToString()
            {
                return $"{nameof(ControllerState)}.{nameof(ParameterID)}" +
                    $"({nameof(Name)}: '{Name}'" +
                    $", {nameof(Hash)}: {Hash})";
            }

             
        }

         
        #endregion
         
        #region Inspector
         

        public virtual int ParameterCount => 0;

        public virtual int GetParameterHash(int index) => throw new NotSupportedException();

         
#if UNITY_EDITOR
         

        protected internal override Editor.IEasyAnimatorNodeDrawer CreateDrawer() => new Drawer(this);

         

        public sealed class Drawer : Editor.ParametizedEasyAnimatorStateDrawer<ControllerState>
        {
             

            public Drawer(ControllerState state) : base(state) { }

             

            protected override void DoDetailsGUI()
            {
                GatherParameters();
                base.DoDetailsGUI();
            }

             

            private readonly List<AnimatorControllerParameter>
                Parameters = new List<AnimatorControllerParameter>();

            private void GatherParameters()
            {
                Parameters.Clear();

                var count = Target.ParameterCount;
                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                {
                    var hash = Target.GetParameterHash(i);
                    Parameters.Add(GetParameter(hash));
                }
            }

             

            private AnimatorControllerParameter GetParameter(int hash)
            {
                Validate.AssertPlayable(Target);
                var parameterCount = Target._Playable.GetParameterCount();
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameter = Target._Playable.GetParameter(i);
                    if (parameter.nameHash == hash)
                        return parameter;
                }

                return null;
            }

             

            public override int ParameterCount => Parameters.Count;

            public override string GetParameterName(int index) => Parameters[index].name;

            public override AnimatorControllerParameterType GetParameterType(int index) => Parameters[index].type;

            public override object GetParameterValue(int index)
            {
                Validate.AssertPlayable(Target);
                return EasyAnimatorUtilities.GetParameterValue(Target._Playable, Parameters[index]);
            }

            public override void SetParameterValue(int index, object value)
            {
                Validate.AssertPlayable(Target);
                EasyAnimatorUtilities.SetParameterValue(Target._Playable, Parameters[index], value);
            }

             
        }

         
#endif
         
        #endregion
         
    }
}

