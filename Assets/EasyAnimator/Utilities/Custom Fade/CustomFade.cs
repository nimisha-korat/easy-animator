

using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
    
    public abstract partial class CustomFade : Key, IUpdatable
    {
         

        private float _Time;
        private float _FadeSpeed;
        private NodeWeight _Target;
        private EasyAnimatorLayer _Layer;
        private int _CommandCount;

        private readonly List<NodeWeight> FadeOutNodes = new List<NodeWeight>();

         

        private readonly struct NodeWeight
        {
            public readonly EasyAnimatorNode Node;
            public readonly float StartingWeight;

            public NodeWeight(EasyAnimatorNode node)
            {
                Node = node;
                StartingWeight = node.Weight;
            }
        }

         

        protected void Apply(EasyAnimatorState state)
        {
            EasyAnimatorUtilities.Assert(state.Parent != null, "Node is not connected to a layer.");

            Apply((EasyAnimatorNode)state);

            var parent = state.Parent;
            for (int i = parent.ChildCount - 1; i >= 0; i--)
            {
                var other = parent.GetChild(i);
                if (other != state && other.FadeSpeed != 0)
                {
                    other.FadeSpeed = 0;
                    FadeOutNodes.Add(new NodeWeight(other));
                }
            }
        }

        protected void Apply(EasyAnimatorNode node)
        {
#if UNITY_ASSERTIONS
            EasyAnimatorUtilities.Assert(node != null, "Node is null.");
            EasyAnimatorUtilities.Assert(node.IsValid, "Node is not valid.");
            EasyAnimatorUtilities.Assert(node.FadeSpeed != 0, $"Node is not fading ({nameof(node.FadeSpeed)} is 0).");

            var EasyAnimator = node.Root;
            EasyAnimatorUtilities.Assert(EasyAnimator != null, $"{nameof(node)}.{nameof(node.Root)} is null.");

            if (OptionalWarning.CustomFadeBounds.IsEnabled())
            {
                if (CalculateWeight(0) != 0)
                    OptionalWarning.CustomFadeBounds.Log("CalculateWeight(0) != 0.", EasyAnimator.Component);
                if (CalculateWeight(1) != 1)
                    OptionalWarning.CustomFadeBounds.Log("CalculateWeight(1) != 1.", EasyAnimator.Component);
            }
#endif

            _Time = 0;
            _Target = new NodeWeight(node);
            _FadeSpeed = node.FadeSpeed;
            _Layer = node.Layer;
            _CommandCount = _Layer.CommandCount;

            node.FadeSpeed = 0;

            FadeOutNodes.Clear();

            node.Root.RequirePreUpdate(this);
        }

         

        protected abstract float CalculateWeight(float progress);

        protected abstract void Release();

         

        void IUpdatable.Update()
        {
            // Stop fading if the state was destroyed or something else was played.
            if (!_Target.Node.IsValid() ||
                _Layer != _Target.Node.Layer ||
                _CommandCount != _Layer.CommandCount)
            {
                FadeOutNodes.Clear();
                _Layer.Root.CancelPreUpdate(this);
                Release();
                return;
            }

            _Time += EasyAnimatorPlayable.DeltaTime * _Layer.Speed * _FadeSpeed;

            if (_Time < 1)// Fade.
            {
                var weight = CalculateWeight(_Time);

                _Target.Node.SetWeight(Mathf.LerpUnclamped(_Target.StartingWeight, _Target.Node.TargetWeight, weight));

                weight = 1 - weight;
                for (int i = FadeOutNodes.Count - 1; i >= 0; i--)
                {
                    var node = FadeOutNodes[i];
                    node.Node.SetWeight(node.StartingWeight * weight);
                }
            }
            else// End.
            {
                _Time = 1;
                ForceFinishFade(_Target.Node);

                for (int i = FadeOutNodes.Count - 1; i >= 0; i--)
                    ForceFinishFade(FadeOutNodes[i].Node);

                FadeOutNodes.Clear();
                _Layer.Root.CancelPreUpdate(this);
                Release();
            }
        }

         

        private static void ForceFinishFade(EasyAnimatorNode node)
        {
            var weight = node.TargetWeight;
            node.SetWeight(weight);
            if (weight == 0)
                node.Stop();
        }

         
    }
}
