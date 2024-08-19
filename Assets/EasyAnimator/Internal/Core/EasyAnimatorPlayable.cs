

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
   
    public sealed partial class EasyAnimatorPlayable : PlayableBehaviour,
        IEnumerator, IPlayableWrapper, IAnimationClipCollection
    {
         
        #region Fields and Properties
         

        private static float _DefaultFadeDuration = 0.25f;

         

#if UNITY_EDITOR
        public const string DefaultFadeDurationNamespace = nameof(EasyAnimator);

        public const string DefaultFadeDurationClass = nameof(DefaultFadeDuration);

        static EasyAnimatorPlayable()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Iterate backwards since it's more likely to be towards the end.
            for (int iAssembly = assemblies.Length - 1; iAssembly >= 0; iAssembly--)
            {
                var type = assemblies[iAssembly].GetType(DefaultFadeDurationNamespace + "." + DefaultFadeDurationClass);
                if (type != null)
                {
                    var methods = type.GetMethods(Editor.EasyAnimatorEditorUtilities.StaticBindings);
                    for (int iMethod = 0; iMethod < methods.Length; iMethod++)
                    {
                        var method = methods[iMethod];
                        if (method.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), false))
                        {
                            method.Invoke(null, null);
                            return;
                        }
                    }
                }
            }
        }
#endif

         

      
        public static float DefaultFadeDuration
        {
            get => _DefaultFadeDuration;
            set
            {
                EasyAnimatorUtilities.Assert(value >= 0 && value < float.PositiveInfinity,
                    $"{nameof(EasyAnimatorPlayable)}.{nameof(DefaultFadeDuration)} must not be negative or infinity.");

                _DefaultFadeDuration = value;
            }
        }

         

        internal PlayableGraph _Graph;
        public PlayableGraph Graph => _Graph;

        internal Playable _RootPlayable;

        internal Playable _LayerMixer;

         

        Playable IPlayableWrapper.Playable => _LayerMixer;

        IPlayableWrapper IPlayableWrapper.Parent => null;

        float IPlayableWrapper.Weight => 1;

        int IPlayableWrapper.ChildCount => Layers.Count;

        EasyAnimatorNode IPlayableWrapper.GetChild(int index) => Layers[index];

      
       
        public LayerList Layers { get; private set; }

        public StateDictionary States { get; private set; }

        private Key.KeyedList<IUpdatable> _PreUpdatables;

        private Key.KeyedList<IUpdatable> _PostUpdatables;

        private PostUpdate _PostUpdate;

         

        public IEasyAnimatorComponent Component { get; private set; }

         

       
        public int CommandCount => Layers[0].CommandCount;

         

        public DirectorUpdateMode UpdateMode
        {
            get => _Graph.GetTimeUpdateMode();
            set => _Graph.SetTimeUpdateMode(value);
        }

         

        
        public float Speed
        {
            get => (float)_LayerMixer.GetSpeed();
            set => _LayerMixer.SetSpeed(value);
        }

         

        private bool _KeepChildrenConnected;

        
        public bool KeepChildrenConnected
        {
            get => _KeepChildrenConnected;
            set
            {
                if (_KeepChildrenConnected == value)
                    return;

                _KeepChildrenConnected = value;

                if (value)
                    _PostUpdate.IsConnected = true;

                Layers.SetWeightlessChildrenConnected(value);
            }
        }

         

        private bool _SkipFirstFade;

       
        public bool SkipFirstFade
        {
            get => _SkipFirstFade;
            set
            {
                _SkipFirstFade = value;

                if (!value && Layers.Count < 2)
                {
                    Layers.Count = 1;
                    _LayerMixer.SetInputCount(2);
                }
            }
        }

         
        #endregion
         
        #region Initialisation
         

       
        private static readonly EasyAnimatorPlayable Template = new EasyAnimatorPlayable();

         

      
        public static EasyAnimatorPlayable Create()
        {
#if UNITY_EDITOR
            var name = _NextGraphName;
            _NextGraphName = null;

            var graph = name != null ?
                PlayableGraph.Create(name) :
                PlayableGraph.Create();
#else
            var graph = PlayableGraph.Create();
#endif

            return ScriptPlayable<EasyAnimatorPlayable>.Create(graph, Template, 2)
                .GetBehaviour();
        }

         

        public override void OnPlayableCreate(Playable playable)
        {
            _RootPlayable = playable;
            _Graph = playable.GetGraph();

            _PostUpdatables = new Key.KeyedList<IUpdatable>();
            _PreUpdatables = new Key.KeyedList<IUpdatable>();
            _PostUpdate = PostUpdate.Create(this);
            Layers = new LayerList(this, out _LayerMixer);
            States = new StateDictionary(this);

            playable.SetInputWeight(0, 1);

#if UNITY_EDITOR
            RegisterInstance();
#endif
        }

         

