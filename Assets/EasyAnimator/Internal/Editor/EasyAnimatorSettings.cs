

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using EasyAnimator.Units;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyAnimator.Editor
{
  
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(EasyAnimatorSettings))]
    public sealed class EasyAnimatorSettings : ScriptableObject
    {
         

        private static EasyAnimatorSettings _Instance;

     
        public static EasyAnimatorSettings Instance
        {
            get
            {
                if (_Instance != null)
                    return _Instance;

                _Instance = EasyAnimatorEditorUtilities.FindAssetOfType<EasyAnimatorSettings>();

                if (_Instance != null)
                    return _Instance;

                _Instance = CreateInstance<EasyAnimatorSettings>();
                _Instance.name = "EasyAnimator Settings";
                _Instance.hideFlags = HideFlags.DontSaveInBuild;

                var script = MonoScript.FromScriptableObject(_Instance);
                var path = AssetDatabase.GetAssetPath(script);
                path = Path.Combine(Path.GetDirectoryName(path), $"{_Instance.name}.asset");
                AssetDatabase.CreateAsset(_Instance, path);

                return _Instance;
            }
        }

         

        private SerializedObject _SerializedObject;

        public static SerializedObject SerializedObject
            => Instance._SerializedObject ?? (Instance._SerializedObject = new SerializedObject(Instance));

         

        private readonly Dictionary<string, SerializedProperty>
            SerializedProperties = new Dictionary<string, SerializedProperty>();

        private static SerializedProperty GetSerializedProperty(string propertyPath)
        {
            var properties = Instance.SerializedProperties;
            if (!properties.TryGetValue(propertyPath, out var property))
            {
                property = SerializedObject.FindProperty(propertyPath);
                properties.Add(propertyPath, property);
            }

            return property;
        }

         

        public abstract class Group
        {
             

            private string _BasePropertyPath;

            internal void SetBasePropertyPath(string propertyPath)
            {
                _BasePropertyPath = propertyPath + ".";
            }

             

            protected SerializedProperty GetSerializedProperty(string propertyPath)
                => EasyAnimatorSettings.GetSerializedProperty(_BasePropertyPath + propertyPath);

             

            protected SerializedProperty DoPropertyField(string propertyPath)
            {
                var property = GetSerializedProperty(propertyPath);
                EditorGUILayout.PropertyField(property, true);
                return property;
            }

             
        }

         

        private void OnEnable()
        {
            if (_TransitionPreviewWindow == null)
                _TransitionPreviewWindow = new TransitionPreviewWindow.Settings();
            _TransitionPreviewWindow.SetBasePropertyPath(nameof(_TransitionPreviewWindow));
        }

         

        public static new void SetDirty() => EditorUtility.SetDirty(_Instance);

         

        [SerializeField]
        private TransitionPreviewWindow.Settings _TransitionPreviewWindow;

        internal static TransitionPreviewWindow.Settings TransitionPreviewWindow => Instance._TransitionPreviewWindow;

         

        [SerializeField]
        private AnimationTimeAttribute.Settings _AnimationTimeFields;

        public static AnimationTimeAttribute.Settings AnimationTimeFields => Instance._AnimationTimeFields;

         

        [SerializeField, Range(0.01f, 1)]
        [Tooltip("The amount of time between repaint commands when 'Display Options/Repaint Constantly' is disabled")]
        private float _InspectorRepaintInterval = 0.25f;

        public static float InspectorRepaintInterval => Instance._InspectorRepaintInterval;

         

        [SerializeField]
        [Tooltip("The frame rate to use for new animations")]
        private float _NewAnimationFrameRate = 12;

        public static SerializedProperty NewAnimationFrameRate => GetSerializedProperty(nameof(_NewAnimationFrameRate));

         

        [CustomEditor(typeof(EasyAnimatorSettings), true), CanEditMultipleObjects]
        public sealed class Editor : UnityEditor.Editor
        {
             

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.BeginHorizontal();

                using (ObjectPool.Disposable.AcquireContent(out var label, "Disabled Warnings"))
                {
                    EditorGUI.BeginChangeCheck();
                    var value = EditorGUILayout.EnumFlagsField(label, Validate.PermanentlyDisabledWarnings);
                    if (EditorGUI.EndChangeCheck())
                        Validate.PermanentlyDisabledWarnings = (OptionalWarning)value;
                }

                if (GUILayout.Button("Help", EditorStyles.miniButton, EasyAnimatorGUI.DontExpandWidth))
                    Application.OpenURL(Strings.DocsURLs.OptionalWarning);

                EditorGUILayout.EndHorizontal();
            }

             
        }

         
    }
}

#endif
