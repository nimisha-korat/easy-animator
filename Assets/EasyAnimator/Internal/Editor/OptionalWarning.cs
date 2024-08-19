

using System;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
    
    [Flags]
    public enum OptionalWarning
    {
        
        ProOnly = 1 << 0,

       
        CreateGraphWhileDisabled = 1 << 1,

       
        CreateGraphDuringGuiEvent = 1 << 2,

       
        NativeControllerHumanoid = 1 << 3,

       
        NativeControllerHybrid = 1 << 4,

       
        DuplicateEvent = 1 << 5,

       
        EndEventInterrupt = 1 << 6,

      
        UselessEvent = 1 << 7,

      
        UnsupportedEvents = 1 << 8,

       
        UnsupportedSpeed = 1 << 9,

        UnsupportedIK = 1 << 10,

       
        MixerMinChildren = 1 << 11,

       
        MixerSynchronizeZeroLength = 1 << 12,

        CustomFadeBounds = 1 << 13,

       
        CustomFadeNotNull = 1 << 14,

       
        AnimatorSpeed = 1 << 15,

       
        UnusedNode = 1 << 16,

       
        PlayableAssetAnimatorBinding = 1 << 17,

       
        All = ~0,
    }

    public static partial class Validate
    {
         

#if UNITY_ASSERTIONS
        private static OptionalWarning _DisabledWarnings;
#endif

         

       
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Disable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings |= type;
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Enable(this OptionalWarning type)
        {
#if UNITY_ASSERTIONS
            _DisabledWarnings &= ~type;
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void SetEnabled(this OptionalWarning type, bool enable)
        {
#if UNITY_ASSERTIONS
            if (enable)
                type.Enable();
            else
                type.Disable();
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void Log(this OptionalWarning type, string message, object context = null)
        {
#if UNITY_ASSERTIONS
            if (message == null || type.IsDisabled())
                return;

            Debug.LogWarning($"Possible Bug Detected: {message}\n\nThis warning can be disabled via the " +
                $"Settings panel in '{Strings.EasyAnimatorToolsMenuPath}'" +
                $" or by calling {nameof(EasyAnimator)}.{nameof(OptionalWarning)}.{type}.{nameof(Disable)}()" +
                " and it will automatically be compiled out of Runtime Builds (except for Development Builds)." +
                $" More information can be found at {Strings.DocsURLs.OptionalWarning}\n",
                context as Object);
#endif
        }

         
#if UNITY_ASSERTIONS
         

        public static bool IsEnabled(this OptionalWarning type) => (_DisabledWarnings & type) == 0;

         

        public static bool IsDisabled(this OptionalWarning type) => (_DisabledWarnings & type) == type;

         

        public static OptionalWarning DisableTemporarily(this OptionalWarning type)
        {
            var previous = type;
            type.Disable();
            return previous & type;
        }

         

        private const string PermanentlyDisabledWarningsKey = nameof(EasyAnimator) + "." + nameof(PermanentlyDisabledWarnings);

        public static OptionalWarning PermanentlyDisabledWarnings
        {
            get => (OptionalWarning)PlayerPrefs.GetInt(PermanentlyDisabledWarningsKey);
            set
            {
                _DisabledWarnings = value;
                PlayerPrefs.SetInt(PermanentlyDisabledWarningsKey, (int)value);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        private static void InitializePermanentlyDisabledWarnings()
        {
            _DisabledWarnings |= PermanentlyDisabledWarnings;
        }

         
#endif
         
    }
}

