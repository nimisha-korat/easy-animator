

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
    
#if EasyAnimator_SCRIPTABLE_OBJECT_EDITOR
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true), CanEditMultipleObjects]
#endif
    public class ScriptableObjectEditor : UnityEditor.Editor
    {
         

       
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target != null &&
                EditorApplication.isPlayingOrWillChangePlaymode &&
                EditorUtility.IsPersistent(target))
            {
                EditorGUILayout.HelpBox("This is an asset, not a scene object," +
                    " which means that any changes you make to it are permanent" +
                    " and will NOT be undone when you exit Play Mode.", MessageType.Warning);
            }
        }

         
    }
}

#endif

