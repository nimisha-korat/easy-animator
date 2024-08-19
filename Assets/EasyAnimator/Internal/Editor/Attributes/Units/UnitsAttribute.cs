

#if UNITY_EDITOR
using EasyAnimator.Editor;
using UnityEditor;
using UnityEngine;
using System;
#endif

namespace EasyAnimator.Units
{
   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public class UnitsAttribute : SelfDrawerAttribute
    {
         

        public Validate.Value Rule { get; set; }

         

        protected UnitsAttribute() { }

        public UnitsAttribute(string suffix)
        {
#if UNITY_EDITOR
            SetUnits(new float[] { 1 }, new CompactUnitConversionCache[] { new CompactUnitConversionCache(suffix) }, 0);
#endif
        }

        public UnitsAttribute(float[] multipliers, string[] suffixes, int unitIndex = 0)
        {
#if UNITY_EDITOR
            SetUnits(multipliers, new CompactUnitConversionCache[suffixes.Length], unitIndex);
            for (int i = 0; i < suffixes.Length; i++)
                DisplayConverters[i] = new CompactUnitConversionCache(suffixes[i]);
#endif
        }

         
#if UNITY_EDITOR
         

        public float[] Multipliers { get; private set; }

        public CompactUnitConversionCache[] DisplayConverters { get; private set; }

        public int UnitIndex { get; private set; }

        public bool IsOptional { get; set; }

        public float DefaultValue { get; set; }

         

        protected void SetUnits(float[] multipliers, CompactUnitConversionCache[] displayConverters, int unitIndex = 0)
        {
            if (multipliers.Length != displayConverters.Length)
                throw new ArgumentException(
                    $"[Units] {nameof(Multipliers)} and {nameof(DisplayConverters)} must have the same Length.");

            if (unitIndex < 0 || unitIndex >= multipliers.Length)
                throw new ArgumentOutOfRangeException(
                    $"[Units] {nameof(UnitIndex)} must be an index in the {nameof(Multipliers)} array.");

            Multipliers = multipliers;
            DisplayConverters = displayConverters;
            UnitIndex = unitIndex;
        }

         

        protected static float StandardSpacing => EasyAnimatorGUI.StandardSpacing;

        protected static float LineHeight => EasyAnimatorGUI.LineHeight;

         

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineCount = GetLineCount(property, label);
            return LineHeight * lineCount + StandardSpacing * (lineCount - 1);
        }

        protected virtual int GetLineCount(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.wideMode ? 1 : 2;

         

        protected static void BeginProperty(Rect area, SerializedProperty property, ref GUIContent label, out float value)
        {
            label = EditorGUI.BeginProperty(area, label, property);

            EditorGUI.BeginChangeCheck();

            value = property.floatValue;
        }

        protected static void EndProperty(Rect area, SerializedProperty property, ref float value)
        {
            if (EasyAnimatorGUI.TryUseClickEvent(area, 2))
                DefaultValueAttribute.SetToDefault(ref value, property);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;

            EditorGUI.EndProperty();
        }

         

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            BeginProperty(area, property, ref label, out var value);
            DoFieldGUI(area, label, ref value);
            EndProperty(area, property, ref value);
        }

         

