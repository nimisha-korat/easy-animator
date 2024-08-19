

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EasyAnimator
{
    
    partial class EasyAnimatorPlayable
    {
       
        public sealed class LayerList : IEnumerable<EasyAnimatorLayer>, IAnimationClipCollection
        {
             
            #region Fields
             

            private readonly EasyAnimatorPlayable Root;

            internal EasyAnimatorLayer[] _Layers;

            private readonly AnimationLayerMixerPlayable LayerMixer;

            private int _Count;

             

            internal LayerList(EasyAnimatorPlayable root, out Playable layerMixer)
            {
                Root = root;
                _Layers = new EasyAnimatorLayer[DefaultCapacity];
                layerMixer = LayerMixer = AnimationLayerMixerPlayable.Create(root._Graph, 1);
                Root._Graph.Connect(layerMixer, 0, Root._RootPlayable, 0);
            }

             
            #endregion
             
            #region List Operations
             

           
            public int Count
            {
                get => _Count;
                set
                {
                    var count = _Count;

                    if (value == count)
                        return;

                    CheckAgain:

                    if (value > count)// Increasing.
                    {
                        Add();
                        count++;
                        goto CheckAgain;
                    }
                    else// Decreasing.
                    {
                        while (value < count--)
                        {
                            var layer = _Layers[count];
                            if (layer._Playable.IsValid())
                                Root._Graph.DestroySubgraph(layer._Playable);
                            layer.DestroyStates();
                        }

                        Array.Clear(_Layers, value, _Count - value);

                        _Count = value;

                        Root._LayerMixer.SetInputCount(value);
                    }
                }
            }

             

            public void SetMinCount(int min)
            {
                if (Count < min)
                    Count = min;
            }

             

           
            public static int DefaultCapacity { get; set; } = 4;

            public static void SetMinDefaultCapacity(int min)
            {
                if (DefaultCapacity < min)
                    DefaultCapacity = min;
            }

             

           
            public int Capacity
            {
                get => _Layers.Length;
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value), $"must be greater than 0 ({value} <= 0)");

                    if (_Count > value)
                        Count = value;

                    Array.Resize(ref _Layers, value);
                }
            }

             

           
            public EasyAnimatorLayer Add()
            {
                var index = _Count;

                if (index >= _Layers.Length)
                    throw new InvalidOperationException(
                        "Attempted to increase the layer count above the current capacity (" +
                        (index + 1) + " > " + _Layers.Length + "). This is simply a safety measure," +
                        " so if you do actually need more layers you can just increase the " +
                        $"{nameof(Capacity)} or {nameof(DefaultCapacity)}.");

                _Count = index + 1;
                Root._LayerMixer.SetInputCount(_Count);

                var layer = new EasyAnimatorLayer(Root, index);
                _Layers[index] = layer;
                return layer;
            }

             

            public EasyAnimatorLayer this[int index]
            {
                get
                {
                    SetMinCount(index + 1);
                    return _Layers[index];
                }
            }

             
            #endregion
             
            #region Enumeration
             

            public FastEnumerator<EasyAnimatorLayer> GetEnumerator()
                => new FastEnumerator<EasyAnimatorLayer>(_Layers, _Count);

            IEnumerator<EasyAnimatorLayer> IEnumerable<EasyAnimatorLayer>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

             

            public void GatherAnimationClips(ICollection<AnimationClip> clips) => clips.GatherFromSource(_Layers);

             
            #endregion
             
            #region Layer Details
             

        
            public bool IsAdditive(int index)
            {
                return LayerMixer.IsLayerAdditive((uint)index);
            }

            
            public void SetAdditive(int index, bool value)
            {
                SetMinCount(index + 1);
                LayerMixer.SetLayerAdditive((uint)index, value);
            }

             

           
            public void SetMask(int index, AvatarMask mask)
            {
                SetMinCount(index + 1);

#if UNITY_ASSERTIONS
                _Layers[index]._Mask = mask;
#endif

                if (mask == null)
                    mask = new AvatarMask();

                LayerMixer.SetLayerMaskFromAvatarMask((uint)index, mask);
            }

             

            
            [System.Diagnostics.Conditional(Strings.UnityEditor)]
            public void SetName(int index, string name) => this[index].SetDebugName(name);

             

           
            public Vector3 AverageVelocity
            {
                get
                {
                    var velocity = default(Vector3);

                    for (int i = 0; i < _Count; i++)
                    {
                        var layer = _Layers[i];
                        velocity += layer.AverageVelocity * layer.Weight;
                    }

                    return velocity;
                }
            }

             

            internal void SetWeightlessChildrenConnected(bool connected)
            {
                if (connected)
                {
                    for (int i = _Count - 1; i >= 0; i--)
                        _Layers[i].ConnectAllChildrenToGraph();
                }
                else
                {
                    for (int i = _Count - 1; i >= 0; i--)
                        _Layers[i].DisconnectWeightlessChildrenFromGraph();
                }
            }

             
            #endregion
             
        }
    }
}

