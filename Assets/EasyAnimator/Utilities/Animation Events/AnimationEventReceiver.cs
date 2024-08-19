

using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    public struct AnimationEventReceiver
    {
         

        private EasyAnimatorState _Source;
        private int _SourceID;

       
        public EasyAnimatorState Source
        {
            get
            {
                if (_Source == null ||
                    _Source.Layer.CommandCount != _SourceID)
                    return null;

                return _Source;
            }
            set
            {
                _Source = value;

                if (value != null)
                    _SourceID = value.Layer.CommandCount;
            }
        }

         

       
        public Action<AnimationEvent> Callback { get; set; }

         

        public AnimationEventReceiver(EasyAnimatorState source, Action<AnimationEvent> callback)
        {
            _Source = source;
            _SourceID = source != null ? source.Layer.CommandCount : -1;

            Callback = callback;

#if UNITY_EDITOR
            FunctionName = null;
            ValidateSourceHasCorrectEvent();
#endif
        }

        public void Set(EasyAnimatorState source, Action<AnimationEvent> callback)
        {
            Source = source;
            Callback = callback;

#if UNITY_EDITOR
            ValidateSourceHasCorrectEvent();
#endif
        }

         

#if UNITY_EDITOR
        public string FunctionName { get; private set; }
#endif

        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public void SetFunctionName(string name)
        {
#if UNITY_EDITOR
            FunctionName = name;
#endif
        }

#if UNITY_EDITOR
        private void ValidateSourceHasCorrectEvent()
        {
            if (FunctionName == null || _Source == null || EasyAnimatorUtilities.HasEvent(_Source, FunctionName))
                return;

            var message = ObjectPool.AcquireStringBuilder()
                .Append("No Animation Event was found in ")
                .Append(_Source)
                .Append(" with the Function Name '")
                .Append(FunctionName)
                .Append('\'');

            if (_Source != null)
            {
                message.Append('\n');
                _Source.Root.AppendDescription(message);
            }

            Debug.LogWarning(message.ReleaseToString(), _Source.Root?.Component as Object);
        }
#endif

         

        public void Clear()
        {
            _Source = null;
            Callback = null;
        }

         

        public bool HandleEvent(AnimationEvent animationEvent)
        {
            if (Callback == null)
                return false;

            if (_Source != null)
            {
                if (_Source.Layer.CommandCount != _SourceID ||
                    !ReferenceEquals(_Source.Clip, animationEvent.animatorClipInfo.clip))
                    return false;
            }

#if UNITY_EDITOR
            if (FunctionName != null && FunctionName != animationEvent.functionName)
                throw new ArgumentException(
                    $"Function Name Mismatch: receiver.{nameof(FunctionName)}='{FunctionName}'" +
                    $" while {nameof(animationEvent)}.{nameof(animationEvent.functionName)}='{animationEvent.functionName}'");
#endif

            Callback(animationEvent);
            return true;
        }

         
    }
}
