

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EasyAnimator.Editor
{
 

    partial class TransitionPreviewWindow
    {
         

        public static Scene InstanceScene => _Instance != null ? _Instance._Scene : null;

         

        [Serializable]
        public sealed class Scene
        {
             
            #region Fields and Properties
             

            private const HideFlags HideAndDontSave = HideFlags.HideInHierarchy | HideFlags.DontSave;

            [SerializeField]
            private UnityEngine.SceneManagement.Scene _Scene;

            public Transform PreviewSceneRoot { get; private set; }

            public Transform InstanceRoot { get; private set; }

             

            [SerializeField]
            private Transform _OriginalRoot;

            public Transform OriginalRoot
            {
                get => _OriginalRoot;
                set
                {
                    _OriginalRoot = value;
                    InstantiateModel();

                    if (value != null)
                        Settings.AddModel(value.gameObject);
                }
            }

             

            public Animator[] InstanceAnimators { get; private set; }

            [SerializeField] private int _SelectedInstanceAnimator;
            [NonSerialized] private AnimationType _SelectedInstanceType;

            public Animator SelectedInstanceAnimator
            {
                get
                {
                    if (InstanceAnimators == null ||
                        InstanceAnimators.Length == 0)
                        return null;

                    if (_SelectedInstanceAnimator > InstanceAnimators.Length)
                        _SelectedInstanceAnimator = InstanceAnimators.Length;

                    return InstanceAnimators[_SelectedInstanceAnimator];
                }
            }

             

            [NonSerialized]
            private EasyAnimatorPlayable _EasyAnimator;

            public EasyAnimatorPlayable EasyAnimator
            {
                get
                {
                    if ((_EasyAnimator == null || !_EasyAnimator.IsValid) &&
                        InstanceRoot != null)
                    {
                        var animator = SelectedInstanceAnimator;
                        if (animator != null)
                        {
                            EasyAnimatorPlayable.SetNextGraphName($"{animator.name} (EasyAnimator Preview)");
                            _EasyAnimator = EasyAnimatorPlayable.Create();
                            _EasyAnimator.SetOutput(
                                new EasyAnimatorEditorUtilities.DummyEasyAnimatorComponent(animator, _EasyAnimator));
                            _EasyAnimator.RequirePostUpdate(Animations.WindowMatchStateTime.Instance);
                            _Instance._Animations.NormalizedTime = _Instance._Animations.NormalizedTime;
                        }
                    }

                    return _EasyAnimator;
                }
            }

             
            #endregion
             
            #region Initialisation
             

            public void OnEnable()
            {
                EditorSceneManager.sceneOpening += OnSceneOpening;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;

                duringSceneGui += DoCustomGUI;

                CreateScene();
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();
            }

             

            private void CreateScene()
            {
                _Scene = EditorSceneManager.NewPreviewScene();
                _Scene.name = "Transition Preview";
                _Instance.customScene = _Scene;

                PreviewSceneRoot = EditorUtility.CreateGameObjectWithHideFlags(
                    $"{nameof(EasyAnimator)}.{nameof(TransitionPreviewWindow)}", HideAndDontSave).transform;
                SceneManager.MoveGameObjectToScene(PreviewSceneRoot.gameObject, _Scene);
                _Instance.customParentForDraggedObjects = PreviewSceneRoot;
            }

             

            private void InstantiateModel()
            {
                DestroyModelInstance();

                if (_OriginalRoot == null)
                    return;

                PreviewSceneRoot.gameObject.SetActive(false);
                InstanceRoot = Instantiate(_OriginalRoot, PreviewSceneRoot);
                InstanceRoot.localPosition = default;
                InstanceRoot.name = _OriginalRoot.name;

                DisableUnnecessaryComponents(InstanceRoot.gameObject);

                InstanceAnimators = InstanceRoot.GetComponentsInChildren<Animator>();
                for (int i = 0; i < InstanceAnimators.Length; i++)
                {
                    var animator = InstanceAnimators[i];
                    animator.enabled = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.fireEvents = false;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                }

                PreviewSceneRoot.gameObject.SetActive(true);

                SetSelectedAnimator(_SelectedInstanceAnimator);
                FocusCamera();
                _Instance._Animations.GatherAnimations();
            }

             

            private static void DisableUnnecessaryComponents(GameObject root)
            {
                var behaviours = root.GetComponentsInChildren<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];

                    // Other undesirable components aren't Behaviours anyway: Transform, MeshFilter, Renderer
                    if (behaviour is Animator)
                        continue;

                    behaviour.enabled = false;
                    behaviour.hideFlags |= HideFlags.NotEditable;
                    if (behaviour is MonoBehaviour mono)
                        mono.runInEditMode = false;
                }
            }

             

            public void SetSelectedAnimator(int index)
            {
                DestroyEasyAnimatorInstance();

                var animator = SelectedInstanceAnimator;
                if (animator != null && animator.enabled)
                {
                    animator.Rebind();
                    animator.enabled = false;
                    return;
                }

                _SelectedInstanceAnimator = index;

                animator = SelectedInstanceAnimator;
                if (animator != null)
                {
                    animator.enabled = true;
                    _SelectedInstanceType = AnimationBindings.GetAnimationType(animator);
                    _Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
                }
            }

             

            public void OnTargetPropertyChanged()
            {
                _SelectedInstanceAnimator = 0;
                if (_ExpandedHierarchy != null)
                    _ExpandedHierarchy.Clear();

                OriginalRoot = EasyAnimatorEditorUtilities.FindRoot(_Instance._TransitionProperty.TargetObject);
                if (OriginalRoot == null)
                    OriginalRoot = Settings.TrySelectBestModel();

                _Instance._Animations.NormalizedTime = 0;

                _Instance.in2DMode = _SelectedInstanceType == AnimationType.Sprite;
            }

             

            private void FocusCamera()
            {
                var bounds = CalculateBounds(InstanceRoot);

                var rotation = _Instance.in2DMode ?
                    Quaternion.identity :
                    Quaternion.Euler(35, 135, 0);

                var size = bounds.extents.magnitude * 1.5f;
                if (size == float.PositiveInfinity)
                    return;
                else if (size == 0)
                    size = 10;

                _Instance.LookAt(bounds.center, rotation, size, _Instance.in2DMode, true);
            }

             

            private static Bounds CalculateBounds(Transform transform)
            {
                var renderers = transform.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return default;

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

             
            #endregion
             
            #region Execution
             

            public void OnGUI()
            {
                if (!EasyAnimatorEditorUtilities.IsChangingPlayMode && InstanceRoot == null)
                    InstantiateModel();

                if (_EasyAnimator != null && _EasyAnimator.IsGraphPlaying)
                    EasyAnimatorGUI.RepaintEverything();

                if (Selection.activeObject == _Instance &&
                    Event.current.type == EventType.KeyUp &&
                    Event.current.keyCode == KeyCode.F)
                    FocusCamera();
            }

             

            private void OnPlayModeChanged(PlayModeStateChange change)
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    case PlayModeStateChange.ExitingPlayMode:
                        DestroyModelInstance();
                        break;
                }
            }

             

            private void OnSceneOpening(string path, OpenSceneMode mode)
            {
                if (mode == OpenSceneMode.Single)
                    DestroyModelInstance();
            }

             

            private void DoCustomGUI(SceneView sceneView)
            {
                var Animator = EasyAnimator;
                if (Animator != null &&
                    sceneView is TransitionPreviewWindow instance &&
                    EasyAnimatorUtilities.TryGetWrappedObject(Transition, out ITransitionGUI gui) &&
                    instance._TransitionProperty != null)
                {
                    EditorGUI.BeginChangeCheck();

                    using (TransitionDrawer.DrawerContext.Get(instance._TransitionProperty))
                    {
                        try
                        {
                            gui.OnPreviewSceneGUI(new TransitionPreviewDetails(Animator));
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                        EasyAnimatorGUI.RepaintEverything();
                }
            }

             

            public bool IsSceneObject(Object obj)
            {
                return
                    obj is GameObject gameObject &&
                    gameObject.transform.IsChildOf(PreviewSceneRoot);
            }

             

            [SerializeField]
            private List<Transform> _ExpandedHierarchy;

            public List<Transform> ExpandedHierarchy
            {
                get
                {
                    if (_ExpandedHierarchy == null)
                        _ExpandedHierarchy = new List<Transform>();
                    return _ExpandedHierarchy;
                }
            }

             
            #endregion
             
            #region Cleanup
             

            public void OnDisable()
            {
                EditorSceneManager.sceneOpening -= OnSceneOpening;
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;

                duringSceneGui -= DoCustomGUI;

                DestroyEasyAnimatorInstance();

                EditorSceneManager.ClosePreviewScene(_Scene);
            }

             

            public void OnDestroy()
            {
                if (PreviewSceneRoot != null)
                {
                    DestroyImmediate(PreviewSceneRoot.gameObject);
                    PreviewSceneRoot = null;
                }
            }

             

            public void DestroyModelInstance()
            {
                DestroyEasyAnimatorInstance();

                if (InstanceRoot == null)
                    return;

                DestroyImmediate(InstanceRoot.gameObject);
                InstanceRoot = null;
                InstanceAnimators = null;
            }

             

            private void DestroyEasyAnimatorInstance()
            {
                if (_EasyAnimator == null)
                    return;

                _EasyAnimator.CancelPostUpdate(Animations.WindowMatchStateTime.Instance);
                _EasyAnimator.Destroy();
                _EasyAnimator = null;
            }

             
            #endregion
             
        }
    }
}

#endif

