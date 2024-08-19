
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace EasyAnimator
{
   
    public partial class ManualMixerState : MixerState
    {
         

        public interface ITransition : ITransition<ManualMixerState> { }

         
        #region Properties
         

        private EasyAnimatorState[] _States = Array.Empty<EasyAnimatorState>();

        public override IList<EasyAnimatorState> ChildStates => _States;

         

        public override int ChildCount => _States.Length;

        public override EasyAnimatorState GetChild(int index) => _States[index];

        public override FastEnumerator<EasyAnimatorState> GetEnumerator()
            => new FastEnumerator<EasyAnimatorState>(_States, _States.Length);

         
        #endregion
         
        #region Initialisation
         

        public virtual void Initialize(int childCount)
        {
#if UNITY_ASSERTIONS
            if (childCount <= 1 && OptionalWarning.MixerMinChildren.IsEnabled())
                OptionalWarning.MixerMinChildren.Log(
                    $"{this} is being initialized with {nameof(childCount)} <= 1." +
                    $" The purpose of a mixer is to mix multiple child states.", Root?.Component);
#endif

            for (int i = _States.Length - 1; i >= 0; i--)
            {
                var state = _States[i];
                if (state == null)
                    continue;

                state.Destroy();
            }

            _States = new EasyAnimatorState[childCount];

            if (_Playable.IsValid())
            {
                _Playable.SetInputCount(childCount);
            }
            else if (Root != null)
            {
                CreatePlayable();
            }
        }

         

        public void Initialize(params AnimationClip[] clips)
        {
#if UNITY_ASSERTIONS
            if (clips == null)
                throw new ArgumentNullException(nameof(clips));
#endif

            var count = clips.Length;
            Initialize(count);

            for (int i = 0; i < count; i++)
            {
                var clip = clips[i];
                if (clip != null)
                    CreateChild(i, clip);
            }
        }

         

        public void Initialize(params Object[] states)
        {
#if UNITY_ASSERTIONS
            if (states == null)
                throw new ArgumentNullException(nameof(states));
#endif

            var count = states.Length;
            Initialize(count);

            for (int i = 0; i < count; i++)
            {
                var state = states[i];
                if (state != null)
                    CreateChild(i, state);
            }
        }

         
        #endregion
         
    }
}

