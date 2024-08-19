
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
   
    partial class EasyAnimatorPlayable
    {
       
        public sealed class StateDictionary : IEnumerable<EasyAnimatorState>, IAnimationClipCollection
        {
             

            private readonly EasyAnimatorPlayable Root;

             

          
            public static bool ReferenceKeysOnly { get; set; }

            private readonly Dictionary<object, EasyAnimatorState>
                States = new Dictionary<object, EasyAnimatorState>(
                    ReferenceKeysOnly ? (IEqualityComparer<object>)FastReferenceComparer.Instance : FastComparer.Instance);

             

            internal StateDictionary(EasyAnimatorPlayable root) => Root = root;

             

            public int Count => States.Count;

             
            #region Create
             

           
            public ClipState Create(AnimationClip clip) => Root.Layers[0].CreateState(clip);

           
            public ClipState Create(object key, AnimationClip clip) => Root.Layers[0].CreateState(key, clip);

             

           
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
            }

           
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
                GetOrCreate(clip2);
            }

           
            public void CreateIfNew(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
            {
                GetOrCreate(clip0);
                GetOrCreate(clip1);
                GetOrCreate(clip2);
                GetOrCreate(clip3);
            }

            public void CreateIfNew(params AnimationClip[] clips)
            {
                if (clips == null)
                    return;

                var count = clips.Length;
                for (int i = 0; i < count; i++)
                {
                    var clip = clips[i];
                    if (clip != null)
                        GetOrCreate(clip);
                }
            }

             
            #endregion
             
            #region Access
             

          
            public EasyAnimatorState Current => Root.Layers[0].CurrentState;

             

         
            public EasyAnimatorState this[AnimationClip clip] => States[Root.GetKey(clip)];

          
            public EasyAnimatorState this[IHasKey hasKey] => States[hasKey.Key];

           
            public EasyAnimatorState this[object key] => States[key];

             

            public bool TryGet(AnimationClip clip, out EasyAnimatorState state)
            {
                if (clip == null)
                {
                    state = null;
                    return false;
                }

                return TryGet(Root.GetKey(clip), out state);
            }

           
            public bool TryGet(IHasKey hasKey, out EasyAnimatorState state)
            {
                if (hasKey == null)
                {
                    state = null;
                    return false;
                }

                return TryGet(hasKey.Key, out state);
            }

           
            public bool TryGet(object key, out EasyAnimatorState state)
            {
                if (key == null)
                {
                    state = null;
                    return false;
                }

                return States.TryGetValue(key, out state);
            }

             

          
            public EasyAnimatorState GetOrCreate(AnimationClip clip, bool allowSetClip = false)
                => GetOrCreate(Root.GetKey(clip), clip, allowSetClip);

            
            public EasyAnimatorState GetOrCreate(ITransition transition)
            {
                var key = transition.Key;
                if (!TryGet(key, out var state))
                {
                    state = transition.CreateState();
                    state.SetRoot(Root);
                    Register(key, state);
                }

                return state;
            }

           
            public EasyAnimatorState GetOrCreate(object key, AnimationClip clip, bool allowSetClip = false)
            {
                if (TryGet(key, out var state))
                {
                    // If a state exists with the 'key' but has the wrong clip, either change it or complain.
                    if (!ReferenceEquals(state.Clip, clip))
                    {
                        if (allowSetClip)
                        {
                            state.Clip = clip;
                        }
                        else
                        {
                            throw new ArgumentException(GetClipMismatchError(key, state.Clip, clip));
                        }
                    }
                }
                else
                {
                    state = Root.Layers[0].CreateState(key, clip);
                }

                return state;
            }

             

            public static string GetClipMismatchError(object key, AnimationClip oldClip, AnimationClip newClip)
                => $"A state already exists using the specified '{nameof(key)}', but has a different {nameof(AnimationClip)}:" +
                $"\n - Key: {key}" +
                $"\n - Old Clip: {oldClip}" +
                $"\n - New Clip: {newClip}";

             

           
            internal void Register(object key, EasyAnimatorState state)
            {
                if (key != null)
                {
#if UNITY_ASSERTIONS
                    if (state.Root != Root)
                        throw new ArgumentException(
                            $"{nameof(StateDictionary)} cannot register a state with a different {nameof(Root)}: " + state);
#endif

                    States.Add(key, state);
                }

                state._Key = key;
            }

            internal void Unregister(EasyAnimatorState state)
            {
                if (state._Key == null)
                    return;

                States.Remove(state._Key);
                state._Key = null;
            }

             
            #region Enumeration
             
            // IEnumerable for 'foreach' statements.
             

            public Dictionary<object, EasyAnimatorState>.ValueCollection.Enumerator GetEnumerator()
                => States.Values.GetEnumerator();

            IEnumerator<EasyAnimatorState> IEnumerable<EasyAnimatorState>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

             

           
            public void GatherAnimationClips(ICollection<AnimationClip> clips)
            {
                foreach (var state in States.Values)
                    clips.GatherFromSource(state);
            }

             
            #endregion
             
            #endregion
             
            #region Destroy
             

            
            public bool Destroy(AnimationClip clip)
            {
                if (clip == null)
                    return false;

                return Destroy(Root.GetKey(clip));
            }

           
            public bool Destroy(IHasKey hasKey)
            {
                if (hasKey == null)
                    return false;

                return Destroy(hasKey.Key);
            }

          
            public bool Destroy(object key)
            {
                if (!TryGet(key, out var state))
                    return false;

                state.Destroy();
                return true;
            }

             

            public void DestroyAll(IList<AnimationClip> clips)
            {
                if (clips == null)
                    return;

                for (int i = clips.Count - 1; i >= 0; i--)
                    Destroy(clips[i]);
            }

            public void DestroyAll(IEnumerable<AnimationClip> clips)
            {
                if (clips == null)
                    return;

                foreach (var clip in clips)
                    Destroy(clip);
            }

             

           
            public void DestroyAll(IAnimationClipSource source)
            {
                if (source == null)
                    return;

                var clips = ObjectPool.AcquireList<AnimationClip>();
                source.GetAnimationClips(clips);
                DestroyAll(clips);
                ObjectPool.Release(clips);
            }

           
            public void DestroyAll(IAnimationClipCollection source)
            {
                if (source == null)
                    return;

                var clips = ObjectPool.AcquireSet<AnimationClip>();
                source.GatherAnimationClips(clips);
                DestroyAll(clips);
                ObjectPool.Release(clips);
            }

             

         
            public void DestroyAll()
            {
                var count = Root.Layers.Count;
                while (--count >= 0)
                    Root.Layers._Layers[count].DestroyStates();

                States.Clear();
            }

             
            #endregion
             
            #region Key Error Methods
#if UNITY_EDITOR
          
            [Obsolete("You should not use an EasyAnimatorState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public EasyAnimatorState this[EasyAnimatorState key] => key;

         
            [Obsolete("You should not use an EasyAnimatorState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public bool TryGet(EasyAnimatorState key, out EasyAnimatorState state)
            {
                state = key;
                return true;
            }

            
            [Obsolete("You should not use an EasyAnimatorState as a key. The whole point of a key is to identify a state in the first place.", true)]
            public EasyAnimatorState GetOrCreate(EasyAnimatorState key, AnimationClip clip) => key;

            [Obsolete("You should not use an EasyAnimatorState as a key. Just call EasyAnimatorState.Destroy.", true)]
            public bool Destroy(EasyAnimatorState key)
            {
                key.Destroy();
                return true;
            }

             
#endif
            #endregion
             
        }
    }
}

