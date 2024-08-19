

using System;
using System.Reflection;

#if UNITY_EDITOR
using EasyAnimator.Editor;
using UnityEditor;
#endif

namespace EasyAnimator
{
  
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public class DefaultValueAttribute : Attribute
    {
         

        public virtual object Primary { get; protected set; }

         

        public virtual object Secondary { get; protected set; }

         

        public DefaultValueAttribute(object primary, object secondary = null)
        {
            Primary = primary;
            Secondary = secondary;
        }

         

        protected DefaultValueAttribute() { }

         
#if UNITY_EDITOR
         

        public static void SetToDefault<T>(ref T value, SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            var field = accessor.GetField(property);
            if (field == null)
                accessor.SetValue(property, null);
            else
                SetToDefault(ref value, field);
        }

         

        public static void SetToDefault<T>(ref T value, FieldInfo field)
        {
            var defaults = field.GetAttribute<DefaultValueAttribute>();
            if (defaults != null)
                defaults.SetToDefault(ref value);
            else
                value = default;
        }

         

        public void SetToDefault<T>(ref T value)
        {
            var primary = Primary;
            if (!Equals(value, primary))
            {
                value = (T)primary;
                return;
            }

            var secondary = Secondary;
            if (secondary != null || !typeof(T).IsValueType)
            {
                value = (T)secondary;
                return;
            }
        }

         

        public static void SetToDefault<T>(ref T value, T primary, T secondary)
        {
            if (!Equals(value, primary))
                value = primary;
            else
                value = secondary;
        }

         
#endif
         
    }
}

