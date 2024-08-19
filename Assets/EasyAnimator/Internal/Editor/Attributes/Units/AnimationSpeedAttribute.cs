
#if UNITY_EDITOR
using EasyAnimator.Editor;
using UnityEditor;
using UnityEngine;

#endif

namespace EasyAnimator.Units
{
   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class AnimationSpeedAttribute : UnitsAttribute
    {
         

        public AnimationSpeedAttribute()
        {
#if UNITY_EDITOR
            SetUnits(Multipliers, DisplayConverters);
            Rule = Validate.Value.IsFiniteOrNaN;
            IsOptional = true;
            DefaultValue = 1;
#endif
        }

         
#if UNITY_EDITOR
         

        private static new readonly float[]
            Multipliers = { 1 };

        private static new readonly CompactUnitConversionCache[]
            DisplayConverters = { AnimationTimeAttribute.XSuffix, };

         

        protected override int GetLineCount(SerializedProperty property, GUIContent label) => 1;

         
#endif
         
    }
}

