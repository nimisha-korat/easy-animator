

#pragma warning disable CS0649 


#if EasyAnimator_ULT_EVENTS
using SerializableCallback = UltEvents.UltEvent;
#else
using SerializableCallback = UnityEngine.Events.UnityEvent;
#endif

using UnityEngine;
using System;

namespace EasyAnimator
{
 
    partial struct EasyAnimatorEvent
    {
       
        partial class Sequence
        {
           
            [Serializable]
            public class Serializable
#if UNITY_EDITOR
                : ISerializationCallbackReceiver
#endif
            {
                 

                [SerializeField]
                private float[] _NormalizedTimes;

                public ref float[] NormalizedTimes => ref _NormalizedTimes;

                 

                [SerializeField]
                private SerializableCallback[] _Callbacks;

              
                public ref SerializableCallback[] Callbacks => ref _Callbacks;

                 

                [SerializeField]
                private string[] _Names;

                public ref string[] Names => ref _Names;

                 
#if UNITY_EDITOR
                 

                internal const string NormalizedTimesField = nameof(_NormalizedTimes);

                internal const string CallbacksField = nameof(_Callbacks);

                internal const string NamesField = nameof(_Names);

                 
#endif
                 

                private Sequence _Events;

               
                public Sequence Events
                {
                    get
                    {
                        if (_Events == null)
                        {
                            GetEventsOptional();
                            if (_Events == null)
                                _Events = new Sequence();
                        }

                        return _Events;
                    }
                    set => _Events = value;
                }

                 

               
                public Sequence GetEventsOptional()
                {
                    if (_Events != null ||
                        _NormalizedTimes == null)
                        return _Events;

                    var timeCount = _NormalizedTimes.Length;
                    if (timeCount == 0)
                        return null;

                    var callbackCount = _Callbacks.Length;

                    var callback = callbackCount >= timeCount-- ?
                        GetInvoker(_Callbacks[timeCount]) :
                        null;
                    var endEvent = new EasyAnimatorEvent(_NormalizedTimes[timeCount], callback);

                    _Events = new Sequence(timeCount)
                    {
                        endEvent = endEvent,
                        Count = timeCount,
                        _Names = _Names,
                    };

                    for (int i = 0; i < timeCount; i++)
                    {
                        callback = i < callbackCount ? GetInvoker(_Callbacks[i]) : DummyCallback;
                        _Events._Events[i] = new EasyAnimatorEvent(_NormalizedTimes[i], callback);
                    }

                    return _Events;
                }

                public static implicit operator Sequence(Serializable serializable) => serializable?.GetEventsOptional();

                 

                internal Sequence InitializedEvents => _Events;

                 

               
                public static Action GetInvoker(SerializableCallback callback)
                    => HasPersistentCalls(callback) ? callback.Invoke : DummyCallback;

#if UNITY_EDITOR
               
                public static Action GetInvoker(object callback)
                    => GetInvoker((SerializableCallback)callback);
#endif

                 

               
                public static bool HasPersistentCalls(SerializableCallback callback)
                {
                    if (callback == null)
                        return false;

                    // UnityEvents do not allow us to check if any dynamic calls are present.
                    // But we are not giving runtime access to the events so it does not really matter.
                    // UltEvents does allow it (via the HasCalls property), but we might as well be consistent.

#if EasyAnimator_ULT_EVENTS
                    var calls = callback.PersistentCallsList;
                    return calls != null && calls.Count > 0;
#else
                    return callback.GetPersistentEventCount() > 0;
#endif
                }

#if UNITY_EDITOR
               
                public static bool HasPersistentCalls(object callback) => HasPersistentCalls((SerializableCallback)callback);
#endif

                 

                public float GetNormalizedEndTime(float speed = 1)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        return GetDefaultNormalizedEndTime(speed);
                    else
                        return _NormalizedTimes[_NormalizedTimes.Length - 1];
                }

                 

                public void SetNormalizedEndTime(float normalizedTime)
                {
                    if (_NormalizedTimes.IsNullOrEmpty())
                        _NormalizedTimes = new float[] { normalizedTime };
                    else
                        _NormalizedTimes[_NormalizedTimes.Length - 1] = normalizedTime;
                }

                 
#if UNITY_EDITOR
                 

              
                void ISerializationCallbackReceiver.OnAfterDeserialize() { }

                 

