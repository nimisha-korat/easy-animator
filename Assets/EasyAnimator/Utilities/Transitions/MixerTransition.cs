

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace EasyAnimator
{
    [Serializable]
    public abstract class MixerTransition<TMixer, TParameter> : ManualMixerTransition<TMixer>
        where TMixer : MixerState<TParameter>
    {
         

        [SerializeField, HideInInspector]
        private TParameter[] _Thresholds;

        public ref TParameter[] Thresholds => ref _Thresholds;

        public const string ThresholdsField = nameof(_Thresholds);

         

        [SerializeField]
        private TParameter _DefaultParameter;

        public ref TParameter DefaultParameter => ref _DefaultParameter;

        public const string DefaultParameterField = nameof(_DefaultParameter);

         

        public override void InitializeState()
        {
            base.InitializeState();

            State.SetThresholds(_Thresholds);
            State.Parameter = _DefaultParameter;
        }

         
    }

     

#if UNITY_EDITOR
   
    public class MixerTransitionDrawer : ManualMixerTransition.Drawer
    {
         

        private readonly float ThresholdWidth;

         

        private static float _StandardThresholdWidth;

        protected static float StandardThresholdWidth
        {
            get
            {
                if (_StandardThresholdWidth == 0)
                    _StandardThresholdWidth = Editor.EasyAnimatorGUI.CalculateWidth(EditorStyles.popup, "Threshold");
                return _StandardThresholdWidth;
            }
        }

         

        public MixerTransitionDrawer() : this(StandardThresholdWidth) { }

        protected MixerTransitionDrawer(float thresholdWidth) => ThresholdWidth = thresholdWidth;

         

        protected static SerializedProperty CurrentThresholds { get; private set; }

         

        protected override void GatherSubProperties(SerializedProperty property)
        {
            base.GatherSubProperties(property);

            if (CurrentStates == null)
                return;

            CurrentThresholds = property.FindPropertyRelative(MixerTransition2D.ThresholdsField);

            if (CurrentThresholds == null)
                return;

            var count = Math.Max(CurrentStates.arraySize, CurrentThresholds.arraySize);
            CurrentStates.arraySize = count;
            CurrentThresholds.arraySize = count;
            if (CurrentSpeeds != null &&
                CurrentSpeeds.arraySize != 0)
                CurrentSpeeds.arraySize = count;
        }

         

        protected void SplitListRect(Rect area, bool isHeader, out Rect animation, out Rect threshold, out Rect speed, out Rect sync)
        {
            SplitListRect(area, isHeader, out animation, out speed, out sync);

            threshold = animation;

            var xMin = threshold.xMin = EditorGUIUtility.labelWidth + Editor.EasyAnimatorGUI.IndentSize;

            animation.xMax = xMin - Editor.EasyAnimatorGUI.StandardSpacing;
        }

         

        protected override void DoStateListHeaderGUI(Rect area)
        {
            SplitListRect(area, true, out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

            DoAnimationHeaderGUI(animationArea);

            using (ObjectPool.Disposable.AcquireContent(out var label, "Threshold",
                "The parameter values at which each child state will be fully active"))
                DoHeaderDropdownGUI(thresholdArea, CurrentThresholds, label, AddThresholdFunctionsToMenu);

            DoSpeedHeaderGUI(speedArea);

            DoSyncHeaderGUI(syncArea);
        }

         

        protected override void DoElementGUI(Rect area, int index,
            SerializedProperty clip, SerializedProperty speed)
        {
            SplitListRect(area, false, out var animationArea, out var thresholdArea, out var speedArea, out var syncArea);

            DoElementGUI(animationArea, speedArea, syncArea, index, clip, speed);

            DoThresholdGUI(thresholdArea, index);
        }

         

        protected virtual void DoThresholdGUI(Rect area, int index)
        {
            var threshold = CurrentThresholds.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(area, threshold, GUIContent.none);
        }

         

        protected override void OnAddElement(ReorderableList list)
        {
            var index = CurrentStates.arraySize;
            base.OnAddElement(list);
            CurrentThresholds.InsertArrayElementAtIndex(index);
        }

         

        protected override void OnRemoveElement(ReorderableList list)
        {
            base.OnRemoveElement(list);
            Editor.Serialization.RemoveArrayElement(CurrentThresholds, list.index);
        }

         

        protected override void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
        {
            base.OnReorderList(list, oldIndex, newIndex);
            CurrentThresholds.MoveArrayElement(oldIndex, newIndex);
        }

         

        protected virtual void AddThresholdFunctionsToMenu(GenericMenu menu) { }

         
    }
#endif
}