#if UNITY_EDITOR
        private static string _NextGraphName;
#endif

       
        [System.Diagnostics.Conditional(Strings.UnityEditor)]
        public static void SetNextGraphName(string name)
        {
#if UNITY_EDITOR
            _NextGraphName = name;
#endif
        }

         

#if UNITY_EDITOR
        public override string ToString()
            => $"{nameof(EasyAnimatorPlayable)} ({(_Graph.IsValid() ? _Graph.GetEditorName() : "Graph Not Initialized")})";
#endif

         

       
        public void SetOutput(IEasyAnimatorComponent EasyAnimator)
            => SetOutput(EasyAnimator.Animator, EasyAnimator);

        public void SetOutput(Animator animator, IEasyAnimatorComponent EasyAnimator)
        {
#if UNITY_ASSERTIONS
            if (animator == null)
                throw new ArgumentNullException(nameof(animator),
                    $"An {nameof(Animator)} component is required to play animations.");

#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.IsPersistent(animator))
                throw new ArgumentException(
                    $"The specified {nameof(Animator)} component is a prefab which means it cannot play animations.",
                    nameof(animator));
#endif

            if (EasyAnimator != null)
            {
                Debug.Assert(EasyAnimator.IsPlayableInitialized && EasyAnimator.Playable == this,
                    $"{nameof(SetOutput)} was called on an {nameof(EasyAnimatorPlayable)} which does not match the" +
                    $" {nameof(IEasyAnimatorComponent)}.{nameof(IEasyAnimatorComponent.Playable)}.");
                Debug.Assert(animator == EasyAnimator.Animator,
                    $"{nameof(SetOutput)} was called with an {nameof(Animator)} which does not match the" +
                    $" {nameof(IEasyAnimatorComponent)}.{nameof(IEasyAnimatorComponent.Animator)}.");
            }
#endif

            Component = EasyAnimator;

            var output = _Graph.GetOutput(0);
            if (output.IsOutputValid())
                _Graph.DestroyOutput(output);

            var isHumanoid = animator.isHuman;

            KeepChildrenConnected = !isHumanoid;

            SkipFirstFade = isHumanoid || animator.runtimeAnimatorController == null;

            AnimationPlayableUtilities.Play(animator, _RootPlayable, _Graph);
            _IsGraphPlaying = true;
        }

         

      
        public void InsertOutputPlayable(Playable playable)
        {
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
        }

       
        public AnimationScriptPlayable InsertOutputJob<T>(T data) where T : struct, IAnimationJob
        {
            var playable = AnimationScriptPlayable.Create(_Graph, data, 1);
            var output = _Graph.GetOutput(0);
            _Graph.Connect(output.GetSourcePlayable(), 0, playable, 0);
            playable.SetInputWeight(0, 1);
            output.SetSourcePlayable(playable);
            return playable;
        }

         

        #endregion
         
        #region Cleanup
         

       
        public bool IsValid => _Graph.IsValid();

        public void Destroy()
        {
            if (_Graph.IsValid())
                _Graph.Destroy();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            var previous = Current;
            Current = this;

            DisposeAll();
            GC.SuppressFinalize(this);


            Layers = null;
            States = null;

            Current = previous;
        }

         

        private List<IDisposable> _Disposables;

        public List<IDisposable> Disposables => _Disposables ?? (_Disposables = new List<IDisposable>());

         

        ~EasyAnimatorPlayable() => DisposeAll();

        private void DisposeAll()
        {
            if (_Disposables == null)
                return;

            var i = _Disposables.Count;
            DisposeNext:
            try
            {
                while (--i >= 0)
                {
                    _Disposables[i].Dispose();
                }

                _Disposables.Clear();
                _Disposables = null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto DisposeNext;
            }
        }

         
        #region Inverse Kinematics
         

        private bool _ApplyAnimatorIK;

        public bool ApplyAnimatorIK
        {
            get => _ApplyAnimatorIK;
            set
            {
                _ApplyAnimatorIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers._Layers[i].ApplyAnimatorIK = value;
            }
        }

         

        private bool _ApplyFootIK;

        public bool ApplyFootIK
        {
            get => _ApplyFootIK;
            set
            {
                _ApplyFootIK = value;

                for (int i = Layers.Count - 1; i >= 0; i--)
                    Layers._Layers[i].ApplyFootIK = value;
            }
        }

         
        #endregion
         
        #endregion
         
        #region Playing
         

        public object GetKey(AnimationClip clip) => Component != null ? Component.GetKey(clip) : clip;

         
        // Play Immediately.
         

       
        public EasyAnimatorState Play(AnimationClip clip)
            => Play(States.GetOrCreate(clip));

       
        public EasyAnimatorState Play(EasyAnimatorState state)
        {
            var layer = state.Layer ?? Layers[0];
            return layer.Play(state);
        }

         
        // Cross Fade.
         

       
        public EasyAnimatorState Play(AnimationClip clip, float fadeDuration, FadeMode mode = default)
            => Play(States.GetOrCreate(clip), fadeDuration, mode);

     
        public EasyAnimatorState Play(EasyAnimatorState state, float fadeDuration, FadeMode mode = default)
        {
            var layer = state.Layer ?? Layers[0];
            return layer.Play(state, fadeDuration, mode);
        }

         
        // Transition.
         

      
        public EasyAnimatorState Play(ITransition transition)
            => Play(transition, transition.FadeDuration, transition.FadeMode);

        
        public EasyAnimatorState Play(ITransition transition, float fadeDuration, FadeMode mode = default)
        {
            var state = States.GetOrCreate(transition);
            state = Play(state, fadeDuration, mode);
            transition.Apply(state);
            return state;
        }

       
        public EasyAnimatorState TryPlay(object key)
            => States.TryGet(key, out var state) ? Play(state) : null;

       
        public EasyAnimatorState TryPlay(object key, float fadeDuration, FadeMode mode = default)
            => States.TryGet(key, out var state) ? Play(state, fadeDuration, mode) : null;

         

       
        public EasyAnimatorState Stop(IHasKey hasKey) => Stop(hasKey.Key);

      
        public EasyAnimatorState Stop(object key)
        {
            if (States.TryGet(key, out var state))
                state.Stop();

            return state;
        }

       
        public void Stop()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
                Layers._Layers[i].Stop();
        }

         

        public bool IsPlaying(IHasKey hasKey) => IsPlaying(hasKey.Key);

        public bool IsPlaying(object key) => States.TryGet(key, out var state) && state.IsPlaying;

        public bool IsPlaying()
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                if (Layers._Layers[i].IsAnyStatePlaying())
                    return true;
            }

            return false;
        }

         

      
        public bool IsPlayingClip(AnimationClip clip)
        {
            if (!_IsGraphPlaying)
                return false;

            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers._Layers[i].IsPlayingClip(clip))
                    return true;

            return false;
        }

         

        public float GetTotalWeight()
        {
            float weight = 0;

            for (int i = Layers.Count - 1; i >= 0; i--)
                weight += Layers._Layers[i].GetTotalWeight();

            return weight;
        }

         

        public void GatherAnimationClips(ICollection<AnimationClip> clips) => Layers.GatherAnimationClips(clips);

         
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
         

        bool IEnumerator.MoveNext()
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
                if (Layers._Layers[i].IsPlayingAndNotEnding())
                    return true;

            return false;
        }

        object IEnumerator.Current => null;

        void IEnumerator.Reset() { }

         
        #region Key Error Methods