                void ISerializationCallbackReceiver.OnBeforeSerialize()
                {
                    if (_NormalizedTimes == null ||
                        _NormalizedTimes.Length <= 2)
                    {
                        CompactArrays();
                        return;
                    }

                    var eventContext = Editor.SerializableEventSequenceDrawer.Context.Current;
                    var selectedEvent = eventContext?.Property != null ? eventContext.SelectedEvent : -1;

                    var timeCount = _NormalizedTimes.Length - 1;

                    var previousTime = _NormalizedTimes[0];

                    // Bubble Sort based on the normalized times.
                    for (int i = 1; i < timeCount; i++)
                    {
                        var time = _NormalizedTimes[i];
                        if (time >= previousTime)
                        {
                            previousTime = time;
                            continue;
                        }

                        _NormalizedTimes.Swap(i, i - 1);
                        DynamicSwap(ref _Callbacks, i);
                        DynamicSwap(ref _Names, i);

                        if (selectedEvent == i)
                            selectedEvent = i - 1;
                        else if (selectedEvent == i - 1)
                            selectedEvent = i;

                        if (i == 1)
                        {
                            i = 0;
                            previousTime = float.NegativeInfinity;
                        }
                        else
                        {
                            i -= 2;
                            previousTime = _NormalizedTimes[i];
                        }
                    }

                    // If the current animation is looping, clamp all times within the 0-1 range.
                    var transitionContext = Editor.TransitionDrawer.Context;
                    if (transitionContext != null &&
                        transitionContext.Transition != null &&
                        transitionContext.Transition.IsLooping)
                    {
                        for (int i = _NormalizedTimes.Length - 1; i >= 0; i--)
                        {
                            var time = _NormalizedTimes[i];
                            if (time < 0)
                                _NormalizedTimes[i] = 0;
                            else if (time > AlmostOne)
                                _NormalizedTimes[i] = AlmostOne;
                        }
                    }

                    // If the selected event was moved adjust the selection.
                    if (eventContext?.Property != null && eventContext.SelectedEvent != selectedEvent)
                    {
                        eventContext.SelectedEvent = selectedEvent;
                        Editor.TransitionPreviewWindow.PreviewNormalizedTime = _NormalizedTimes[selectedEvent];
                    }

                    CompactArrays();
                }

                 

                private static void DynamicSwap<T>(ref T[] array, int index)
                {
                    var count = array != null ? array.Length : 0;

                    if (index == count)
                        Array.Resize(ref array, ++count);

                    if (index < count)
                        array.Swap(index, index - 1);
                }

                 

                private void CompactArrays()
                {
                    if (_NormalizedTimes == null ||
                        _Callbacks == null)
                        return;

                    // If there is only one time and it is NaN, we don't need to store anything.
                    if (_NormalizedTimes.Length == 1 && _Callbacks.Length == 0 && float.IsNaN(_NormalizedTimes[0]))
                    {
                        _NormalizedTimes = Array.Empty<float>();
                        _Callbacks = Array.Empty<SerializableCallback>();
                        _Names = Array.Empty<string>();
                        return;
                    }

                    Trim(ref _Callbacks, _NormalizedTimes.Length, (callback) => HasPersistentCalls(callback));
                    Trim(ref _Names, _NormalizedTimes.Length, (name) => !string.IsNullOrEmpty(name));
                }

                 

                private static void Trim<T>(ref T[] array, int maxLength, Func<T, bool> isImportant)
                {
                    if (array == null)
                        return;

                    var count = Math.Min(array.Length, maxLength);

                    while (count >= 1)
                    {
                        var item = array[count - 1];
                        if (isImportant(item))
                            break;
                        else
                            count--;
                    }

                    Array.Resize(ref array, count);
                }

                 
#endif
                 
            }
        }
    }
}

 
#if UNITY_EDITOR
 

namespace EasyAnimator.Editor
{
 
    [Serializable]
    internal sealed class SerializableCallbackHolder
    {
#pragma warning disable CS0169 // Field is never used.
        [SerializeField]
        private SerializableCallback _Callback;
#pragma warning restore CS0169 // Field is never used.

        internal const string CallbackField = nameof(_Callback);
    }
}

 
#endif
 

