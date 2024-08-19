

using UnityEngine;

#if UNITY_EDITOR
using EasyAnimator.Editor;
using UnityEditor;
#endif

namespace EasyAnimator
{
   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public abstract class SelfDrawerAttribute : PropertyAttribute
    {
         
#if UNITY_EDITOR
         

        public virtual bool CanCacheInspectorGUI(SerializedProperty property) => true;

        public virtual float GetPropertyHeight(SerializedProperty property, GUIContent label) => EasyAnimatorGUI.LineHeight;

        public abstract void OnGUI(Rect area, SerializedProperty property, GUIContent label);

         
#endif
         
    }
}

#if UNITY_EDITOR

namespace EasyAnimator.Editor
{
   
    [CustomPropertyDrawer(typeof(SelfDrawerAttribute), true)]
    internal sealed class SelfDrawerDrawer : PropertyDrawer
    {
         

        public SelfDrawerAttribute Attribute => (SelfDrawerAttribute)attribute;

         

        public override bool CanCacheInspectorGUI(SerializedProperty property)
            => Attribute.CanCacheInspectorGUI(property);

         

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => Attribute.GetPropertyHeight(property, label);

         

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            => Attribute.OnGUI(area, property, label);

         
    }
}

#endif

