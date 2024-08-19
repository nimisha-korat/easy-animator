

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace EasyAnimator
{

    [Serializable]
    public class ManualMixerTransition : ManualMixerTransition<ManualMixerState>, ManualMixerState.ITransition
    {
         

        public override ManualMixerState CreateState()
        {
            State = new ManualMixerState();
            InitializeState();
            return State;
        }

         
#if UNITY_EDITOR
         

        [CustomPropertyDrawer(typeof(ManualMixerTransition), true)]
        public class Drawer : Editor.TransitionDrawer
        {
             

            public static SerializedProperty CurrentProperty { get; private set; }

            public static SerializedProperty CurrentStates { get; private set; }

            public static SerializedProperty CurrentSpeeds { get; private set; }

            public static SerializedProperty CurrentSynchronizeChildren { get; private set; }

            private readonly Dictionary<string, ReorderableList>
                PropertyPathToStates = new Dictionary<string, ReorderableList>();

             

            protected virtual ReorderableList GatherDetails(SerializedProperty property)
            {
                InitializeMode(property);
                GatherSubProperties(property);

                if (CurrentStates == null)
                    return null;

                var path = property.propertyPath;

                if (!PropertyPathToStates.TryGetValue(path, out var states))
                {
                    states = new ReorderableList(CurrentStates.serializedObject, CurrentStates)
                    {
                        drawHeaderCallback = DoStateListHeaderGUI,
                        elementHeightCallback = GetElementHeight,
                        drawElementCallback = DoElementGUI,
                        onAddCallback = OnAddElement,
                        onRemoveCallback = OnRemoveElement,
                        onReorderCallbackWithDetails = OnReorderList,
                    };

                    PropertyPathToStates.Add(path, states);
                }

                states.serializedProperty = CurrentStates;

                return states;
            }

             

            protected virtual void GatherSubProperties(SerializedProperty property)
            {
                CurrentProperty = property;
                CurrentStates = property.FindPropertyRelative(StatesField);
                CurrentSpeeds = property.FindPropertyRelative(SpeedsField);
                CurrentSynchronizeChildren = property.FindPropertyRelative(SynchronizeChildrenField);

                if (CurrentStates != null &&
                    CurrentSpeeds != null &&
                    CurrentSpeeds.arraySize != 0)
                    CurrentSpeeds.arraySize = CurrentStates.arraySize;
            }

             

            protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                Editor.MenuFunctionState state, Action<SerializedProperty> function)
            {
                Editor.Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, state, (property) =>
                {
                    GatherSubProperties(property);
                    function(property);
                });
            }

            protected void AddPropertyModifierFunction(GenericMenu menu, string label,
                Action<SerializedProperty> function)
            {
                Editor.Serialization.AddPropertyModifierFunction(menu, CurrentProperty, label, (property) =>
                {
                    GatherSubProperties(property);
                    function(property);
                });
            }

             

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var height = EditorGUI.GetPropertyHeight(property, label);

                if (property.isExpanded)
                {
                    var states = GatherDetails(property);
                    if (states != null)
                        height += Editor.EasyAnimatorGUI.StandardSpacing + states.GetHeight();
                }

                return height;
            }

             

            public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
            {
                var originalProperty = property.Copy();

                base.OnGUI(area, property, label);

                if (!originalProperty.isExpanded)
                    return;

                using (DrawerContext.Get(property))
                {
                    if (Context.Transition == null)
                        return;

                    var states = GatherDetails(originalProperty);

                    var indentLevel = EditorGUI.indentLevel;

                    area.yMin = area.yMax - states.GetHeight();

                    EditorGUI.indentLevel++;
                    area = EditorGUI.IndentedRect(area);

                    EditorGUI.indentLevel = 0;
                    states.DoList(area);

                    EditorGUI.indentLevel = indentLevel;

                    TryCollapseArrays();
                }
            }

             

            private static float _SpeedLabelWidth;
            private static float _SyncLabelWidth;

            protected static void SplitListRect(Rect area, bool isHeader, out Rect animation, out Rect speed, out Rect sync)
            {
                if (_SpeedLabelWidth == 0)
                    _SpeedLabelWidth = Editor.EasyAnimatorGUI.CalculateWidth(EditorStyles.popup, "Speed");

                if (_SyncLabelWidth == 0)
                    _SyncLabelWidth = Editor.EasyAnimatorGUI.CalculateWidth(EditorStyles.popup, "Sync");

                var spacing = Editor.EasyAnimatorGUI.StandardSpacing;

                var syncWidth = isHeader ?
                    _SyncLabelWidth :
                    Editor.EasyAnimatorGUI.ToggleWidth - spacing;

                var speedWidth = _SpeedLabelWidth + _SyncLabelWidth - syncWidth;
                if (!isHeader)
                {
                    // Don't use Clamp because the max might be smaller than the min.
                    var max = Math.Max(area.height, area.width * 0.25f - 30);
                    speedWidth = Math.Min(speedWidth, max);
                }

                area.width += spacing;
                sync = Editor.EasyAnimatorGUI.StealFromRight(ref area, syncWidth, spacing);
                speed = Editor.EasyAnimatorGUI.StealFromRight(ref area, speedWidth, spacing);
                animation = area;
            }

             
            #region Headers
             

            protected virtual void DoStateListHeaderGUI(Rect area)
            {
                SplitListRect(area, true, out var animationArea, out var speedArea, out var syncArea);

                DoAnimationHeaderGUI(animationArea);
                DoSpeedHeaderGUI(speedArea);
                DoSyncHeaderGUI(syncArea);
            }

             

            protected static void DoAnimationHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Animation",
                    $"The {nameof(AnimationClip)}s or {nameof(ITransition)}s that will be used for each child state"))
                {
                    DoHeaderDropdownGUI(area, CurrentStates, label, null);
                }
            }

             
            #region Speeds
             

            protected void DoSpeedHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Speed", Strings.Tooltips.Speed))
                {
                    DoHeaderDropdownGUI(area, CurrentSpeeds, label, (menu) =>
                    {
                        AddPropertyModifierFunction(menu, "Reset All to 1",
                            CurrentSpeeds.arraySize == 0 ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal,
                            (_) => CurrentSpeeds.arraySize = 0);

                        AddPropertyModifierFunction(menu, "Normalize Durations", Editor.MenuFunctionState.Normal, NormalizeDurations);
                    });
                }
            }

             

            private static void NormalizeDurations(SerializedProperty property)
            {
                var speedCount = CurrentSpeeds.arraySize;

                var lengths = new float[CurrentStates.arraySize];
                if (lengths.Length <= 1)
                    return;

                int nonZeroLengths = 0;
                float totalLength = 0;
                float totalSpeed = 0;
                for (int i = 0; i < lengths.Length; i++)
                {
                    var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (EasyAnimatorUtilities.TryGetLength(state, out var length) &&
                        length > 0)
                    {
                        nonZeroLengths++;
                        totalLength += length;
                        lengths[i] = length;

                        if (speedCount > 0)
                            totalSpeed += CurrentSpeeds.GetArrayElementAtIndex(i).floatValue;
                    }
                }

                if (nonZeroLengths == 0)
                    return;

                var averageLength = totalLength / nonZeroLengths;
                var averageSpeed = speedCount > 0 ? totalSpeed / nonZeroLengths : 1;

                CurrentSpeeds.arraySize = lengths.Length;
                InitializeSpeeds(speedCount);

                for (int i = 0; i < lengths.Length; i++)
                {
                    if (lengths[i] == 0)
                        continue;

                    CurrentSpeeds.GetArrayElementAtIndex(i).floatValue = averageSpeed * lengths[i] / averageLength;
                }

                TryCollapseArrays();
            }

             

            public static void InitializeSpeeds(int start)
            {
                var count = CurrentSpeeds.arraySize;
                while (start < count)
                    CurrentSpeeds.GetArrayElementAtIndex(start++).floatValue = 1;
            }

             
            #endregion
             
            #region Sync
             

            protected void DoSyncHeaderGUI(Rect area)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, "Sync",
                    "Determines which child states have their normalized times constantly synchronized"))
                {
                    DoHeaderDropdownGUI(area, CurrentSpeeds, label, (menu) =>
                    {
                        var syncCount = CurrentSynchronizeChildren.arraySize;

                        var allState = syncCount == 0 ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "All", allState,
                            (_) => CurrentSynchronizeChildren.arraySize = 0);

                        var syncNone = syncCount == CurrentStates.arraySize;
                        if (syncNone)
                        {
                            for (int i = 0; i < syncCount; i++)
                            {
                                if (CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue)
                                {
                                    syncNone = false;
                                    break;
                                }
                            }
                        }
                        var noneState = syncNone ? Editor.MenuFunctionState.Selected : Editor.MenuFunctionState.Normal;
                        AddPropertyModifierFunction(menu, "None", noneState, (_) =>
                        {
                            var count = CurrentSynchronizeChildren.arraySize = CurrentStates.arraySize;
                            for (int i = 0; i < count; i++)
                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Invert", Editor.MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentSynchronizeChildren.arraySize;
                            for (int i = 0; i < count; i++)
                            {
                                var property = CurrentSynchronizeChildren.GetArrayElementAtIndex(i);
                                property.boolValue = !property.boolValue;
                            }

                            var newCount = CurrentSynchronizeChildren.arraySize = CurrentStates.arraySize;
                            for (int i = count; i < newCount; i++)
                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
                        });

                        AddPropertyModifierFunction(menu, "Non-Stationary", Editor.MenuFunctionState.Normal, (_) =>
                        {
                            var count = CurrentStates.arraySize;

                            for (int i = 0; i < count; i++)
                            {
                                var state = CurrentStates.GetArrayElementAtIndex(i).objectReferenceValue;
                                if (state == null)
                                    continue;

                                if (i >= syncCount)
                                {
                                    CurrentSynchronizeChildren.arraySize = i + 1;
                                    for (int j = syncCount; j < i; j++)
                                        CurrentSynchronizeChildren.GetArrayElementAtIndex(j).boolValue = true;
                                    syncCount = i + 1;
                                }

                                CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue =
                                    EasyAnimatorUtilities.TryGetAverageVelocity(state, out var velocity) &&
                                    velocity != default;
                            }

                            TryCollapseSync();
                        });
                    });
                }
            }

             

            private static void SyncNone()
            {
                var count = CurrentSynchronizeChildren.arraySize = CurrentStates.arraySize;
                for (int i = 0; i < count; i++)
                    CurrentSynchronizeChildren.GetArrayElementAtIndex(i).boolValue = false;
            }

             
            #endregion
             

            public static void DoHeaderDropdownGUI(Rect area, SerializedProperty property, GUIContent content,
                Action<GenericMenu> populateMenu)
            {
                if (property != null)
                    EditorGUI.BeginProperty(area, GUIContent.none, property);

                if (populateMenu != null)
                {
                    if (EditorGUI.DropdownButton(area, content, FocusType.Passive))
                    {
                        var menu = new GenericMenu();
                        populateMenu(menu);
                        menu.ShowAsContext();
                    }
                }
                else
                {
                    GUI.Label(area, content);
                }

                if (property != null)
                    EditorGUI.EndProperty();
            }

             
            #endregion
             

            protected virtual float GetElementHeight(int index) => Editor.EasyAnimatorGUI.LineHeight;

             

            private void DoElementGUI(Rect area, int index, bool isActive, bool isFocused)
            {
                if (index < 0 || index > CurrentStates.arraySize)
                    return;

                var state = CurrentStates.GetArrayElementAtIndex(index);
                var speed = CurrentSpeeds.arraySize > 0 ? CurrentSpeeds.GetArrayElementAtIndex(index) : null;
                DoElementGUI(area, index, state, speed);
            }

             

            protected virtual void DoElementGUI(Rect area, int index,
                SerializedProperty state, SerializedProperty speed)
            {
                SplitListRect(area, false, out var animationArea, out var speedArea, out var syncArea);

                DoElementGUI(animationArea, speedArea, syncArea, index, state, speed);
            }

            protected void DoElementGUI(Rect animationArea, Rect speedArea, Rect syncArea, int index,
                SerializedProperty state, SerializedProperty speed)
            {
                DoClipOrTransitionField(animationArea, state, GUIContent.none);

                if (speed != null)
                {
                    EditorGUI.PropertyField(speedArea, speed, GUIContent.none);
                }
                else
                {
                    EditorGUI.BeginProperty(speedArea, GUIContent.none, CurrentSpeeds);

                    var value = EditorGUI.FloatField(speedArea, 1);
                    if (value != 1)
                    {
                        CurrentSpeeds.InsertArrayElementAtIndex(0);
                        CurrentSpeeds.GetArrayElementAtIndex(0).floatValue = 1;
                        CurrentSpeeds.arraySize = CurrentStates.arraySize;
                        CurrentSpeeds.GetArrayElementAtIndex(index).floatValue = value;
                    }

                    EditorGUI.EndProperty();
                }

                DoSyncToggleGUI(syncArea, index);
            }

             

            public static void DoClipOrTransitionField(Rect area, SerializedProperty property, GUIContent label)
            {
                var targetObject = property.serializedObject.targetObject;
                var oldReference = property.objectReferenceValue;

                EditorGUI.BeginChangeCheck();
                EditorGUI.ObjectField(area, property, label);
                if (EditorGUI.EndChangeCheck())
                {
                    var newReference = property.objectReferenceValue;
                    if (newReference == null || !IsClipOrTransition(newReference) || newReference == targetObject)
                        property.objectReferenceValue = oldReference;
                }
            }

            public static bool IsClipOrTransition(object clipOrTransition)
                => clipOrTransition is AnimationClip || clipOrTransition is ITransition;

             

            protected void DoSyncToggleGUI(Rect area, int index)
            {
                var syncProperty = CurrentSynchronizeChildren;
                var syncFlagCount = syncProperty.arraySize;

                var enabled = true;

                if (index < syncFlagCount)
                {
                    syncProperty = syncProperty.GetArrayElementAtIndex(index);
                    enabled = syncProperty.boolValue;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginProperty(area, GUIContent.none, syncProperty);

                enabled = GUI.Toggle(area, enabled, GUIContent.none);

                EditorGUI.EndProperty();
                if (EditorGUI.EndChangeCheck())
                {
                    if (index < syncFlagCount)
                    {
                        syncProperty.boolValue = enabled;
                    }
                    else
                    {
                        syncProperty.arraySize = index + 1;

                        for (int i = syncFlagCount; i < index; i++)
                        {
                            syncProperty.GetArrayElementAtIndex(i).boolValue = true;
                        }

                        syncProperty.GetArrayElementAtIndex(index).boolValue = enabled;
                    }
                }
            }

             

            protected virtual void OnAddElement(ReorderableList list)
            {
                var index = CurrentStates.arraySize;
                CurrentStates.InsertArrayElementAtIndex(index);

                if (CurrentSpeeds.arraySize > 0)
                    CurrentSpeeds.InsertArrayElementAtIndex(index);
            }

             

            protected virtual void OnRemoveElement(ReorderableList list)
            {
                var index = list.index;

                Editor.Serialization.RemoveArrayElement(CurrentStates, index);

                if (CurrentSpeeds.arraySize > 0)
                    Editor.Serialization.RemoveArrayElement(CurrentSpeeds, index);
            }

             

            protected virtual void OnReorderList(ReorderableList list, int oldIndex, int newIndex)
            {
                CurrentSpeeds.MoveArrayElement(oldIndex, newIndex);

                var syncCount = CurrentSynchronizeChildren.arraySize;
                if (Math.Max(oldIndex, newIndex) >= syncCount)
                {
                    CurrentSynchronizeChildren.arraySize++;
                    CurrentSynchronizeChildren.GetArrayElementAtIndex(syncCount).boolValue = true;
                    CurrentSynchronizeChildren.arraySize = newIndex + 1;
                }

                CurrentSynchronizeChildren.MoveArrayElement(oldIndex, newIndex);
            }

             

            public static void TryCollapseArrays()
            {
                TryCollapseSpeeds();
                TryCollapseSync();
            }

             

            public static void TryCollapseSpeeds()
            {
                var property = CurrentSpeeds;
                var speedCount = property.arraySize;
                if (speedCount <= 0)
                    return;

                for (int i = 0; i < speedCount; i++)
                {
                    if (property.GetArrayElementAtIndex(i).floatValue != 1)
                        return;
                }

                property.arraySize = 0;
            }

             

            public static void TryCollapseSync()
            {
                var property = CurrentSynchronizeChildren;
                var count = property.arraySize;
                var changed = false;

                for (int i = count - 1; i >= 0; i--)
                {
                    if (property.GetArrayElementAtIndex(i).boolValue)
                    {
                        count = i;
                        changed = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (changed)
                    property.arraySize = count;
            }

             
        }

         
#endif
         
    }
}
