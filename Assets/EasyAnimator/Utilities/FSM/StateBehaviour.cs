
using UnityEngine;

namespace EasyAnimator.FSM
{
    
    [HelpURL(StateExtensions.APIDocumentationURL + nameof(StateBehaviour))]
    public abstract class StateBehaviour : MonoBehaviour, IState
    {
         

        public virtual bool CanEnterState => true;

        public virtual bool CanExitState => true;

         

        public virtual void OnEnterState()
        {
#if UNITY_ASSERTIONS
            if (enabled)
                Debug.LogError($"{nameof(StateBehaviour)} was already enabled before {nameof(OnEnterState)}: {this}", this);
#endif
#if UNITY_EDITOR
            // Unity doesn't constantly repaint the Inspector if all the components are collapsed.
            // So we can simply force it here to ensure that it shows the correct state being enabled.
            else
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif

            enabled = true;
        }

         

        public virtual void OnExitState()
        {
            if (this == null)
                return;

#if UNITY_ASSERTIONS
            if (!enabled)
                Debug.LogError($"{nameof(StateBehaviour)} was already disabled before {nameof(OnExitState)}: {this}", this);
#endif

            enabled = false;
        }

         

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            enabled = false;
        }
#endif

         
    }
}
