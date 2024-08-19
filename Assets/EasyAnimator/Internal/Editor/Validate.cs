

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace EasyAnimator
{
    
    public static partial class Validate
    {
         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertNotLegacy(AnimationClip clip)
        {
#if UNITY_ASSERTIONS
            if (clip.legacy)
                throw new ArgumentException(
                    $"Legacy clip '{clip.name}' cannot be used by EasyAnimator." +
                    " Set the legacy property to false before using this clip." +
                    " If it was imported as part of a model then the model's Rig type must be changed to Humanoid or Generic." +
                    " Otherwise you can use the 'Toggle Legacy' function in the clip's context menu" +
                    " (via the cog icon in the top right of its Inspector).");
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertRoot(EasyAnimatorNode node, EasyAnimatorPlayable root)
        {
#if UNITY_ASSERTIONS
            if (node.Root != root)
                throw new ArgumentException(
                    $"{nameof(EasyAnimatorNode)}.{nameof(EasyAnimatorNode.Root)} mismatch:" +
                    $" cannot use a node in an {nameof(EasyAnimatorPlayable)} that is not its {nameof(EasyAnimatorNode.Root)}: " +
                    node.GetDescription());
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertPlayable(EasyAnimatorNode node)
        {
#if UNITY_ASSERTIONS
            if (node._Playable.IsValid())
                return;

            var description = node.ToString();

            if (node is EasyAnimatorState state)
                state.Destroy();

            if (node.Root == null)
                throw new InvalidOperationException(
                    $"{nameof(EasyAnimatorNode)}.{nameof(EasyAnimatorNode.Root)} hasn't been set so it's" +
                    $" {nameof(Playable)} hasn't been created. It can be set by playing the state" +
                    $" or calling {nameof(EasyAnimatorState.SetRoot)} on it directly." +
                    $" {nameof(EasyAnimatorState.SetParent)} would also work if the parent has a {nameof(EasyAnimatorNode.Root)}." +
                    $"\n• State: {description}");
            else
                throw new InvalidOperationException(
                    $"{nameof(EasyAnimatorNode)}.{nameof(IPlayableWrapper.Playable)} has not been created." +
                    $" {nameof(EasyAnimatorNode.CreatePlayable)} likely needs to be called on it before performing this operation." +
                    $"\n• State: {description}");
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertCanRemoveChild(EasyAnimatorState state, IList<EasyAnimatorState> states)
        {
#if UNITY_ASSERTIONS
            var index = state.Index;

            if (index < 0)
                throw new InvalidOperationException(
                    $"Cannot remove a child state that did not have an {nameof(state.Index)} assigned");

            if (index > states.Count)
                throw new IndexOutOfRangeException(
                    $"{nameof(EasyAnimatorState)}.{nameof(state.Index)} ({state.Index})" +
                    $" is outside the collection of states (count {states.Count})");

            if (states[state.Index] != state)
                throw new InvalidOperationException(
                    $"Cannot remove a child state that was not actually connected to its port on {state.Parent}:" +
                    $"\n• Port: {state.Index}" +
                    $"\n• Connected Child: {EasyAnimatorUtilities.ToStringOrNull(states[state.Index])}" +
                    $"\n• Disconnecting Child: {EasyAnimatorUtilities.ToStringOrNull(state)}");
#endif
        }

         
    }
}