#if UNITY_EDITOR
      
        [System.Obsolete("You should not use an EasyAnimatorState as a key. Just call EasyAnimatorState.Stop().", true)]
        public EasyAnimatorState Stop(EasyAnimatorState key)
        {
            key.Stop();
            return key;
        }

       
        [System.Obsolete("You should not use an EasyAnimatorState as a key. Just check EasyAnimatorState.IsPlaying.", true)]
        public bool IsPlaying(EasyAnimatorState key) => key.IsPlaying;

         
#endif
        #endregion
         
        #endregion
         
        #region Evaluation
         

        private bool _IsGraphPlaying = true;

        public bool IsGraphPlaying
        {
            get => _IsGraphPlaying;
            set
            {
                if (value)
                    UnpauseGraph();
                else
                    PauseGraph();
            }
        }

        public void UnpauseGraph()
        {
            if (!_IsGraphPlaying)
            {
                _Graph.Play();
                _IsGraphPlaying = true;

#if UNITY_EDITOR
                // In Edit Mode, unpausing the graph does not work properly unless we force it to change.
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    Evaluate(Time.maximumDeltaTime);
#endif
            }
        }

      
        public void PauseGraph()
        {
            if (_IsGraphPlaying)
            {
                _Graph.Stop();
                _IsGraphPlaying = false;
            }
        }

         

       
        public void Evaluate() => _Graph.Evaluate();

      
        public void Evaluate(float deltaTime) => _Graph.Evaluate(deltaTime);

         

        public string GetDescription()
        {
            var text = ObjectPool.AcquireStringBuilder();
            AppendDescription(text);
            return text.ReleaseToString();
        }

        public void AppendDescription(StringBuilder text)
        {
            text.Append($"{nameof(EasyAnimatorPlayable)} (")
                .Append(Component)
                .Append(") Layer Count: ")
                .Append(Layers.Count);

            const string separator = "\n    ";
            EasyAnimatorNode.AppendIKDetails(text, separator, this);

            var count = Layers.Count;
            for (int i = 0; i < count; i++)
            {
                text.Append(separator);
                Layers[i].AppendDescription(text, separator);
            }

            text.AppendLine();
            AppendInternalDetails(text, Strings.Indent, Strings.Indent + Strings.Indent);
        }

        public void AppendInternalDetails(StringBuilder text, string sectionPrefix, string itemPrefix)
        {
            AppendAll(text, sectionPrefix, itemPrefix, _PreUpdatables, "Pre Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _PostUpdatables, "Post Updatables");
            text.AppendLine();
            AppendAll(text, sectionPrefix, itemPrefix, _Disposables, "Disposables");
        }

        private static void AppendAll(StringBuilder text, string sectionPrefix, string itemPrefix, ICollection collection, string name)
        {
            var count = collection != null ? collection.Count : 0;
            text.Append(sectionPrefix).Append(name).Append(": ").Append(count);
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    text.AppendLine().Append(itemPrefix).Append(item);
                }
            }
        }

         
        #endregion
         
        #region Update
         

        
        public void RequirePreUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is EasyAnimatorNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertRoot(node, this);
            }
