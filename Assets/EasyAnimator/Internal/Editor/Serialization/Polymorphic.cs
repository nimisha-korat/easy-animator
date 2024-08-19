

using UnityEngine;

namespace EasyAnimator
{
     

    public interface IPolymorphic { }

     

    public interface IPolymorphicReset : IPolymorphic
    {
        void Reset();
    }

     

    public sealed class PolymorphicAttribute : PropertyAttribute { }

     
}

#if UNITY_EDITOR

namespace EasyAnimator.Editor
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(IPolymorphic), true)]
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public class PolymorphicDrawer : PropertyDrawer
    {
         

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

         

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            using (new TypeSelectionButton(area, property, true))
                EditorGUI.PropertyField(area, property, label, true);
        }

         
    }
}

#endif
