
#if UNITY_EDITOR

using EasyAnimator.Units;
using System;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
     
    [CustomPropertyDrawer(typeof(ITransitionDetailed), true)]
    public class TransitionDrawer : PropertyDrawer
    {
         

        private enum Mode
        {
            Uninitialized,
            Normal,
            AlwaysExpanded,
        }

        private Mode _Mode;

         

        protected readonly string MainPropertyName;

        protected readonly string MainPropertyPathSuffix;

         

        public TransitionDrawer() { }

        public TransitionDrawer(string mainPropertyName)
        {
            MainPropertyName = mainPropertyName;
            MainPropertyPathSuffix = "." + mainPropertyName;
        }

         

        private SerializedProperty GetMainProperty(SerializedProperty rootProperty)
        {
            if (MainPropertyName == null)
                return null;
            else
                return rootProperty.FindPropertyRelative(MainPropertyName);
        }

         

        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

         

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            using (DrawerContext.Get(property))
            {
                InitializeMode(property);

                var height = EditorGUI.GetPropertyHeight(property, label, true);

                if (property.isExpanded)
                {
                    if (property.propertyType != SerializedPropertyType.ManagedReference)
                    {
                        var mainProperty = GetMainProperty(property);
                        if (mainProperty != null)
                            height -= EditorGUI.GetPropertyHeight(mainProperty) + EasyAnimatorGUI.StandardSpacing;
                    }

                    // The End Time from the Event Sequence is drawn out in the main transition so we need to add it.
                    // But rather than figuring out which array element actually holds the end time, we just use the
                    // Start Time field since it will have the same height.
                    var startTime = property.FindPropertyRelative(NormalizedStartTimeFieldName);
                    if (startTime != null)
                        height += EditorGUI.GetPropertyHeight(startTime) + EasyAnimatorGUI.StandardSpacing;
                }

                return height;
            }
        }

         

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            InitializeMode(property);

            // Highlight the whole area if this transition is currently being previewed.
            var isPreviewing = TransitionPreviewWindow.IsPreviewing(property);
            if (isPreviewing)
            {
                var highlightArea = area;
                highlightArea.xMin -= EasyAnimatorGUI.IndentSize;
                EditorGUI.DrawRect(highlightArea, new Color(0.35f, 0.5f, 1, 0.2f));
            }

            var headerArea = area;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref headerArea, property, isPreviewing);

            using (new TypeSelectionButton(headerArea, property, true))
            {
                DoPropertyGUI(area, property, label, isPreviewing);
            }
        }

         

        private void DoPropertyGUI(Rect area, SerializedProperty property, GUIContent label, bool isPreviewing)
        {
            using (DrawerContext.Get(property))
            {
                if (Context.Transition == null)
                {
                    EditorGUI.PrefixLabel(area, label);
                    return;
                }

                EditorGUI.BeginChangeCheck();

                var mainProperty = GetMainProperty(property);
                DoHeaderGUI(ref area, property, mainProperty, label, isPreviewing);
                DoChildPropertiesGUI(area, property, mainProperty);

                if (EditorGUI.EndChangeCheck() && isPreviewing)
                    TransitionPreviewWindow.PreviewNormalizedTime = TransitionPreviewWindow.PreviewNormalizedTime;
            }
        }

         

        protected void InitializeMode(SerializedProperty property)
        {
            if (_Mode == Mode.Uninitialized)
            {
                _Mode = Mode.AlwaysExpanded;

                var iterator = property.serializedObject.GetIterator();
                iterator.Next(true);

                var count = 0;
                do
                {
                    switch (iterator.propertyPath)
                    {
                        case "m_ObjectHideFlags":
                        case "m_Script":
                            break;

                        default:
                            count++;
                            if (count > 1)
                            {
                                _Mode = Mode.Normal;
                                return;
                            }
                            break;
                    }
                }
                while (iterator.NextVisible(false));
            }

            if (_Mode == Mode.AlwaysExpanded)
                property.isExpanded = true;
        }

         

        public void DoHeaderGUI(ref Rect area, SerializedProperty rootProperty, SerializedProperty mainProperty,
            GUIContent label, bool isPreviewing)
        {
            area.height = EasyAnimatorGUI.LineHeight;
            var labelArea = area;
            EasyAnimatorGUI.NextVerticalArea(ref area);

            if (rootProperty.propertyType != SerializedPropertyType.ManagedReference)
                DoPreviewButtonGUI(ref labelArea, rootProperty, isPreviewing);

            // Drawing the main property might assign its details to the label,
            // so we need to keep them to draw the root property afterwards so the foldout doesn't steal input from it.
            using (ObjectPool.Disposable.AcquireContent(out var rootLabel, label.text, label.tooltip))
            {
                // Main Property.
                DoMainPropertyGUI(labelArea, out labelArea, rootProperty, mainProperty);

                // Root Property.
                if (_Mode != Mode.AlwaysExpanded)
                {
                    EditorGUI.PropertyField(labelArea, rootProperty, rootLabel, false);
                }
                else
                {
                    rootLabel = EditorGUI.BeginProperty(labelArea, rootLabel, rootProperty);
                    EditorGUI.LabelField(labelArea, rootLabel);
                    EditorGUI.EndProperty();
                }
            }
        }

         

        private void DoMainPropertyGUI(Rect area, out Rect labelArea, SerializedProperty rootProperty, SerializedProperty mainProperty)
        {
            labelArea = area;
            if (mainProperty == null)
                return;

            var fullArea = area;
            labelArea = EasyAnimatorGUI.StealFromLeft(ref area, EditorGUIUtility.labelWidth, EasyAnimatorGUI.StandardSpacing);

            var mainPropertyReferenceIsMissing =
                mainProperty.propertyType == SerializedPropertyType.ObjectReference &&
                mainProperty.objectReferenceValue == null;

            var hierarchyMode = EditorGUIUtility.hierarchyMode;
            EditorGUIUtility.hierarchyMode = true;

            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (rootProperty.isExpanded || _Mode == Mode.AlwaysExpanded)
                {
                    EditorGUI.indentLevel++;

                    EasyAnimatorGUI.NextVerticalArea(ref fullArea);
                    using (ObjectPool.Disposable.AcquireContent(out var label, mainProperty))
                        EditorGUI.PropertyField(fullArea, mainProperty, label, true);

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.PropertyField(area, mainProperty, GUIContent.none, true);

                EditorGUI.indentLevel = indentLevel;
            }

            EditorGUIUtility.hierarchyMode = hierarchyMode;

            // If the main Object reference was just assigned and all fields were at their type default,
            // reset the value to run its default constructor and field initializers then reassign the reference.
            var reference = mainProperty.objectReferenceValue;
            if (mainPropertyReferenceIsMissing && reference != null)
            {
                mainProperty.objectReferenceValue = null;
                if (Serialization.IsDefaultValueByType(rootProperty))
                    rootProperty.GetAccessor().ResetValue(rootProperty);
                mainProperty.objectReferenceValue = reference;
            }
        }

         

        private static void DoPreviewButtonGUI(ref Rect area, SerializedProperty property, bool isPreviewing)
        {
            if (property.serializedObject.targetObjects.Length != 1 ||
                !TransitionPreviewWindow.CanBePreviewed(property))
                return;

            var enabled = GUI.enabled;
            var currentEvent = Event.current;
            if (currentEvent.button == 1)// Ignore Right Clicks on the Preview Button.
            {
                switch (currentEvent.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                    case EventType.ContextClick:
                        GUI.enabled = false;
                        break;
                }
            }

            var tooltip = isPreviewing ? TransitionPreviewWindow.Inspector.CloseTooltip : "Preview this transition";

            if (DoPreviewButtonGUI(ref area, isPreviewing, tooltip))
                TransitionPreviewWindow.OpenOrClose(property);

            GUI.enabled = enabled;
        }

        public static bool DoPreviewButtonGUI(ref Rect area, bool selected, string tooltip)
        {
            var width = EasyAnimatorGUI.LineHeight + EasyAnimatorGUI.StandardSpacing * 2;
            var buttonArea = EasyAnimatorGUI.StealFromRight(ref area, width, EasyAnimatorGUI.StandardSpacing);
            buttonArea.height = EasyAnimatorGUI.LineHeight;

            using (ObjectPool.Disposable.AcquireContent(out var content, "", tooltip))
            {
                content.image = TransitionPreviewWindow.Icon;

                return GUI.Toggle(buttonArea, selected, content, PreviewButtonStyle) != selected;
            }
        }

         

        private static GUIStyle _PreviewButtonStyle;

        public static GUIStyle PreviewButtonStyle
        {
            get
            {
                if (_PreviewButtonStyle == null)
                {
                    _PreviewButtonStyle = new GUIStyle(EasyAnimatorGUI.MiniButton)
                    {
                        padding = new RectOffset(0, 0, 0, 1),
                        fixedWidth = 0,
                        fixedHeight = 0,
                    };
                }

                return _PreviewButtonStyle;
            }
        }

         

        private void DoChildPropertiesGUI(Rect area, SerializedProperty rootProperty, SerializedProperty mainProperty)
        {
            if (!rootProperty.isExpanded && _Mode != Mode.AlwaysExpanded)
                return;

            // Skip over the main property if it was already drawn by the header.
            if (rootProperty.propertyType == SerializedPropertyType.ManagedReference &&
                mainProperty != null)
                EasyAnimatorGUI.NextVerticalArea(ref area);

            EditorGUI.indentLevel++;

            var property = rootProperty.Copy();

            SerializedProperty eventsProperty = null;

            var depth = property.depth;
            property.NextVisible(true);
            while (property.depth > depth)
            {
                // Grab the Events property and draw it last.
                var path = property.propertyPath;
                if (eventsProperty == null && path.EndsWith("._Events"))
                {
                    eventsProperty = property.Copy();
                }
                // Don't draw the main property again.
                else if (mainProperty != null && path.EndsWith(MainPropertyPathSuffix))
                {
                }
                else
                {
                    if (eventsProperty != null)
                    {
                        var type = Context.Transition.GetType();
                        var accessor = property.GetAccessor();
                        var field = Serialization.GetField(type, accessor.Name);
                        if (field != null && field.IsDefined(typeof(DrawAfterEventsAttribute), false))
                        {
                            using (ObjectPool.Disposable.AcquireContent(out var eventsLabel, eventsProperty))
                                DoChildPropertyGUI(ref area, rootProperty, eventsProperty, eventsLabel);
                            EasyAnimatorGUI.NextVerticalArea(ref area);
                            eventsProperty = null;
                        }
                    }

                    using (ObjectPool.Disposable.AcquireContent(out var label, property))
                        DoChildPropertyGUI(ref area, rootProperty, property, label);
                    EasyAnimatorGUI.NextVerticalArea(ref area);
                }

                if (!property.NextVisible(false))
                    break;
            }

            if (eventsProperty != null)
            {
                using (ObjectPool.Disposable.AcquireContent(out var label, eventsProperty))
                    DoChildPropertyGUI(ref area, rootProperty, eventsProperty, label);
            }

            EditorGUI.indentLevel--;
        }

         

        protected virtual void DoChildPropertyGUI(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            // If we keep using the GUIContent that was passed into OnGUI then GetPropertyHeight will change it to
            // match the 'property' which we don't want.

            using (ObjectPool.Disposable.AcquireContent(out var content, label.text, label.tooltip, false))
            {
                area.height = EditorGUI.GetPropertyHeight(property, content, true);

                if (TryDoStartTimeField(ref area, rootProperty, property, content))
                    return;

                if (!EditorGUIUtility.hierarchyMode)
                    EditorGUI.indentLevel++;

                EditorGUI.PropertyField(area, property, content, true);

                if (!EditorGUIUtility.hierarchyMode)
                    EditorGUI.indentLevel--;
            }
        }

         

        public const string NormalizedStartTimeFieldName = "_NormalizedStartTime";

        public static bool TryDoStartTimeField(ref Rect area, SerializedProperty rootProperty,
            SerializedProperty property, GUIContent label)
        {
            if (!property.propertyPath.EndsWith("." + NormalizedStartTimeFieldName))
                return false;

            // Start Time.
            label.text = EasyAnimatorGUI.GetNarrowText("Start Time");
            AnimationTimeAttribute.nextDefaultValue = EasyAnimatorEvent.Sequence.GetDefaultNormalizedStartTime(Context.Transition.Speed);
            EditorGUI.PropertyField(area, property, label, false);

            EasyAnimatorGUI.NextVerticalArea(ref area);

            // End Time.
            var events = rootProperty.FindPropertyRelative("_Events");
            using (var context = SerializableEventSequenceDrawer.Context.Get(events))
            {
                var areaCopy = area;
                var index = Mathf.Max(0, context.Times.Count - 1);
                SerializableEventSequenceDrawer.DoTimeGUI(ref areaCopy, context, index, true);
            }

            return true;
        }

         
        #region Context
         

        public static DrawerContext Context => DrawerContext.Stack.Current;

         

      
        public sealed class DrawerContext : IDisposable
        {
             

            public SerializedProperty Property { get; private set; }

            public ITransitionDetailed Transition { get; private set; }

            public float MaximumDuration { get; private set; }

             

            public static readonly LazyStack<DrawerContext> Stack = new LazyStack<DrawerContext>();

            public static IDisposable Get(SerializedProperty transitionProperty)
            {
                var context = Stack.Increment();

                context.Property = transitionProperty;
                context.Transition = transitionProperty.GetValue<ITransitionDetailed>();

                EasyAnimatorUtilities.TryGetLength(context.Transition, out var length);
                context.MaximumDuration = length;

                EditorGUI.BeginChangeCheck();

                return context;
            }

             

            public void Dispose()
            {
                var context = Stack.Current;

                if (EditorGUI.EndChangeCheck())
                    context.Property.serializedObject.ApplyModifiedProperties();

                context.Property = null;
                context.Transition = null;

                Stack.Decrement();
            }

             
        }

         
        #endregion
         
    }
}

#endif