#endif

            _PreUpdatables.AddNew(updatable);
        }

         

        
        public void RequirePostUpdate(IUpdatable updatable)
        {
#if UNITY_ASSERTIONS
            if (updatable is EasyAnimatorNode node)
            {
                Validate.AssertPlayable(node);
                Validate.AssertRoot(node, this);
            }
#endif

            _PostUpdatables.AddNew(updatable);
        }

         

      
        private void CancelUpdate(Key.KeyedList<IUpdatable> updatables, IUpdatable updatable)
        {
            var index = updatables.IndexOf(updatable);
            if (index < 0)
                return;

            updatables.RemoveAtSwap(index);

            if (_CurrentUpdatable < index && updatables == _CurrentUpdatables)
                _CurrentUpdatable--;
        }

    
        public void CancelPreUpdate(IUpdatable updatable) => CancelUpdate(_PreUpdatables, updatable);

    
        public void CancelPostUpdate(IUpdatable updatable) => CancelUpdate(_PostUpdatables, updatable);

         

        public int PreUpdatableCount => _PreUpdatables.Count;

        public int PostUpdatableCount => _PostUpdatables.Count;

         

        public IUpdatable GetPreUpdatable(int index) => _PreUpdatables[index];

        public IUpdatable GetPostUpdatable(int index) => _PostUpdatables[index];

         

        public static EasyAnimatorPlayable Current { get; private set; }

        public static float DeltaTime { get; private set; }

       
        public ulong FrameID { get; private set; }

        private static Key.KeyedList<IUpdatable> _CurrentUpdatables;

        private static int _CurrentUpdatable = -1;

         

        
        public override void PrepareFrame(Playable playable, FrameData info)
        {
#if UNITY_ASSERTIONS
            if (OptionalWarning.AnimatorSpeed.IsEnabled() && Component != null)
            {
                var animator = Component.Animator;
                if (animator != null &&
                    animator.speed != 1 &&
                    animator.runtimeAnimatorController == null)
                {
                    animator.speed = 1;
                    OptionalWarning.AnimatorSpeed.Log(
                        $"{nameof(Animator)}.{nameof(Animator.speed)} does not affect {nameof(EasyAnimator)}." +
                        $" Use {nameof(EasyAnimatorPlayable)}.{nameof(Speed)} instead.", animator);
                }
            }
#endif

            UpdateAll(_PreUpdatables, info.deltaTime);

            if (!_KeepChildrenConnected)
                _PostUpdate.IsConnected = _PostUpdatables.Count != 0;

            // Any time before or during this method will still have all Playables at their time from last frame, so we
            // don't want them to think their time is dirty until we are done.
            FrameID = info.frameId;
        }

         

        private void UpdateAll(Key.KeyedList<IUpdatable> updatables, float deltaTime)
        {
            var previous = Current;
            Current = this;

            var previousUpdatables = _CurrentUpdatables;
            _CurrentUpdatables = updatables;

            DeltaTime = deltaTime;

#if UNITY_2021_1_OR_NEWER
            DeltaTime *= Time.timeScale;
#endif

            var previousUpdatable = _CurrentUpdatable;
            _CurrentUpdatable = updatables.Count;
            ContinueNodeLoop:
            try
            {
                while (--_CurrentUpdatable >= 0)
                {
                    updatables[_CurrentUpdatable].Update();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, Component as Object);
                goto ContinueNodeLoop;
            }
            _CurrentUpdatable = previousUpdatable;

            _CurrentUpdatables = previousUpdatables;
            Current = previous;
        }

         
        #region Post Update
         

        public static bool IsRunningPostUpdate(EasyAnimatorPlayable EasyAnimator) => _CurrentUpdatables == EasyAnimator._PostUpdatables;

         

        
        private sealed class PostUpdate : PlayableBehaviour
        {
             

            private static readonly PostUpdate Template = new PostUpdate();

            private EasyAnimatorPlayable _Root;

            private Playable _Playable;

             

            public static PostUpdate Create(EasyAnimatorPlayable root)
            {
                var instance = ScriptPlayable<PostUpdate>.Create(root._Graph, Template, 0)
                    .GetBehaviour();
                instance._Root = root;
                return instance;
            }

             

            public override void OnPlayableCreate(Playable playable) => _Playable = playable;

             

            private bool _IsConnected;

            public bool IsConnected
            {
                get => _IsConnected;
                set
                {
                    if (value)
                    {
                        if (!_IsConnected)
                        {
                            _IsConnected = true;
                            _Root._Graph.Connect(_Playable, 0, _Root._RootPlayable, 1);
                        }
                    }
                    else
                    {
                        if (_IsConnected)
                        {
                            _IsConnected = false;
                            _Root._Graph.Disconnect(_Root._RootPlayable, 1);
                        }
                    }
                }
            }

             

           
            public override void PrepareFrame(Playable playable, FrameData info)
            {
                _Root.UpdateAll(_Root._PostUpdatables, info.deltaTime);

               
            }

             
        }

         
        #endregion
         
        #endregion
         
        #region Editor