        protected void DoFieldGUI(Rect area, GUIContent label, ref float value)
        {
            var isMultiLine = area.height > LineHeight;
            area.height = LineHeight;

            DoOptionalBeforeGUI(IsOptional, area, out var toggleArea, out var guiWasEnabled, out var previousLabelWidth);

            Rect allFieldArea;

            if (isMultiLine)
            {
                EditorGUI.LabelField(area, label);
                label = null;
                EasyAnimatorGUI.NextVerticalArea(ref area);

                EditorGUI.indentLevel++;
                allFieldArea = EditorGUI.IndentedRect(area);
                EditorGUI.indentLevel--;
            }
            else
            {
                var labelXMax = area.x + EditorGUIUtility.labelWidth;
                allFieldArea = new Rect(labelXMax, area.y, area.xMax - labelXMax, area.height);
            }

            // Count the number of active fields.
            var count = 0;
            var last = 0;
            for (int i = 0; i < Multipliers.Length; i++)
            {
                if (!float.IsNaN(Multipliers[i]))
                {
                    count++;
                    last = i;
                }
            }

            var width = (allFieldArea.width - StandardSpacing * (count - 1)) / count;
            var fieldArea = new Rect(allFieldArea.x, allFieldArea.y, width, allFieldArea.height);

            var displayValue = GetDisplayValue(value, DefaultValue);

            // Draw the active fields.
            for (int i = 0; i < Multipliers.Length; i++)
            {
                var multiplier = Multipliers[i];
                if (float.IsNaN(multiplier))
                    continue;

                if (label != null)
                {
                    fieldArea.xMin = area.xMin;
                }
                else if (i < last)
                {
                    fieldArea.width = width;
                    fieldArea.xMax = EasyAnimatorUtilities.Round(fieldArea.xMax);
                }
                else
                {
                    fieldArea.xMax = area.xMax;
                }

                EditorGUI.BeginChangeCheck();

                var fieldValue = displayValue * multiplier;
                fieldValue = DoSpecialFloatField(fieldArea, label, fieldValue, DisplayConverters[i]);
                label = null;

                if (EditorGUI.EndChangeCheck())
                    value = fieldValue / multiplier;

                fieldArea.x += fieldArea.width + StandardSpacing;
            }

            DoOptionalAfterGUI(IsOptional, toggleArea, ref value, DefaultValue, guiWasEnabled, previousLabelWidth);

            Validate.ValueRule(ref value, Rule);
        }

         

       
        public static float DoSpecialFloatField(Rect area, GUIContent label, float value, CompactUnitConversionCache toString)
        {
            if (label != null)
            {
                if (Event.current.type != EventType.Repaint)
                    return EditorGUI.FloatField(area, label, value);

                var dragArea = new Rect(area.x, area.y, EditorGUIUtility.labelWidth, area.height);
                EditorGUIUtility.AddCursorRect(dragArea, MouseCursor.SlideArrow);

                var text = toString.Convert(value, area.width - EditorGUIUtility.labelWidth);
                EditorGUI.TextField(area, label, text);
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                if (Event.current.type != EventType.Repaint)
                    value = EditorGUI.FloatField(area, value);
                else
                    EditorGUI.TextField(area, toString.Convert(value, area.width));

                EditorGUI.indentLevel = indentLevel;
            }

            return value;
        }

         

        private void DoOptionalBeforeGUI(bool isOptional, Rect area, out Rect toggleArea, out bool guiWasEnabled, out float previousLabelWidth)
        {
            toggleArea = area;
            guiWasEnabled = GUI.enabled;
            previousLabelWidth = EditorGUIUtility.labelWidth;
            if (!isOptional)
                return;

            toggleArea.x += previousLabelWidth;

            toggleArea.width = EasyAnimatorGUI.ToggleWidth;
            EditorGUIUtility.labelWidth += toggleArea.width;

            EditorGUIUtility.AddCursorRect(toggleArea, MouseCursor.Arrow);

            // We need to draw the toggle after everything else to it goes on top of the label. But we want it to
            // get priority for input events, so we disable the other controls during those events in its area.
            var currentEvent = Event.current;
            if (guiWasEnabled && toggleArea.Contains(currentEvent.mousePosition))
            {
                switch (currentEvent.type)
                {
                    case EventType.Repaint:
                    case EventType.Layout:
                        break;

                    default:
                        GUI.enabled = false;
                        break;
                }
            }
        }

         

        private void DoOptionalAfterGUI(bool isOptional, Rect area, ref float value, float defaultValue, bool guiWasEnabled, float previousLabelWidth)
        {
            GUI.enabled = guiWasEnabled;
            EditorGUIUtility.labelWidth = previousLabelWidth;

            if (!isOptional)
                return;

            area.x += EasyAnimatorGUI.StandardSpacing;

            var wasEnabled = !float.IsNaN(value);

            // Use the EditorGUI method instead to properly handle EditorGUI.showMixedValue.
            //var isEnabled = GUI.Toggle(area, wasEnabled, GUIContent.none);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var isEnabled = EditorGUI.Toggle(area, wasEnabled);

            EditorGUI.indentLevel = indentLevel;

            if (isEnabled != wasEnabled)
            {
                value = isEnabled ? defaultValue : float.NaN;
                EasyAnimatorGUI.Deselect();
            }
        }

         

        public static float GetDisplayValue(float value, float defaultValue)
            => float.IsNaN(value) ? defaultValue : value;

         
#endif
         
    }
}

