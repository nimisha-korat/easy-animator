

#if UNITY_EDITOR
using EasyAnimator.Editor;
using System;
using UnityEditor;
using UnityEngine;
#endif

namespace EasyAnimator.Units
{
   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class AnimationTimeAttribute : UnitsAttribute
    {
         

        public enum Units
        {
            Normalized = 0,

            Seconds = 1,

            Frames = 2,
        }

        public const string Tooltip = "x = Normalized, s = Seconds, f = Frame";

         

        public AnimationTimeAttribute(Units units)
        {
#if UNITY_EDITOR
            SetUnits(Multipliers, DisplayConverters, (int)units);
#endif
        }

         
#if UNITY_EDITOR
         

        public static readonly CompactUnitConversionCache
            XSuffix = new CompactUnitConversionCache("x");

        private static new readonly CompactUnitConversionCache[] DisplayConverters =
        {
            XSuffix,
            new CompactUnitConversionCache("s"),
            new CompactUnitConversionCache("f"),
        };

         

        public static float nextDefaultValue = float.NaN;

         

        protected override int GetLineCount(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.wideMode || TransitionDrawer.Context == null ? 1 : 2;

         

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            var nextDefaultValue = AnimationTimeAttribute.nextDefaultValue;

            BeginProperty(area, property, ref label, out var value);
            OnGUI(area, label, ref value);
            EndProperty(area, property, ref value);

            if (EditorGUI.EndChangeCheck())
            {
                TransitionPreviewWindow.PreviewNormalizedTime =
                    GetDisplayValue(value, nextDefaultValue) * Multipliers[(int)Units.Normalized];
            }
        }

         

        public void OnGUI(Rect area, GUIContent label, ref float value)
        {
            var context = TransitionDrawer.Context;
            if (context == null)
            {
                value = DoSpecialFloatField(area, label, value, DisplayConverters[UnitIndex]);
                goto Return;
            }

            var length = context.MaximumDuration;
            if (length <= 0)
                length = float.NaN;

            EasyAnimatorUtilities.TryGetFrameRate(context.Transition, out var frameRate);

            var multipliers = CalculateMultipliers(length, frameRate);
            if (multipliers == null)
            {
                EditorGUI.LabelField(area, label.text, $"Invalid {nameof(Validate)}.{nameof(Validate.Value)}");
                goto Return;
            }

            DoPreviewTimeButton(ref area, ref value, context.Transition, multipliers);

            IsOptional = !float.IsNaN(nextDefaultValue);
            DefaultValue = nextDefaultValue;
            DoFieldGUI(area, label, ref value);

            Return:
            nextDefaultValue = float.NaN;
        }

         

        private static new readonly float[] Multipliers = new float[3];

        private float[] CalculateMultipliers(float length, float frameRate)
        {
            switch ((Units)UnitIndex)
            {
                case Units.Normalized:
                    Multipliers[(int)Units.Normalized] = 1;
                    Multipliers[(int)Units.Seconds] = length;
                    Multipliers[(int)Units.Frames] = length * frameRate;
                    break;

                case Units.Seconds:
                    Multipliers[(int)Units.Normalized] = 1f / length;
                    Multipliers[(int)Units.Seconds] = 1;
                    Multipliers[(int)Units.Frames] = frameRate;
                    break;

                case Units.Frames:
                    Multipliers[(int)Units.Normalized] = 1f / length / frameRate;
                    Multipliers[(int)Units.Seconds] = 1f / frameRate;
                    Multipliers[(int)Units.Frames] = 1;
                    break;

                default:
                    return null;
            }

            var settings = EasyAnimatorSettings.AnimationTimeFields;
            ApplyVisibilitySetting(settings.showNormalized, Units.Normalized);
            ApplyVisibilitySetting(settings.showSeconds, Units.Seconds);
            ApplyVisibilitySetting(settings.showFrames, Units.Frames);

            void ApplyVisibilitySetting(bool show, Units setting)
            {
                if (show)
                    return;

                var index = (int)setting;
                if (UnitIndex != index)
                    Multipliers[index] = float.NaN;
            }

            return Multipliers;
        }

         

        private void DoPreviewTimeButton(ref Rect area, ref float value, ITransitionDetailed transition,
            float[] multipliers)
        {
            if (!TransitionPreviewWindow.IsPreviewingCurrentProperty())
                return;

            var previewTime = TransitionPreviewWindow.PreviewNormalizedTime;

            const string Tooltip =
                "• Left Click = preview the current value of this field." +
                "\n• Right Click = set this field to use the current preview time.";

            var displayValue = GetDisplayValue(value, nextDefaultValue);

            var multiplier = multipliers[(int)Units.Normalized];
            displayValue *= multiplier;

            var isCurrent = Mathf.Approximately(displayValue, previewTime);

            var buttonArea = area;
            if (TransitionDrawer.DoPreviewButtonGUI(ref buttonArea, isCurrent, Tooltip))
            {
                if (Event.current.button != 1)
                    TransitionPreviewWindow.PreviewNormalizedTime = displayValue;
                else
                    value = previewTime / multiplier;
            }

            // Only steal the button area for single line fields.
            if (area.height <= LineHeight)
                area = buttonArea;
        }

         

        [Serializable]
        public sealed class Settings
        {
             

            [Tooltip("Should time fields show approximations if the value is too long for the GUI?" +
                " For example, '1.111111' could instead show '1.111~'.")]
            public bool showApproximations = true;

            [Tooltip("Should the " + nameof(Units.Normalized) + " field be shown?")]
            public bool showNormalized = true;

            [Tooltip("Should the " + nameof(Units.Seconds) + " field be shown?")]
            public bool showSeconds = true;

            [Tooltip("Should the " + nameof(Units.Frames) + " field be shown?")]
            public bool showFrames = true;

             
        }

         
#endif
         
    }
}