#if UNITY_EDITOR
         

        private static List<EasyAnimatorPlayable> _AllInstances;

        private void RegisterInstance()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (_AllInstances == null)
            {
                _AllInstances = new List<EasyAnimatorPlayable>();
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += () =>
                {
                    for (int i = _AllInstances.Count - 1; i >= 0; i--)
                    {
                        var playable = _AllInstances[i];
                        if (playable.IsValid)
                            playable.Destroy();
                    }

                    _AllInstances.Clear();
                };
            }
            else// Clear out any old instances.
            {
                for (int i = _AllInstances.Count - 1; i >= 0; i--)
                {
                    var playable = _AllInstances[i];
                    if (!playable.ShouldStayAlive())
                    {
                        if (playable.IsValid)
                            playable.Destroy();

                        _AllInstances.RemoveAt(i);
                    }
                }
            }

            _AllInstances.Add(this);
        }

         

        private bool ShouldStayAlive()
        {
            if (!IsValid)
                return false;

            if (Component == null)
                return true;

            if (Component is Object obj && obj == null)
                return false;

            if (Component.Animator == null)
                return false;

            return true;
        }

         

        
        public static bool HasChangedToOrFromAnimatePhysics(AnimatorUpdateMode? initial, AnimatorUpdateMode current)
        {
            if (initial == null)
                return false;

            var wasAnimatePhysics = initial.Value == AnimatorUpdateMode.AnimatePhysics;
            var isAnimatePhysics = current == AnimatorUpdateMode.AnimatePhysics;
            return wasAnimatePhysics != isAnimatePhysics;
        }

         
#endif
        #endregion
         
    }
}

