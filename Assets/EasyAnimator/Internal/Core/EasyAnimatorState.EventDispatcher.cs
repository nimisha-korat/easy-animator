
using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyAnimator
{
  
    partial class EasyAnimatorState
    {
         

     
        private EventDispatcher _EventDispatcher;

      
        public EasyAnimatorEvent.Sequence Events
        {
            get
            {
                EventDispatcher.Acquire(this);
                return _EventDispatcher.Events;
            }
            set
            {
                if (value != null)
                {
                    EventDispatcher.Acquire(this);
                    _EventDispatcher.Events = value;
                }
                else if (_EventDispatcher != null)
                {
                    _EventDispatcher.Events = null;
                }
            }
        }

         

      
        public bool HasEvents => _EventDispatcher != null;

         

       
        public static bool AutomaticallyClearEvents { get; set; } = true;

         

#if UNITY_ASSERTIONS
     
        protected virtual string UnsupportedEventsMessage => null;
#endif

         

      
        public sealed class EventDispatcher : Key, IUpdatable
        {
             
            #region Pooling
             

       
            internal static void Acquire(EasyAnimatorState state)
            {
                ref var dispatcher = ref state._EventDispatcher;
                if (dispatcher != null)
                    return;

                ObjectPool.Acquire(out dispatcher);

#if UNITY_ASSERTIONS
                dispatcher._LoggedEndEventInterrupt = false;

                OptionalWarning.UnsupportedEvents.Log(state.UnsupportedEventsMessage, state.Root?.Component);

                if (dispatcher._State != null)
                    Debug.LogError(dispatcher + " already has a state even though it was in the list of spares.",
                        state.Root?.Component as Object);

                if (dispatcher._Events != null)
                    Debug.LogError(dispatcher + " has event sequence even though it was in the list of spares.",
                        state.Root?.Component as Object);

                if (dispatcher._GotEventsFromPool)
                    Debug.LogError(dispatcher + " is marked as having pooled events even though it has no events.",
                        state.Root?.Component as Object);

                if (dispatcher._NextEventIndex != RecalculateEventIndex)
                    Debug.LogError($"{dispatcher} has a {nameof(_NextEventIndex)} even though it was pooled.",
                        state.Root?.Component as Object);

                if (IsInList(dispatcher))
                    Debug.LogError(dispatcher + " is currently in a Keyed List even though it was also in the list of spares.",
                        state.Root?.Component as Object);
#endif

                dispatcher._IsLooping = state.IsLooping;
                dispatcher._PreviousTime = state.NormalizedTime;
                dispatcher._State = state;
                state.Root?.RequirePostUpdate(dispatcher);
            }

             

            private void Release()
            {
                if (_State == null)
                    return;

                _State.Root?.CancelPostUpdate(this);
                _State._EventDispatcher = null;
                _State = null;

                Events = null;

                ObjectPool.Release(this);
            }

             

           
            internal static void TryClear(EventDispatcher events)
            {
                if (events != null)
                    events.Events = null;
            }

             
            #endregion
             

            private EasyAnimatorState _State;
            private EasyAnimatorEvent.Sequence _Events;
            private bool _GotEventsFromPool;
            private bool _IsLooping;
            private float _PreviousTime;
            private int _NextEventIndex = RecalculateEventIndex;
            private int _SequenceVersion;
            private bool _WasPlayingForwards;

           
            private const int RecalculateEventIndex = int.MinValue;

            private const string SequenceVersionException =
                nameof(EasyAnimatorState) + "." + nameof(EasyAnimatorState.Events) + " sequence was modified while iterating through it." +
                " Events in a sequence must not modify that sequence.";

             

            internal EasyAnimatorEvent.Sequence Events
            {
                get
                {
                    if (_Events == null)
                    {
                        ObjectPool.Acquire(out _Events);
                        _GotEventsFromPool = true;

#if UNITY_ASSERTIONS
                        if (!_Events.IsEmpty)
                            Debug.LogError(_Events + " is not in its default state even though it was in the list of spares.",
                            _State?.Root?.Component as Object);
#endif
                    }

                    return _Events;
                }
                set
                {
                    if (_GotEventsFromPool)
                    {
                        _Events.Clear();
                        ObjectPool.Release(_Events);
                        _GotEventsFromPool = false;
                    }

                    _Events = value;
                    _NextEventIndex = RecalculateEventIndex;
                }
            }

             

            void IUpdatable.Update()
            {
                if (_Events == null || _Events.IsEmpty)
                {
                    Release();
                    return;
                }

                var length = _State.Length;
                if (length == 0)
                {
                    UpdateZeroLength();
                    return;
                }

                var currentTime = _State.Time / length;
                if (_PreviousTime == currentTime)
                    return;

               
                CheckGeneralEvents(currentTime);
                if (_Events == null)
                {
                    Release();
                    return;
                }

                
                var endEvent = _Events.endEvent;
                if (endEvent.callback != null)
                {

                    if (currentTime > _PreviousTime)// Playing Forwards.
                    {
                        var eventTime = float.IsNaN(endEvent.normalizedTime) ?
                            1 : endEvent.normalizedTime;

                        if (currentTime > eventTime)
                        {
                            ValidateBeforeEndEvent();
                            endEvent.Invoke(_State);
                            ValidateAfterEndEvent(endEvent.callback);
                        }
                    }
                    else// Playing Backwards.
                    {
                        var eventTime = float.IsNaN(endEvent.normalizedTime) ?
                            0 : endEvent.normalizedTime;

                        if (currentTime < eventTime)
                        {
                            ValidateBeforeEndEvent();
                            endEvent.Invoke(_State);
                            ValidateAfterEndEvent(endEvent.callback);
                        }
                    }
                }

                _PreviousTime = currentTime;
            }

             
            #region End Event Validation
             

#if UNITY_ASSERTIONS
            private bool _LoggedEndEventInterrupt;

            private static EasyAnimatorLayer _BeforeEndLayer;
            private static int _BeforeEndCommandCount;
#endif

             

           
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void ValidateBeforeEndEvent()
            {
#if UNITY_ASSERTIONS
                _BeforeEndLayer = _State.Layer;
                _BeforeEndCommandCount = _BeforeEndLayer.CommandCount;
#endif
            }

             

           
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void ValidateAfterEndEvent(Action callback)
            {
#if UNITY_ASSERTIONS
                if (!_LoggedEndEventInterrupt &&
                    _Events != null)
                {
                    var layer = _State.Layer;
                    if (_BeforeEndLayer == layer &&
                        _BeforeEndCommandCount == layer.CommandCount &&
                       _State.Root.IsGraphPlaying &&
                       _State.IsPlaying &&
                       _State.EffectiveSpeed != 0)
                    {
                        _LoggedEndEventInterrupt = true;
                        if (OptionalWarning.EndEventInterrupt.IsEnabled())
                            OptionalWarning.EndEventInterrupt.Log(
                                "An End Event did not actually end the animation:" +
                                $"\n - State: {_State}" +
                                $"\n - Callback: {callback.Method.DeclaringType.Name}.{callback.Method.Name}" +
                                "\n\nEnd Events are triggered every frame after their time has passed, so if that is not desired behaviour" +
                                " then it might be necessary to explicitly clear the event or simply use a regular event instead.",
                                _State.Root?.Component);
                    }
                }

                if (OptionalWarning.DuplicateEvent.IsDisabled())
                    return;

                if (!EasyAnimatorUtilities.TryGetInvocationListNonAlloc(callback, out var delegates) ||
                    delegates == null)
                    return;

                var count = delegates.Length;
                for (int iA = 0; iA < count; iA++)
                {
                    var a = delegates[iA];
                    for (int iB = iA + 1; iB < count; iB++)
                    {
                        var b = delegates[iB];

                        if (a == b)
                        {
                            OptionalWarning.DuplicateEvent.Log(
                                $"The {nameof(EasyAnimatorEvent)}.{nameof(EasyAnimatorEvent.Sequence)}.{nameof(EasyAnimatorEvent.Sequence.OnEnd)}" +
                                " callback being invoked contains multiple identical delegates which may mean" +
                                " that they are being unintentionally added multiple times." +
                                $"\n - State: {_State}" +
                                $"\n - Method: {a.Method.Name}",
                                _State.Root?.Component);
                        }
                        else if (a?.Method == b?.Method)
                        {
                            OptionalWarning.DuplicateEvent.Log(
                                $"The {nameof(EasyAnimatorEvent)}.{nameof(EasyAnimatorEvent.Sequence)}.{nameof(EasyAnimatorEvent.Sequence.OnEnd)}" +
                                " callback being invoked contains multiple delegates using the same method with different targets." +
                                " This often happens when a Transition is shared by multiple objects," +
                                " in which case it can be avoided by giving each object its own" +
                                $" {nameof(EasyAnimatorEvent)}.{nameof(EasyAnimatorEvent.Sequence)} as explained in the documentation:" +
                                $" {Strings.DocsURLs.SharedEventSequences}" +
                                $"\n - State: {_State}" +
                                $"\n - Method: {a.Method.Name}",
                                _State.Root?.Component);
                        }
                    }
                }
#endif
            }

             
            #endregion
             

            internal void OnTimeChanged()
            {
                _PreviousTime = _State.NormalizedTime;
                _NextEventIndex = RecalculateEventIndex;
            }

             

            private void UpdateZeroLength()
            {
                var speed = _State.EffectiveSpeed;
                if (speed == 0)
                    return;

                if (_Events.Count > 0)
                {
                    var sequenceVersion = _Events.Version;

                    int playDirectionInt;
                    if (speed < 0)
                    {
                        playDirectionInt = -1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            _SequenceVersion != sequenceVersion ||
                            _WasPlayingForwards)
                        {
                            _NextEventIndex = Events.Count - 1;
                            _SequenceVersion = sequenceVersion;
                            _WasPlayingForwards = false;
                        }
                    }
                    else
                    {
                        playDirectionInt = 1;
                        if (_NextEventIndex == RecalculateEventIndex ||
                            _SequenceVersion != sequenceVersion ||
                            !_WasPlayingForwards)
                        {
                            _NextEventIndex = 0;
                            _SequenceVersion = sequenceVersion;
                            _WasPlayingForwards = true;
                        }
                    }

                    if (!InvokeAllEvents(1, playDirectionInt))
                        return;
                }

                var endEvent = _Events.endEvent;
                if (endEvent.callback != null)
                    endEvent.Invoke(_State);
            }

             

            private void CheckGeneralEvents(float currentTime)
            {
                var count = _Events.Count;
                if (count == 0)
                    return;

                ValidateNextEventIndex(ref currentTime, out var playDirectionFloat, out var playDirectionInt);

                if (_IsLooping)// Looping.
                {
                    var EasyAnimatorEvent = _Events[_NextEventIndex];
                    var eventTime = EasyAnimatorEvent.normalizedTime * playDirectionFloat;

                    var loopDelta = GetLoopDelta(_PreviousTime, currentTime, eventTime);
                    if (loopDelta == 0)
                        return;

                    // For each additional loop, invoke all events without needing to check their times.
                    if (!InvokeAllEvents(loopDelta - 1, playDirectionInt))
                        return;

                    var loopStartIndex = _NextEventIndex;

                    Invoke:
                    EasyAnimatorEvent.Invoke(_State);

                    if (!NextEventLooped(playDirectionInt) ||
                        _NextEventIndex == loopStartIndex)
                        return;

                    EasyAnimatorEvent = _Events[_NextEventIndex];
                    eventTime = EasyAnimatorEvent.normalizedTime * playDirectionFloat;
                    if (loopDelta == GetLoopDelta(_PreviousTime, currentTime, eventTime))
                        goto Invoke;
                }
                else// Non-Looping.
                {
                    while ((uint)_NextEventIndex < (uint)count)
                    {
                        var EasyAnimatorEvent = _Events[_NextEventIndex];
                        var eventTime = EasyAnimatorEvent.normalizedTime * playDirectionFloat;

                        if (currentTime <= eventTime)
                            break;

                        EasyAnimatorEvent.Invoke(_State);

                        if (!NextEvent(playDirectionInt))
                            return;
                    }
                }
            }

             

            private void ValidateNextEventIndex(ref float currentTime,
                out float playDirectionFloat, out int playDirectionInt)
            {
                var sequenceVersion = _Events.Version;

                if (currentTime < _PreviousTime)// Playing Backwards.
                {
                    var previousTime = _PreviousTime;
                    _PreviousTime = -previousTime;
                    currentTime = -currentTime;
                    playDirectionFloat = -1;
                    playDirectionInt = -1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        _SequenceVersion != sequenceVersion ||
                        _WasPlayingForwards)
                    {
                        _NextEventIndex = _Events.Count - 1;
                        _SequenceVersion = sequenceVersion;
                        _WasPlayingForwards = false;

                        if (_IsLooping)
                            previousTime = EasyAnimatorUtilities.Wrap01(previousTime);

                        while (_NextEventIndex > 0 &&
                            _Events[_NextEventIndex].normalizedTime > previousTime)
                            _NextEventIndex--;

                        _Events.AssertNormalizedTimes(_State, _IsLooping);
                    }
                }
                else// Playing Forwards.
                {
                    playDirectionFloat = 1;
                    playDirectionInt = 1;

                    if (_NextEventIndex == RecalculateEventIndex ||
                        _SequenceVersion != sequenceVersion ||
                        !_WasPlayingForwards)
                    {
                        _NextEventIndex = 0;
                        _SequenceVersion = sequenceVersion;
                        _WasPlayingForwards = true;

                        var previousTime = _PreviousTime;
                        if (_IsLooping)
                            previousTime = EasyAnimatorUtilities.Wrap01(previousTime);

                        var max = _Events.Count - 1;
                        while (_NextEventIndex < max &&
                            _Events[_NextEventIndex].normalizedTime < previousTime)
                            _NextEventIndex++;

                        _Events.AssertNormalizedTimes(_State, _IsLooping);
                    }
                }

               
            }

             

            private static int GetLoopDelta(float previousTime, float nextTime, float eventTime)
            {
                previousTime -= eventTime;
                var previousLoopCount = Mathf.FloorToInt(previousTime);
                var nextLoopCount = Mathf.FloorToInt(nextTime - eventTime);

                if (previousTime == previousLoopCount)
                    nextLoopCount++;

                return nextLoopCount - previousLoopCount;
            }

             

            private bool InvokeAllEvents(int count, int playDirectionInt)
            {
                var loopStartIndex = _NextEventIndex;
                while (count-- > 0)
                {
                    do
                    {
                        _Events[_NextEventIndex].Invoke(_State);

                        if (!NextEventLooped(playDirectionInt))
                            return false;
                    }
                    while (_NextEventIndex != loopStartIndex);
                }

                return true;
            }

             

            private bool NextEvent(int playDirectionInt)
            {
                if (_NextEventIndex == RecalculateEventIndex)
                    return false;

                if (_Events.Version != _SequenceVersion)
                    throw new InvalidOperationException(SequenceVersionException);

                _NextEventIndex += playDirectionInt;

                return true;
            }

             

            private bool NextEventLooped(int playDirectionInt)
            {
                if (!NextEvent(playDirectionInt))
                    return false;

                var count = _Events.Count;
                if (_NextEventIndex >= count)
                    _NextEventIndex = 0;
                else if (_NextEventIndex < 0)
                    _NextEventIndex = count - 1;

                return true;
            }

             

            public override string ToString()
            {
                return _State != null ?
                    $"{nameof(EventDispatcher)} ({_State})" :
                    $"{nameof(EventDispatcher)} (No Target State)";
            }

             
        }

         
    }
}

