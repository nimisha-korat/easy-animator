


#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Shared File Last Modified: 2021-07-24.
namespace EasyAnimator.Editor
// namespace InspectorGadgets.Editor
{
    public partial class Serialization
    {
        [Serializable]
        public sealed class PropertyReference
        {
             

            [SerializeField] private ObjectReference[] _TargetObjects;

            public ObjectReference TargetObject
            {
                get
                {
                    return _TargetObjects != null && _TargetObjects.Length > 0 ?
                        _TargetObjects[0] : null;
                }
            }

            public ObjectReference[] TargetObjects => _TargetObjects;

             

            [SerializeField] private ObjectReference _Context;

            public ObjectReference Context => _Context;

             

            [SerializeField] private string _PropertyPath;

            public string PropertyPath => _PropertyPath;

             

            [NonSerialized] private bool _IsInitialized;

            public bool IsInitialized => _IsInitialized;

             

            [NonSerialized] private SerializedProperty _Property;

            public SerializedProperty Property
            {
                get
                {
                    Initialize();
                    return _Property;
                }
            }

             

            public PropertyReference(SerializedProperty property)
            {
                _TargetObjects = ObjectReference.Convert(property.serializedObject.targetObjects);

                _Context = property.serializedObject.context;
                _PropertyPath = property.propertyPath;

                // Don't set the _Property. If it gets accessed we want to create out own instance.
            }

             

            public static implicit operator PropertyReference(SerializedProperty property) => new PropertyReference(property);

            public static implicit operator SerializedProperty(PropertyReference reference) => reference.Property;

             

            private void Initialize()
            {
                if (_IsInitialized)
                {
                    if (!TargetsExist)
                        Dispose();
                    return;
                }

                _IsInitialized = true;

                if (string.IsNullOrEmpty(_PropertyPath) ||
                    !TargetsExist)
                    return;

                var targetObjects = ObjectReference.Convert(_TargetObjects);
                var serializedObject = new SerializedObject(targetObjects, _Context);
                _Property = serializedObject.FindProperty(_PropertyPath);
            }

             

            public bool IsTarget(SerializedProperty property, Object[] targetObjects)
            {
                if (_Property == null ||
                    _Property.propertyPath != property.propertyPath ||
                    _TargetObjects == null ||
                    _TargetObjects.Length != targetObjects.Length)
                    return false;

                for (int i = 0; i < _TargetObjects.Length; i++)
                {
                    if (_TargetObjects[i] != targetObjects[i])
                        return false;
                }

                return true;
            }

             

            private bool TargetsExist
            {
                get
                {
                    if (_TargetObjects == null ||
                        _TargetObjects.Length == 0)
                        return false;

                    for (int i = 0; i < _TargetObjects.Length; i++)
                    {
                        if (_TargetObjects[i].Object == null)
                            return false;
                    }

                    return true;
                }
            }

             

            public void Update()
            {
                if (_Property == null)
                    return;

                if (!TargetsExist)
                {
                    Dispose();
                    return;
                }

                _Property.serializedObject.Update();
            }

            public void ApplyModifiedProperties()
            {
                if (_Property == null)
                    return;

                if (!TargetsExist)
                {
                    Dispose();
                    return;
                }

                _Property.serializedObject.ApplyModifiedProperties();
            }

            public void Dispose()
            {
                if (_Property != null)
                {
                    _Property.serializedObject.Dispose();
                    _Property = null;
                }
            }

             

            public float GetPropertyHeight()
            {
                if (_Property == null)
                    return 0;

                return EditorGUI.GetPropertyHeight(_Property, _Property.isExpanded);
            }

             

            public void DoTargetGUI(Rect area)
            {
                area.height = EditorGUIUtility.singleLineHeight;

                Initialize();

                if (_Property == null)
                {
                    GUI.Label(area, "Missing " + this);
                    return;
                }

                var targets = _Property.serializedObject.targetObjects;

                using (new EditorGUI.DisabledScope(true))
                {
                    var showMixedValue = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = targets.Length > 1;

                    var target = targets.Length > 0 ? targets[0] : null;
                    EditorGUI.ObjectField(area, target, typeof(Object), true);

                    EditorGUI.showMixedValue = showMixedValue;
                }
            }

             

            public void DoPropertyGUI(Rect area)
            {
                Initialize();

                if (_Property == null)
                    return;

                _Property.serializedObject.Update();

                GUI.BeginGroup(area);
                area.x = area.y = 0;

                EditorGUI.PropertyField(area, _Property, _Property.isExpanded);

                GUI.EndGroup();

                _Property.serializedObject.ApplyModifiedProperties();
            }

             
        }

         

        public static bool IsValid(this PropertyReference reference) => reference?.Property != null;

         
    }
}

#endif
