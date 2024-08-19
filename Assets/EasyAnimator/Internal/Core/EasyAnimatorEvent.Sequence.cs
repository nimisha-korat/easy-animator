

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator
{
    partial struct EasyAnimatorEvent
    {
       
        public partial class Sequence : IEnumerable<EasyAnimatorEvent>
        {
             
            #region Fields and Properties
             

            internal const string
                IndexOutOfRangeError = "index must be within the range of 0 <= index < " + nameof(Count);

#if UNITY_ASSERTIONS
            private const string
                NullCallbackError = nameof(EasyAnimatorEvent) + " callbacks can't be null (except for End Events)." +
                " The " + nameof(EasyAnimatorEvent) + "." + nameof(DummyCallback) + " can be assigned to make an event do nothing.";
#endif

             

           
            private EasyAnimatorEvent[] _Events;

             

            public int Count { get; private set; }

             

            public bool IsEmpty
            {
                get
                {
                    return
                        endEvent.callback == null &&
                        float.IsNaN(endEvent.normalizedTime) &&
                        Count == 0;
                }
            }

             

            public const int DefaultCapacity = 8;

          
            public int Capacity
            {
                get => _Events.Length;
                set
                {
                    if (value < Count)
                        throw new ArgumentOutOfRangeException(nameof(value),
                            $"{nameof(Capacity)} cannot be set lower than {nameof(Count)}");

                    if (value == _Events.Length)
                        return;

                    if (value > 0)
                    {
                        var newEvents = new EasyAnimatorEvent[value];
                        if (Count > 0)
                            Array.Copy(_Events, 0, newEvents, 0, Count);
                        _Events = newEvents;
                    }
                    else
                    {
                        _Events = Array.Empty<EasyAnimatorEvent>();
                    }
                }
            }

             

         
            public int Version { get; private set; }

             
            #region End Event
             

          
            public EasyAnimatorEvent endEvent = new EasyAnimatorEvent(float.NaN, null);

             

           
            public ref Action OnEnd => ref endEvent.callback;

             

            
            public ref float NormalizedEndTime => ref endEvent.normalizedTime;

             

          
            public static float GetDefaultNormalizedStartTime(float speed) => speed < 0 ? 1 : 0;

           
            public static float GetDefaultNormalizedEndTime(float speed) => speed < 0 ? 0 : 1;

             
            #endregion
             
            #region Names
             

            private string[] _Names;
            public ref string[] Names => ref _Names;

             

            public string GetName(int index)
            {
                if (_Names == null ||
                    _Names.Length <= index)
                    return null;
                else
                    return _Names[index];
            }

             

            public void SetName(int index, string name)
            {
                EasyAnimatorUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);

                // Capacity can't be 0 at this point.

                if (_Names == null)
                {
                    _Names = new string[Capacity];
                }
                else if (_Names.Length <= index)
                {
                    var names = new string[Capacity];
                    Array.Copy(_Names, names, _Names.Length);
                    _Names = names;
                }

                _Names[index] = name;
            }

             

            public int IndexOf(string name, int startIndex = 0)
            {
                if (_Names == null)
                    return -1;

                var count = Mathf.Min(Count, _Names.Length);
                for (; startIndex < count; startIndex++)
                    if (_Names[startIndex] == name)
                        return startIndex;

                return -1;
            }

            public int IndexOfRequired(string name, int startIndex = 0)
            {
                startIndex = IndexOf(name, startIndex);
                if (startIndex >= 0)
                    return startIndex;

                throw new ArgumentException($"No event exists with the name '{name}'.");
            }

             
            #endregion
             
            #endregion
             
            #region Constructors
             

            public Sequence()
            {
                _Events = Array.Empty<EasyAnimatorEvent>();
            }

             

            
            public Sequence(int capacity)
            {
                _Events = capacity > 0 ? new EasyAnimatorEvent[capacity] : Array.Empty<EasyAnimatorEvent>();
            }

             

            public Sequence(Sequence copyFrom)
            {
                _Events = Array.Empty<EasyAnimatorEvent>();
                if (copyFrom != null)
                    CopyFrom(copyFrom);
            }

             
            #endregion
             
            #region Iteration
             

            public EasyAnimatorEvent this[int index]
            {
                get
                {
                    EasyAnimatorUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                    return _Events[index];
                }
            }

            public EasyAnimatorEvent this[string name] => this[IndexOfRequired(name)];

             

           
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(EasyAnimatorState state)
            {
                if (Count == 0 ||
                    (_Events[0].normalizedTime >= 0 && _Events[Count - 1].normalizedTime < 1))
                    return;

                throw new ArgumentOutOfRangeException(nameof(normalizedTime),
                    "Events on looping animations are triggered every loop and must be" +
                    $" within the range of 0 <= {nameof(normalizedTime)} < 1.\n{state}\n{DeepToString()}");
            }

           
            [System.Diagnostics.Conditional(Strings.Assertions)]
            public void AssertNormalizedTimes(EasyAnimatorState state, bool isLooping)
            {
                if (isLooping)
                    AssertNormalizedTimes(state);
            }

             

            public string DeepToString(bool multiLine = true)
            {
                var text = ObjectPool.AcquireStringBuilder()
                    .Append(ToString())
                    .Append('[')
                    .Append(Count)
                    .Append(']');

                text.Append(multiLine ? "\n{" : " {");

                for (int i = 0; i < Count; i++)
                {
                    if (multiLine)
                        text.Append("\n   ");
                    else if (i > 0)
                        text.Append(',');

                    text.Append(" [");

                    text.Append(i)
                        .Append("] ");

                    this[i].AppendDetails(text);

                    var name = GetName(i);
                    if (name != null)
                    {
                        text.Append(", Name: '")
                            .Append(name)
                            .Append('\'');
                    }
                }

                if (multiLine)
                {
                    text.Append("\n    [End] ");
                }
                else
                {
                    if (Count > 0)
                        text.Append(',');
                    text.Append(" [End] ");
                }
                endEvent.AppendDetails(text);

                if (multiLine)
                    text.Append("\n}\n");
                else
                    text.Append(" } )");

                return text.ReleaseToString();
            }

             

           
            public FastEnumerator<EasyAnimatorEvent> GetEnumerator()
                => new FastEnumerator<EasyAnimatorEvent>(_Events, Count);

            IEnumerator<EasyAnimatorEvent> IEnumerable<EasyAnimatorEvent>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

             

            public int IndexOf(EasyAnimatorEvent EasyAnimatorEvent) => IndexOf(Count / 2, EasyAnimatorEvent);

            public int IndexOfRequired(EasyAnimatorEvent EasyAnimatorEvent) => IndexOfRequired(Count / 2, EasyAnimatorEvent);

            public int IndexOf(int indexHint, EasyAnimatorEvent EasyAnimatorEvent)
            {
                if (Count == 0)
                    return -1;

                if (indexHint >= Count)
                    indexHint = Count - 1;

                var otherEvent = _Events[indexHint];
                if (otherEvent == EasyAnimatorEvent)
                    return indexHint;

                if (otherEvent.normalizedTime > EasyAnimatorEvent.normalizedTime)
                {
                    while (--indexHint >= 0)
                    {
                        otherEvent = _Events[indexHint];
                        if (otherEvent.normalizedTime < EasyAnimatorEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == EasyAnimatorEvent.normalizedTime)
                            if (otherEvent.callback == EasyAnimatorEvent.callback)
                                return indexHint;
                    }
                }
                else
                {
                    while (otherEvent.normalizedTime == EasyAnimatorEvent.normalizedTime)
                    {
                        indexHint--;
                        if (indexHint < 0)
                            break;

                        otherEvent = _Events[indexHint];
                    }

                    while (++indexHint < Count)
                    {
                        otherEvent = _Events[indexHint];
                        if (otherEvent.normalizedTime > EasyAnimatorEvent.normalizedTime)
                            return -1;
                        else if (otherEvent.normalizedTime == EasyAnimatorEvent.normalizedTime)
                            if (otherEvent.callback == EasyAnimatorEvent.callback)
                                return indexHint;
                    }
                }

                return -1;
            }

            public int IndexOfRequired(int indexHint, EasyAnimatorEvent EasyAnimatorEvent)
            {
                indexHint = IndexOf(indexHint, EasyAnimatorEvent);
                if (indexHint >= 0)
                    return indexHint;

                throw new ArgumentException($"Event not found in {nameof(Sequence)} '{EasyAnimatorEvent}'.");
            }

             
            #endregion
             
            #region Modification
             

           
            public int Add(EasyAnimatorEvent EasyAnimatorEvent)
            {
#if UNITY_ASSERTIONS
                if (EasyAnimatorEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(EasyAnimatorEvent)}.{nameof(callback)}", NullCallbackError);

#endif
                var index = Insert(EasyAnimatorEvent.normalizedTime);
                AssertEventUniqueness(index, EasyAnimatorEvent);
                _Events[index] = EasyAnimatorEvent;
                return index;
            }

         
            public int Add(float normalizedTime, Action callback)
                => Add(new EasyAnimatorEvent(normalizedTime, callback));

           
            public int Add(int indexHint, EasyAnimatorEvent EasyAnimatorEvent)
            {
#if UNITY_ASSERTIONS
                if (EasyAnimatorEvent.callback == null)
                    throw new ArgumentNullException($"{nameof(EasyAnimatorEvent)}.{nameof(callback)}", NullCallbackError);

#endif
                indexHint = Insert(indexHint, EasyAnimatorEvent.normalizedTime);
                AssertEventUniqueness(indexHint, EasyAnimatorEvent);
                _Events[indexHint] = EasyAnimatorEvent;
                return indexHint;
            }

           
            public int Add(int indexHint, float normalizedTime, Action callback)
                => Add(indexHint, new EasyAnimatorEvent(normalizedTime, callback));

             

           
            public void AddRange(IEnumerable<EasyAnimatorEvent> enumerable)
            {
                foreach (var item in enumerable)
                    Add(item);
            }

             

           
            public void AddCallback(int index, Action callback)
            {
                ref var EasyAnimatorEvent = ref _Events[index];
                AssertCallbackUniqueness(EasyAnimatorEvent.callback, callback, $"{nameof(callback)} being added");
                EasyAnimatorEvent.callback += callback;
                Version++;
            }

            
            public void AddCallback(string name, Action callback) => AddCallback(IndexOfRequired(name), callback);

             

         
            public void RemoveCallback(int index, Action callback)
            {
                ref var EasyAnimatorEvent = ref _Events[index];
                EasyAnimatorEvent.callback -= callback;
                if (EasyAnimatorEvent.callback == null)
                    EasyAnimatorEvent.callback = DummyCallback;
                Version++;
            }

            public void RemoveCallback(string name, Action callback) => RemoveCallback(IndexOfRequired(name), callback);

             

           
            public void SetCallback(int index, Action callback)
            {
#if UNITY_ASSERTIONS
                if (callback == null)
                    throw new ArgumentNullException(nameof(callback), NullCallbackError);
#endif

                ref var EasyAnimatorEvent = ref _Events[index];
                AssertCallbackUniqueness(EasyAnimatorEvent.callback, callback, $"{nameof(callback)} being assigned");
                EasyAnimatorEvent.callback = callback;
                Version++;
            }

           
            public void SetCallback(string name, Action callback) => SetCallback(IndexOfRequired(name), callback);

             

          
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private static void AssertCallbackUniqueness(Action oldCallback, Action newCallback, string target)
            {
#if UNITY_ASSERTIONS
                if (oldCallback == DummyCallback ||
                    OptionalWarning.DuplicateEvent.IsDisabled())
                    return;

                if (oldCallback == newCallback)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence" +
                        " which may mean that it is being unintentionally added multiple times." +
                        $" If the {nameof(EasyAnimatorEvent)}.{nameof(Sequence)} is owned by a Transition then it will not" +
                        " be cleared each time it is played so the events should only be initialized once on startup." +
                        $" See the documentation for more information: {Strings.DocsURLs.ClearAutomatically}");
                }
                else if (oldCallback?.Method == newCallback?.Method)
                {
                    OptionalWarning.DuplicateEvent.Log($"The {target}" +
                        " is identical to an existing event in the sequence except for the target object." +
                        " This often happens when a Transition is shared by multiple objects," +
                        " in which case it can be avoided by giving each object its own" +
                        $" {nameof(EasyAnimatorEvent)}.{nameof(Sequence)} as explained in the documentation:" +
                        $" {Strings.DocsURLs.SharedEventSequences}");
                }
#endif
            }

          
            [System.Diagnostics.Conditional(Strings.Assertions)]
            private void AssertEventUniqueness(int index, EasyAnimatorEvent newEvent)
            {
#if UNITY_ASSERTIONS
                if (OptionalWarning.DuplicateEvent.IsDisabled() || index == 0)
                    return;

                var previousEvent = _Events[index - 1];
                if (previousEvent.normalizedTime != newEvent.normalizedTime)
                    return;

                AssertCallbackUniqueness(previousEvent.callback, newEvent.callback, $"{nameof(EasyAnimatorEvent)} being added");
#endif
            }

             

           
            public int SetNormalizedTime(int index, float normalizedTime)
            {
#if UNITY_ASSERTIONS
                if (!normalizedTime.IsFinite())
                    throw new ArgumentOutOfRangeException(nameof(normalizedTime), normalizedTime, $"{nameof(normalizedTime)} must be finite");
#endif

                var EasyAnimatorEvent = _Events[index];
                if (EasyAnimatorEvent.normalizedTime == normalizedTime)
                    return index;

                var moveTo = index;
                if (EasyAnimatorEvent.normalizedTime < normalizedTime)
                {
                    while (moveTo < Count - 1)
                    {
                        if (_Events[moveTo + 1].normalizedTime >= normalizedTime)
                            break;
                        else
                            moveTo++;
                    }
                }
                else
                {
                    while (moveTo > 0)
                    {
                        if (_Events[moveTo - 1].normalizedTime <= normalizedTime)
                            break;
                        else
                            moveTo--;
                    }
                }

                if (index != moveTo)
                {
                    var name = GetName(index);
                    Remove(index);

                    index = moveTo;

                    Insert(index);
                    if (!string.IsNullOrEmpty(name))
                        SetName(index, name);
                }

                EasyAnimatorEvent.normalizedTime = normalizedTime;
                _Events[index] = EasyAnimatorEvent;

                Version++;

                return index;
            }

            
            public int SetNormalizedTime(string name, float normalizedTime)
                => SetNormalizedTime(IndexOfRequired(name), normalizedTime);

          
            public int SetNormalizedTime(EasyAnimatorEvent EasyAnimatorEvent, float normalizedTime)
                => SetNormalizedTime(IndexOfRequired(EasyAnimatorEvent), normalizedTime);

             

          
            private int Insert(float normalizedTime)
            {
                var index = Count;
                while (index > 0 && _Events[index - 1].normalizedTime > normalizedTime)
                    index--;
                Insert(index);
                return index;
            }

          
            private int Insert(int indexHint, float normalizedTime)
            {
                if (Count == 0)
                {
                    Count = 0;
                }
                else
                {
                    if (indexHint >= Count)
                        indexHint = Count - 1;

                    if (_Events[indexHint].normalizedTime > normalizedTime)
                    {
                        while (indexHint > 0 && _Events[indexHint - 1].normalizedTime > normalizedTime)
                            indexHint--;
                    }
                    else
                    {
                        while (indexHint < Count && _Events[indexHint].normalizedTime <= normalizedTime)
                            indexHint++;
                    }
                }

                Insert(indexHint);
                return indexHint;
            }

             

          
            private void Insert(int index)
            {
                EasyAnimatorUtilities.Assert((uint)index <= (uint)Count, IndexOutOfRangeError);

                var capacity = _Events.Length;
                if (Count == capacity)
                {
                    if (capacity == 0)
                    {
                        capacity = DefaultCapacity;
                        _Events = new EasyAnimatorEvent[DefaultCapacity];
                    }
                    else
                    {
                        capacity *= 2;
                        if (capacity < DefaultCapacity)
                            capacity = DefaultCapacity;

                        var events = new EasyAnimatorEvent[capacity];

                        Array.Copy(_Events, 0, events, 0, index);
                        if (Count > index)
                            Array.Copy(_Events, index, events, index + 1, Count - index);

                        _Events = events;
                    }
                }
                else if (Count > index)
                {
                    Array.Copy(_Events, index, _Events, index + 1, Count - index);
                }

                if (_Names != null)
                {
                    if (_Names.Length < capacity)
                    {
                        var names = new string[capacity];

                        Array.Copy(_Names, 0, names, 0, Math.Min(_Names.Length, index));
                        if (Count > index)
                            Array.Copy(_Names, index, names, index + 1, Count - index);

                        _Names = names;
                    }
                    else
                    {
                        if (Count > index)
                            Array.Copy(_Names, index, _Names, index + 1, Count - index);

                        _Names[index] = null;
                    }
                }

                Count++;
                Version++;
            }

             

          
            public void Remove(int index)
            {
                EasyAnimatorUtilities.Assert((uint)index < (uint)Count, IndexOutOfRangeError);
                Count--;
                if (index < Count)
                {
                    Array.Copy(_Events, index + 1, _Events, index, Count - index);

                    if (_Names != null)
                    {
                        var nameCount = Mathf.Min(Count + 1, _Names.Length);
                        if (index + 1 < nameCount)
                            Array.Copy(_Names, index + 1, _Names, index, nameCount - index - 1);

                        _Names[nameCount - 1] = default;
                    }
                }
                else if (_Names != null && index < _Names.Length)
                {
                    _Names[index] = default;
                }

                _Events[Count] = default;
                Version++;
            }

           
            public bool Remove(string name)
            {
                var index = IndexOf(name);
                if (index >= 0)
                {
                    Remove(index);
                    return true;
                }
                else return false;
            }

           
            public bool Remove(EasyAnimatorEvent EasyAnimatorEvent)
            {
                var index = IndexOf(EasyAnimatorEvent);
                if (index >= 0)
                {
                    Remove(index);
                    return true;
                }
                else return false;
            }

             

            public void Clear()
            {
                if (_Names != null)
                    Array.Clear(_Names, 0, _Names.Length);

                Array.Clear(_Events, 0, Count);
                Count = 0;
                Version++;

                endEvent = new EasyAnimatorEvent(float.NaN, null);
            }

             
            #endregion
             
            #region Copying
             

            public void CopyFrom(Sequence source)
            {
                if (source == null)
                {
                    if (_Names != null)
                        Array.Clear(_Names, 0, _Names.Length);

                    Array.Clear(_Events, 0, Count);
                    Count = 0;
                    Capacity = 0;
                    endEvent = default;
                    return;
                }

                if (source._Names == null)
                {
                    _Names = null;
                }
                else
                {
                    var nameCount = source._Names.Length;
                    EasyAnimatorUtilities.SetLength(ref _Names, nameCount);
                    Array.Copy(source._Names, 0, _Names, 0, nameCount);
                }

                var sourceCount = source.Count;

                if (Count > sourceCount)
                    Array.Clear(_Events, Count, sourceCount - Count);
                else if (_Events.Length < sourceCount)
                    Capacity = sourceCount;

                Count = sourceCount;

                Array.Copy(source._Events, 0, _Events, 0, sourceCount);

                endEvent = source.endEvent;
            }

             

      
            public void CopyTo(EasyAnimatorEvent[] array, int index)
            {
                Array.Copy(_Events, 0, array, index, Count);
            }

             
            #endregion
             
        }
    }
}

