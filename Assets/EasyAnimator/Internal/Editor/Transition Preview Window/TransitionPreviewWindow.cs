


#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
   

    [HelpURL(Strings.DocsURLs.TransitionPreviews)]
#if UNITY_2020_1_OR_NEWER
    [EditorWindowTitle]// Prevent the base SceneView from trying to use this type name to find the icon.
#endif
    public sealed partial class TransitionPreviewWindow : SceneView
    {
         
        #region Public API
         

        private static Texture _Icon;

        public static Texture Icon
        {
            get
            {
                if (_Icon == null)
                {
                    // Possible icons: "UnityEditor.LookDevView", "SoftlockInline", "ViewToolOrbit", "ClothInspector.ViewValue".
                    var name = EditorGUIUtility.isProSkin ? "ViewToolOrbit On" : "ViewToolOrbit";

                    _Icon = EasyAnimatorGUI.LoadIcon(name);
                    if (_Icon == null)
                        _Icon = EditorGUIUtility.whiteTexture;
                }

                return _Icon;
            }
        }

         

        public static void OpenOrClose(SerializedProperty transitionProperty)
        {
            transitionProperty = transitionProperty.Copy();

            EditorApplication.delayCall += () =>
            {
                if (!IsPreviewing(transitionProperty))
                {
                    GetWindow<TransitionPreviewWindow>(typeof(SceneView))
                        .SetTargetProperty(transitionProperty);
                }
                else
                {
                    _Instance.Close();
                }
            };
        }

         

        public static float PreviewNormalizedTime
        {
            get => _Instance._Animations.NormalizedTime;
            set
            {
                if (value.IsFinite() &&
                    IsPreviewingCurrentProperty())
                    _Instance._Animations.NormalizedTime = value;
            }
        }

         

        public static EasyAnimatorState GetCurrentState()
        {
            if (!IsPreviewingCurrentProperty() ||
                _Instance._Scene.EasyAnimator == null)
                return null;

            _Instance._Scene.EasyAnimator.States.TryGet(Transition, out var state);
            return state;
        }

         

        public static bool IsPreviewingCurrentProperty()
        {
            return
                TransitionDrawer.Context != null &&
                IsPreviewing(TransitionDrawer.Context.Property);
        }

        public static bool IsPreviewing(SerializedProperty property)
        {
            return
                _Instance != null &&
                _Instance._TransitionProperty.IsValid() &&
                Serialization.AreSameProperty(property, _Instance._TransitionProperty);
        }

         
        #endregion
         
        #region Messages
         

        private static TransitionPreviewWindow _Instance;

        [SerializeField] private Object[] _PreviousSelection;
        [SerializeField] private Animations _Animations;
        [SerializeField] private Scene _Scene;

         

        public override void OnEnable()
        {
            _Instance = this;

#if ! UNITY_2020_1_OR_NEWER
            // Unity 2019 logs an error message when opening this window.
            // This is because the base SceneView has a [EditorWindowTitle] attribute which looks for the icon by
            // name, but since it's internal before Unity 2020 we can't replace it to prevent it from doing so.
            // Error: Unable to load the icon: 'EasyAnimator.Editor.TransitionPreviewWindow'.
            using (BlockAllLogs.Activate())
#endif
            {
                base.OnEnable();
            }

            name = "Transition Preview Window";
            titleContent = new GUIContent("Transition Preview", Icon);
            autoRepaintOnSceneChange = true;
            sceneViewState.showSkybox = false;
            sceneLighting = false;

            if (_Scene == null)
                _Scene = new Scene();
            if (_Animations == null)
                _Animations = new Animations();

            if (_TransitionProperty.IsValid() &&
                !CanBePreviewed(_TransitionProperty))
            {
                DestroyTransitionProperty();
            }

            _Scene.OnEnable();

            Selection.selectionChanged += OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload += DeselectPreviewSceneObjects;

            // Re-select next frame.
            // This fixes an issue where the Inspector header displays differently after a domain reload.
            if (Selection.activeObject == this)
            {
                Selection.activeObject = null;
                EditorApplication.delayCall += () => Selection.activeObject = this;
            }
        }

         

        public override void OnDisable()
        {
            base.OnDisable();
            _Scene.OnDisable();
            _Instance = null;
            Selection.selectionChanged -= OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= DeselectPreviewSceneObjects;
        }

         

        private new void OnDestroy()
        {
            base.OnDestroy();
            _Scene.OnDestroy();
            DestroyTransitionProperty();

            using (ObjectPool.Disposable.AcquireList<Object>(out var objects))
            {
                for (int i = 0; i < _PreviousSelection.Length; i++)
                {
                    var obj = _PreviousSelection[i];
                    if (obj != null)
                        objects.Add(obj);
                }
                Selection.objects = objects.ToArray();
            }

            _TransitionProperty = null;

            EasyAnimatorGUI.RepaintEverything();
        }

         

        protected override void OnGUI()
        {
            _Instance = this;

            var activeObject = Selection.activeObject;

            base.OnGUI();

            // Don't allow clicks in this window to select objects in the preview scene.
            if (activeObject != Selection.activeObject)
                DeselectPreviewSceneObjects();

            _Scene.OnGUI();
        }

         

        private void Update()
        {
            if (Selection.activeObject == null)
                Selection.activeObject = this;

            if (Settings.AutoClose && !_TransitionProperty.IsValid())
            {
                Close();
                return;
            }
        }

         

        protected override bool SupportsStageHandling() => false;

         

        private void OnSelectionChanged()
        {
            if (Selection.activeObject == null)
                EditorApplication.delayCall += () => Selection.activeObject = this;
        }

         

        private void DeselectPreviewSceneObjects()
        {
            using (ObjectPool.Disposable.AcquireList<Object>(out var objects))
            {
                var selection = Selection.objects;
                for (int i = 0; i < selection.Length; i++)
                {
                    var obj = selection[i];
                    if (!_Scene.IsSceneObject(obj))
                        objects.Add(obj);
                }
                Selection.objects = objects.ToArray();
            }
        }

         
        #endregion
         
        #region Transition Property
         

        [SerializeField]
        private Serialization.PropertyReference _TransitionProperty;

        public static SerializedProperty TransitionProperty => _Instance._TransitionProperty;

         

        public static ITransitionDetailed Transition
        {
            get
            {
                var property = _Instance._TransitionProperty;
                if (!property.IsValid())
                    return null;

                return property.Property.GetValue<ITransitionDetailed>();
            }
        }

         

        public static bool CanBePreviewed(SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            if (accessor == null)
                return false;

            var type = accessor.GetFieldElementType(property);
            if (typeof(ITransitionDetailed).IsAssignableFrom(type))
                return true;

            var value = accessor.GetValue(property);
            return
                value != null &&
                typeof(ITransitionDetailed).IsAssignableFrom(value.GetType());
        }

         

        private void SetTargetProperty(SerializedProperty property)
        {
            if (property.serializedObject.targetObjects.Length != 1)
            {
                Close();
                throw new ArgumentException($"{nameof(TransitionPreviewWindow)} does not support multi-object selection.");
            }

            if (!CanBePreviewed(property))
            {
                Close();
                throw new ArgumentException($"The specified property does not implement {nameof(ITransitionDetailed)}.");
            }

            if (!_TransitionProperty.IsValid())
                _PreviousSelection = Selection.objects;
            Selection.activeObject = this;

            DestroyTransitionProperty();

            _TransitionProperty = property;
            _Scene.OnTargetPropertyChanged();
        }

         

        private void DestroyTransitionProperty()
        {
            if (_TransitionProperty == null)
                return;

            _Scene.DestroyModelInstance();

            _TransitionProperty.Dispose();
            _TransitionProperty = null;
        }

         
        #endregion
         
        #region Error Intercepts
#if ! UNITY_2020_1_OR_NEWER
         

        private sealed class BlockAllLogs : IDisposable, ILogHandler
        {
            private static readonly BlockAllLogs Instance = new BlockAllLogs();

            private ILogHandler _PreviousHandler;

            public static IDisposable Activate()
            {
                EasyAnimatorUtilities.Assert(Instance._PreviousHandler == null,
                    $"{nameof(BlockAllLogs)} can't be used recursively.");

                Instance._PreviousHandler = Debug.unityLogger.logHandler;
                Debug.unityLogger.logHandler = Instance;
                return Instance;
            }

            void IDisposable.Dispose()
            {
                Debug.unityLogger.logHandler = _PreviousHandler;
                _PreviousHandler = null;
            }

            void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args) { }

            void ILogHandler.LogException(Exception exception, Object context) { }
        }

         
#endif
        #endregion
         
    }
}

#endif

