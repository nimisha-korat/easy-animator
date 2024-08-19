
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
    
    public abstract class ParametizedEasyAnimatorStateDrawer<T> : EasyAnimatorStateDrawer<T> where T : EasyAnimatorState
    {
         

        public virtual int ParameterCount => 0;

        public virtual string GetParameterName(int index) => throw new NotSupportedException();

        public virtual AnimatorControllerParameterType GetParameterType(int index) => throw new NotSupportedException();

        public virtual object GetParameterValue(int index) => throw new NotSupportedException();

        public virtual void SetParameterValue(int index, object value) => throw new NotSupportedException();

         

        protected ParametizedEasyAnimatorStateDrawer(T state) : base(state) { }

         

        protected override void DoDetailsGUI()
        {
            base.DoDetailsGUI();

            if (!IsExpanded)
                return;

            var count = ParameterCount;
            if (count <= 0)
                return;

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= EasyAnimatorGUI.IndentSize;

            for (int i = 0; i < count; i++)
            {
                var type = GetParameterType(i);
                if (type == 0)
                    continue;

                var name = GetParameterName(i);
                var value = GetParameterValue(i);

                EditorGUI.BeginChangeCheck();

                var area = EasyAnimatorGUI.LayoutSingleLineRect(EasyAnimatorGUI.SpacingMode.Before);
                area = EditorGUI.IndentedRect(area);

                switch (type)
                {
                    case AnimatorControllerParameterType.Float:
                        value = EditorGUI.FloatField(area, name, (float)value);
                        break;

                    case AnimatorControllerParameterType.Int:
                        value = EditorGUI.IntField(area, name, (int)value);
                        break;

                    case AnimatorControllerParameterType.Bool:
                        value = EditorGUI.Toggle(area, name, (bool)value);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        value = EditorGUI.Toggle(area, name, (bool)value, EditorStyles.radioButton);
                        break;

                    default:
                        EditorGUI.LabelField(area, name, "Unsupported Type: " + type);
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                    SetParameterValue(i, value);
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

         
    }
}

#endif

