

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace EasyAnimator.Editor

{
    public enum MenuFunctionState
    {
         

        Normal,

        Selected,

        Disabled,

         
    }

    public static partial class Serialization
    {
         
        #region Public Static API
         

        public const string
            ArrayDataPrefix = ".Array.data[",
            ArrayDataSuffix = "]";

        public const BindingFlags
            InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

         

        public static string GetFriendlyPath(this SerializedProperty property)
        {
            return property.propertyPath.Replace(ArrayDataPrefix, "[");
        }

         
        #region Get Value
         

        public static object GetValue(this SerializedProperty property, object targetObject)
        {
            if (property.hasMultipleDifferentValues &&
                property.serializedObject.targetObject != targetObject as Object)
            {
                property = new SerializedObject(targetObject as Object).FindProperty(property.propertyPath);
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: return property.boolValue;
                case SerializedPropertyType.Float: return property.floatValue;
                case SerializedPropertyType.String: return property.stringValue;

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    return property.intValue;

                case SerializedPropertyType.Vector2: return property.vector2Value;
                case SerializedPropertyType.Vector3: return property.vector3Value;
                case SerializedPropertyType.Vector4: return property.vector4Value;

                case SerializedPropertyType.Quaternion: return property.quaternionValue;
                case SerializedPropertyType.Color: return property.colorValue;
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue;

                case SerializedPropertyType.Rect: return property.rectValue;
                case SerializedPropertyType.Bounds: return property.boundsValue;

                case SerializedPropertyType.Vector2Int: return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int: return property.vector3IntValue;
                case SerializedPropertyType.RectInt: return property.rectIntValue;
                case SerializedPropertyType.BoundsInt: return property.boundsIntValue;

                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue;
                case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue;

                case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize;

                case SerializedPropertyType.Gradient: return property.GetGradientValue();

                case SerializedPropertyType.Enum:// Would be complex because enumValueIndex can't be cast directly.
                case SerializedPropertyType.Generic:
                default:
                    return GetAccessor(property)?.GetValue(targetObject);
            }
        }

         

        public static object GetValue(this SerializedProperty property) => GetValue(property, property.serializedObject.targetObject);

        public static T GetValue<T>(this SerializedProperty property) => (T)GetValue(property);

        public static void GetValue<T>(this SerializedProperty property, out T value) => value = (T)GetValue(property);

         

        public static T[] GetValues<T>(this SerializedProperty property)
        {
            try
            {
                var targetObjects = property.serializedObject.targetObjects;
                var values = new T[targetObjects.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (T)GetValue(property, targetObjects[i]);
                }

                return values;
            }
            catch
            {
                return null;
            }
        }

         

        public static bool IsDefaultValueByType(SerializedProperty property)
        {
            if (property.hasMultipleDifferentValues)
                return false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: return property.boolValue == default;
                case SerializedPropertyType.Float: return property.floatValue == default;
                case SerializedPropertyType.String: return property.stringValue == "";

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    return property.intValue == default;

                case SerializedPropertyType.Vector2: return property.vector2Value == default;
                case SerializedPropertyType.Vector3: return property.vector3Value == default;
                case SerializedPropertyType.Vector4: return property.vector4Value == default;

                case SerializedPropertyType.Quaternion: return property.quaternionValue == default;
                case SerializedPropertyType.Color: return property.colorValue == default;
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue == default;

                case SerializedPropertyType.Rect: return property.rectValue == default;
                case SerializedPropertyType.Bounds: return property.boundsValue == default;

                case SerializedPropertyType.Vector2Int: return property.vector2IntValue == default;
                case SerializedPropertyType.Vector3Int: return property.vector3IntValue == default;
                case SerializedPropertyType.RectInt: return property.rectIntValue.Equals(default);
                case SerializedPropertyType.BoundsInt: return property.boundsIntValue == default;

                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue == default;
                case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue == default;

                case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize == default;

                case SerializedPropertyType.Enum: return property.enumValueIndex == default;

                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Generic:
                default:
                    if (property.isArray)
                        return property.arraySize == default;

                    var depth = property.depth;
                    property = property.Copy();
                    var enterChildren = true;
                    while (property.Next(enterChildren) && property.depth > depth)
                    {
                        enterChildren = false;
                        if (!IsDefaultValueByType(property))
                            return false;
                    }

                    return true;
            }
        }

         
        #endregion
         
        #region Set Value
         

        public static void SetValue(this SerializedProperty property, object targetObject, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: property.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: property.floatValue = (float)value; break;
                case SerializedPropertyType.String: property.stringValue = (string)value; break;

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    property.intValue = (int)value; break;

                case SerializedPropertyType.Vector2: property.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3: property.vector3Value = (Vector3)value; break;
                case SerializedPropertyType.Vector4: property.vector4Value = (Vector4)value; break;

                case SerializedPropertyType.Quaternion: property.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.Color: property.colorValue = (Color)value; break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = (AnimationCurve)value; break;

                case SerializedPropertyType.Rect: property.rectValue = (Rect)value; break;
                case SerializedPropertyType.Bounds: property.boundsValue = (Bounds)value; break;

                case SerializedPropertyType.Vector2Int: property.vector2IntValue = (Vector2Int)value; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = (Vector3Int)value; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = (RectInt)value; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = (BoundsInt)value; break;

                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = (Object)value; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = (Object)value; break;

                case SerializedPropertyType.FixedBufferSize:
                    throw new InvalidOperationException($"{nameof(SetValue)} failed:" +
                        $" {nameof(SerializedProperty)}.{nameof(SerializedProperty.fixedBufferSize)} is read-only.");

                case SerializedPropertyType.Gradient: property.SetGradientValue((Gradient)value); break;

                case SerializedPropertyType.Enum:// Would be complex because enumValueIndex can't be cast directly.
                case SerializedPropertyType.Generic:
                default:
                    var accessor = GetAccessor(property);
                    if (accessor != null)
                        accessor.SetValue(targetObject, value);
                    break;
            }
        }

         

        public static void SetValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: property.boolValue = (bool)value; break;
                case SerializedPropertyType.Float: property.floatValue = (float)value; break;
                case SerializedPropertyType.Integer: property.intValue = (int)value; break;
                case SerializedPropertyType.String: property.stringValue = (string)value; break;

                case SerializedPropertyType.Vector2: property.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3: property.vector3Value = (Vector3)value; break;
                case SerializedPropertyType.Vector4: property.vector4Value = (Vector4)value; break;

                case SerializedPropertyType.Quaternion: property.quaternionValue = (Quaternion)value; break;
                case SerializedPropertyType.Color: property.colorValue = (Color)value; break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = (AnimationCurve)value; break;

                case SerializedPropertyType.Rect: property.rectValue = (Rect)value; break;
                case SerializedPropertyType.Bounds: property.boundsValue = (Bounds)value; break;

                case SerializedPropertyType.Vector2Int: property.vector2IntValue = (Vector2Int)value; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = (Vector3Int)value; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = (RectInt)value; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = (BoundsInt)value; break;

                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = (Object)value; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = (Object)value; break;

                case SerializedPropertyType.ArraySize: property.intValue = (int)value; break;

                case SerializedPropertyType.FixedBufferSize:
                    throw new InvalidOperationException($"{nameof(SetValue)} failed:" +
                        $" {nameof(SerializedProperty)}.{nameof(SerializedProperty.fixedBufferSize)} is read-only.");

                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Character:
                default:
                    var accessor = GetAccessor(property);
                    if (accessor != null)
                    {
                        var targets = property.serializedObject.targetObjects;
                        for (int i = 0; i < targets.Length; i++)
                        {
                            accessor.SetValue(targets[i], value);
                        }
                    }
                    break;
            }
        }

         

        public static void ResetValue(SerializedProperty property, string undoName = "Inspector")
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: property.boolValue = default; break;
                case SerializedPropertyType.Float: property.floatValue = default; break;
                case SerializedPropertyType.String: property.stringValue = ""; break;

                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                    property.intValue = default;
                    break;

                case SerializedPropertyType.Vector2: property.vector2Value = default; break;
                case SerializedPropertyType.Vector3: property.vector3Value = default; break;
                case SerializedPropertyType.Vector4: property.vector4Value = default; break;

                case SerializedPropertyType.Quaternion: property.quaternionValue = default; break;
                case SerializedPropertyType.Color: property.colorValue = default; break;
                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = default; break;

                case SerializedPropertyType.Rect: property.rectValue = default; break;
                case SerializedPropertyType.Bounds: property.boundsValue = default; break;

                case SerializedPropertyType.Vector2Int: property.vector2IntValue = default; break;
                case SerializedPropertyType.Vector3Int: property.vector3IntValue = default; break;
                case SerializedPropertyType.RectInt: property.rectIntValue = default; break;
                case SerializedPropertyType.BoundsInt: property.boundsIntValue = default; break;

                case SerializedPropertyType.ObjectReference: property.objectReferenceValue = default; break;
                case SerializedPropertyType.ExposedReference: property.exposedReferenceValue = default; break;

                case SerializedPropertyType.Enum: property.enumValueIndex = default; break;

                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Generic:
                default:
                    if (property.isArray)
                    {
                        property.arraySize = default;
                        break;
                    }

                    var depth = property.depth;
                    property = property.Copy();
                    var enterChildren = true;
                    while (property.Next(enterChildren) && property.depth > depth)
                    {
                        enterChildren = false;
                        ResetValue(property);
                    }
                    break;
            }
        }

         

        public static float CopyValueFrom(this SerializedProperty to, SerializedProperty from)
        {
            from = from.Copy();
            var fromPath = from.propertyPath;
            var pathPrefixLength = fromPath.Length + 1;
            var depth = from.depth;

            var copyCount = 0;
            var totalCount = 0;
            StringBuilder issues = null;

            do
            {
                while (from.propertyType == SerializedPropertyType.Generic)
                    if (!from.Next(true))
                        goto LogResults;

                SerializedProperty toRelative;

                var relativePath = from.propertyPath;
                if (relativePath.Length <= pathPrefixLength)
                {
                    toRelative = to;
                }
                else
                {
                    relativePath = relativePath.Substring(pathPrefixLength, relativePath.Length - pathPrefixLength);

                    toRelative = to.FindPropertyRelative(relativePath);
                }

                if (!from.hasMultipleDifferentValues &&
                    toRelative != null &&
                    toRelative.propertyType == from.propertyType &&
                    toRelative.type == from.type)
                {
                    // GetValue and SetValue currently access the underlying field for enums, but we need the stored value.
                    if (toRelative.propertyType == SerializedPropertyType.Enum)
                        toRelative.enumValueIndex = from.enumValueIndex;
                    else
                        toRelative.SetValue(from.GetValue());

                    copyCount++;
                }
                else
                {
                    if (issues == null)
                        issues = new StringBuilder();

                    issues.AppendLine()
                        .Append(" - ");

                    if (from.hasMultipleDifferentValues)
                    {
                        issues
                            .Append("The selected objects have different values for '")
                            .Append(relativePath)
                            .Append("'.");
                    }
                    else if (toRelative == null)
                    {
                        issues
                            .Append("No property '")
                            .Append(relativePath)
                            .Append("' exists relative to '")
                            .Append(to.propertyPath)
                            .Append("'.");
                    }
                    else if (toRelative.propertyType != from.propertyType)
                    {
                        issues
                            .Append("The type of '")
                            .Append(toRelative.propertyPath)
                            .Append("' was '")
                            .Append(toRelative.propertyType)
                            .Append("' but should be '")
                            .Append(from.propertyType)
                            .Append("'.");
                    }
                    else if (toRelative.type != from.type)
                    {
                        issues
                            .Append("The type of '")
                            .Append(toRelative.propertyPath)
                            .Append("' was '")
                            .Append(toRelative.type)
                            .Append("' but should be '")
                            .Append(from.type)
                            .Append("'.");
                    }
                    else// This should never happen.
                    {
                        issues
                            .Append(" - Unknown issue with '")
                            .Append(relativePath)
                            .Append("'.");
                    }
                }

                totalCount++;
            }
            while (from.Next(false) && from.depth > depth);

            LogResults:
            if (copyCount < totalCount)
                Debug.Log($"Copied {copyCount} / {totalCount} values from '{fromPath}' to '{to.propertyPath}': {issues}");

            return (float)copyCount / totalCount;
        }

         
        #endregion
         
        #region Gradients
         

        private static PropertyInfo _GradientValue;

        private static PropertyInfo GradientValue
        {
            get
            {
                if (_GradientValue == null)
                    _GradientValue = typeof(SerializedProperty).GetProperty("gradientValue", InstanceBindings);

                return _GradientValue;
            }
        }

        public static Gradient GetGradientValue(this SerializedProperty property) => (Gradient)GradientValue.GetValue(property, null);

        public static void SetGradientValue(this SerializedProperty property, Gradient value) => GradientValue.SetValue(property, value, null);

         
        #endregion
         

        public static bool AreSameProperty(SerializedProperty a, SerializedProperty b)
        {
            if (a == b)
                return true;

            if (a == null)
                return b == null;

            if (b == null)
                return false;

            if (a.propertyPath != b.propertyPath)
                return false;

            var aTargets = a.serializedObject.targetObjects;
            var bTargets = b.serializedObject.targetObjects;
            if (aTargets.Length != bTargets.Length)
                return false;

            for (int i = 0; i < aTargets.Length; i++)
            {
                if (aTargets[i] != bTargets[i])
                    return false;
            }

            return true;
        }

         

        public static void ForEachTarget(this SerializedProperty property, Action<SerializedProperty> function,
            string undoName = "Inspector")
        {
            var targets = property.serializedObject.targetObjects;

            if (undoName != null)
                Undo.RecordObjects(targets, undoName);

            if (targets.Length == 1)
            {
                function(property);
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var path = property.propertyPath;
                for (int i = 0; i < targets.Length; i++)
                {
                    using (var serializedObject = new SerializedObject(targets[i]))
                    {
                        property = serializedObject.FindProperty(path);
                        function(property);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

         

        public static void AddFunction(this GenericMenu menu, string label, MenuFunctionState state, GenericMenu.MenuFunction function)
        {
            if (state != MenuFunctionState.Disabled)
            {
                menu.AddItem(new GUIContent(label), state == MenuFunctionState.Selected, function);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(label));
            }
        }

        public static void AddFunction(this GenericMenu menu, string label, bool enabled, GenericMenu.MenuFunction function)
            => AddFunction(menu, label, enabled ? MenuFunctionState.Normal : MenuFunctionState.Disabled, function);

         

        public static void AddPropertyModifierFunction(this GenericMenu menu, SerializedProperty property, string label,
            MenuFunctionState state, Action<SerializedProperty> function)
        {
            if (state != MenuFunctionState.Disabled && GUI.enabled)
            {
                menu.AddItem(new GUIContent(label), state == MenuFunctionState.Selected, () =>
                {
                    ForEachTarget(property, function);
                    GUIUtility.keyboardControl = 0;
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.editingTextField = false;
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(label));
            }
        }

        public static void AddPropertyModifierFunction(this GenericMenu menu, SerializedProperty property, string label, bool enabled,
            Action<SerializedProperty> function)
            => AddPropertyModifierFunction(menu, property, label, enabled ? MenuFunctionState.Normal : MenuFunctionState.Disabled, function);

        public static void AddPropertyModifierFunction(this GenericMenu menu, SerializedProperty property, string label,
            Action<SerializedProperty> function)
            => AddPropertyModifierFunction(menu, property, label, MenuFunctionState.Normal, function);

         

        public static void ModifyValues<T>(this SerializedProperty property, Action<T> method, string undoName = "Inspector")
        {
            RecordUndo(property, undoName);

            var values = GetValues<T>(property);
            for (int i = 0; i < values.Length; i++)
                method(values[i]);

            OnPropertyChanged(property);
        }

         

        public static void RecordUndo(this SerializedProperty property, string undoName = "Inspector")
            => Undo.RecordObjects(property.serializedObject.targetObjects, undoName);

         

        public static void OnPropertyChanged(this SerializedProperty property)
        {
            var targets = property.serializedObject.targetObjects;

            // If this change is made to a prefab, this makes sure that any instances in the scene will be updated.
            for (int i = 0; i < targets.Length; i++)
            {
                EditorUtility.SetDirty(targets[i]);
            }

            property.serializedObject.Update();
        }

         

        public static SerializedPropertyType GetPropertyType(Type type)
        {
            // Primitives.

            if (type == typeof(bool))
                return SerializedPropertyType.Boolean;

            if (type == typeof(int))
                return SerializedPropertyType.Integer;

            if (type == typeof(float))
                return SerializedPropertyType.Float;

            if (type == typeof(string))
                return SerializedPropertyType.String;

            if (type == typeof(LayerMask))
                return SerializedPropertyType.LayerMask;

            // Vectors.

            if (type == typeof(Vector2))
                return SerializedPropertyType.Vector2;
            if (type == typeof(Vector3))
                return SerializedPropertyType.Vector3;
            if (type == typeof(Vector4))
                return SerializedPropertyType.Vector4;

            if (type == typeof(Quaternion))
                return SerializedPropertyType.Quaternion;

            // Other.

            if (type == typeof(Color) || type == typeof(Color32))
                return SerializedPropertyType.Color;
            if (type == typeof(Gradient))
                return SerializedPropertyType.Gradient;

            if (type == typeof(Rect))
                return SerializedPropertyType.Rect;
            if (type == typeof(Bounds))
                return SerializedPropertyType.Bounds;

            if (type == typeof(AnimationCurve))
                return SerializedPropertyType.AnimationCurve;

            // Int Variants.

            if (type == typeof(Vector2Int))
                return SerializedPropertyType.Vector2Int;
            if (type == typeof(Vector3Int))
                return SerializedPropertyType.Vector3Int;
            if (type == typeof(RectInt))
                return SerializedPropertyType.RectInt;
            if (type == typeof(BoundsInt))
                return SerializedPropertyType.BoundsInt;

            // Special.

            if (typeof(Object).IsAssignableFrom(type))
                return SerializedPropertyType.ObjectReference;

            if (type.IsEnum)
                return SerializedPropertyType.Enum;

            return SerializedPropertyType.Generic;
        }

         

        public static void RemoveArrayElement(SerializedProperty property, int index)
        {
            var count = property.arraySize;
            property.DeleteArrayElementAtIndex(index);
            if (property.arraySize == count)
                property.DeleteArrayElementAtIndex(index);
        }

         
        #endregion
         
        #region Accessor Pool
         

        private static readonly Dictionary<Type, Dictionary<string, PropertyAccessor>>
            TypeToPathToAccessor = new Dictionary<Type, Dictionary<string, PropertyAccessor>>();

         

        public static PropertyAccessor GetAccessor(this SerializedProperty property)
        {
            var type = property.serializedObject.targetObject.GetType();
            return GetAccessor(property, property.propertyPath, ref type);
        }

         

        private static PropertyAccessor GetAccessor(SerializedProperty property, string propertyPath, ref Type type)
        {
            if (!TypeToPathToAccessor.TryGetValue(type, out var pathToAccessor))
            {
                pathToAccessor = new Dictionary<string, PropertyAccessor>();
                TypeToPathToAccessor.Add(type, pathToAccessor);
            }

            if (!pathToAccessor.TryGetValue(propertyPath, out var accessor))
            {
                var nameStartIndex = propertyPath.LastIndexOf('.');
                string elementName;
                PropertyAccessor parent;

                // Array.
                if (nameStartIndex > 6 &&
                    nameStartIndex < propertyPath.Length - 7 &&
                    string.Compare(propertyPath, nameStartIndex - 6, ArrayDataPrefix, 0, 12) == 0)
                {
                    var index = int.Parse(propertyPath.Substring(nameStartIndex + 6, propertyPath.Length - nameStartIndex - 7));

                    var nameEndIndex = nameStartIndex - 6;
                    nameStartIndex = propertyPath.LastIndexOf('.', nameEndIndex - 1);

                    elementName = propertyPath.Substring(nameStartIndex + 1, nameEndIndex - nameStartIndex - 1);

                    FieldInfo field;
                    if (nameStartIndex >= 0)
                    {
                        parent = GetAccessor(property, propertyPath.Substring(0, nameStartIndex), ref type);
                        field = GetField(parent, property, type, elementName);
                    }
                    else
                    {
                        parent = null;
                        field = GetField(type, elementName);
                    }

                    accessor = new CollectionPropertyAccessor(parent, elementName, field, index);
                }
                else// Single.
                {
                    if (nameStartIndex >= 0)
                    {
                        elementName = propertyPath.Substring(nameStartIndex + 1);
                        parent = GetAccessor(property, propertyPath.Substring(0, nameStartIndex), ref type);
                    }
                    else
                    {
                        elementName = propertyPath;
                        parent = null;
                    }

                    var field = GetField(parent, property, type, elementName);

                    accessor = new PropertyAccessor(parent, elementName, field);
                }

                pathToAccessor.Add(propertyPath, accessor);
            }

            if (accessor != null)
            {
                var field = accessor.GetField(property);
                if (field != null)
                {
                    type = field.FieldType;
                }
                else
                {
                    var value = accessor.GetValue(property);
                    type = value?.GetType();
                }
            }

            return accessor;
        }

         

        public static FieldInfo GetField(PropertyAccessor accessor, SerializedProperty property, Type declaringType, string name)
        {
            declaringType = accessor?.GetFieldElementType(property) ?? declaringType;
            return GetField(declaringType, name);
        }

        public static FieldInfo GetField(Type declaringType, string name)
        {
            while (declaringType != null)
            {
                var field = declaringType.GetField(name, InstanceBindings);
                if (field != null)
                    return field;

                declaringType = declaringType.BaseType;
            }

            return null;
        }

         
        #endregion
         
        #region PropertyAccessor
         

        public class PropertyAccessor
        {
             

            public readonly PropertyAccessor Parent;

            public readonly string Name;

            protected readonly FieldInfo Field;

            protected readonly Type FieldElementType;

             

            internal PropertyAccessor(PropertyAccessor parent, string name, FieldInfo field)
                : this(parent, name, field, field?.FieldType)
            { }

            protected PropertyAccessor(PropertyAccessor parent, string name, FieldInfo field, Type fieldElementType)
            {
                Parent = parent;
                Name = name;
                Field = field;
                FieldElementType = fieldElementType;
            }

             

            public FieldInfo GetField(ref object obj)
            {
                if (Parent != null)
                    obj = Parent.GetValue(obj);

                if (Field != null)
                    return Field;

                if (obj is null)
                    return null;

                return Serialization.GetField(obj.GetType(), Name);
            }

            public FieldInfo GetField(object obj) => Field ?? GetField(ref obj);

            public FieldInfo GetField(SerializedObject serializedObject)
                => serializedObject != null ? GetField(serializedObject.targetObject) : null;

            public FieldInfo GetField(SerializedProperty serializedProperty)
                => serializedProperty != null ? GetField(serializedProperty.serializedObject) : null;

             

            public virtual Type GetFieldElementType(object obj) => FieldElementType ?? GetField(ref obj)?.FieldType;

            public Type GetFieldElementType(SerializedObject serializedObject)
                => serializedObject != null ? GetFieldElementType(serializedObject.targetObject) : null;

            public Type GetFieldElementType(SerializedProperty serializedProperty)
                => serializedProperty != null ? GetFieldElementType(serializedProperty.serializedObject) : null;

             

            public virtual object GetValue(object obj)
                => GetField(ref obj)?.GetValue(obj);

            public object GetValue(SerializedObject serializedObject)
                => serializedObject != null ? GetValue(serializedObject.targetObject) : null;

            public object GetValue(SerializedProperty serializedProperty)
                => serializedProperty != null ? GetValue(serializedProperty.serializedObject) : null;

             

            public virtual void SetValue(object obj, object value)
            {
                var field = GetField(ref obj);

                if (obj is null)
                    return;

                field.SetValue(obj, value);
            }

            public void SetValue(SerializedObject serializedObject, object value)
            {
                if (serializedObject != null)
                    SetValue(serializedObject.targetObject, value);
            }

            public void SetValue(SerializedProperty serializedProperty, object value)
            {
                if (serializedProperty != null)
                    SetValue(serializedProperty.serializedObject, value);
            }

             

            public void ResetValue(SerializedProperty property, string undoName = "Inspector")
            {
                property.RecordUndo(undoName);
                property.serializedObject.ApplyModifiedProperties();

                var type = GetValue(property)?.GetType();
                var value = type != null ? Activator.CreateInstance(type) : null;
                SetValue(property, value);

                property.serializedObject.Update();
            }

             

            public override string ToString()
            {
                if (Parent != null)
                    return $"{Parent}.{Name}";
                else
                    return Name;
            }

             

            public virtual string GetPath()
            {
                if (Parent != null)
                    return $"{Parent.GetPath()}.{Name}";
                else
                    return Name;
            }

             
        }

         
        #endregion
         
        #region CollectionPropertyAccessor
         

        public class CollectionPropertyAccessor : PropertyAccessor
        {
             

            public readonly int ElementIndex;

             

            internal CollectionPropertyAccessor(PropertyAccessor parent, string name, FieldInfo field, int elementIndex)
                : base(parent, name, field, GetElementType(field?.FieldType))
            {
                ElementIndex = elementIndex;
            }

             

            public override Type GetFieldElementType(object obj) => FieldElementType ?? GetElementType(GetField(ref obj)?.FieldType);

             

            public static Type GetElementType(Type fieldType)
            {
                if (fieldType == null)
                    return null;

                if (fieldType.IsArray)
                    return fieldType.GetElementType();

                if (fieldType.IsGenericType)
                    return fieldType.GetGenericArguments()[0];

                Debug.LogWarning($"{nameof(Serialization)}.{nameof(CollectionPropertyAccessor)}:" +
                    $" unable to determine element type for {fieldType}");
                return fieldType;
            }

             

            public object GetCollection(object obj) => base.GetValue(obj);

            public override object GetValue(object obj)
            {
                var collection = base.GetValue(obj);
                if (collection == null)
                    return null;

                var list = collection as IList;
                if (list != null)
                {
                    if (ElementIndex < list.Count)
                        return list[ElementIndex];
                    else
                        return null;
                }

                var enumerator = ((IEnumerable)collection).GetEnumerator();

                for (int i = 0; i < ElementIndex; i++)
                {
                    if (!enumerator.MoveNext())
                        return null;
                }

                return enumerator.Current;
            }

             

            public void SetCollection(object obj, object value) => base.SetValue(obj, value);

            public override void SetValue(object obj, object value)
            {
                var collection = base.GetValue(obj);
                if (collection == null)
                    return;

                var list = collection as IList;
                if (list != null)
                {
                    if (ElementIndex < list.Count)
                        list[ElementIndex] = value;

                    return;
                }

                throw new InvalidOperationException($"{nameof(SetValue)} failed: field doesn't implement {nameof(IList)}.");
            }

             

            public override string ToString() => $"{base.ToString()}[{ElementIndex}]";

             

            public string GetCollectionPath() => base.GetPath();

            public override string GetPath() => $"{base.GetPath()}{ArrayDataPrefix}{ElementIndex}{ArrayDataSuffix}";

             
        }

         
        #endregion
         
    }
}

#endif
