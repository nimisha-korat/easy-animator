
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator.FSM
{
    
    public interface IKeyedStateMachine<TKey>
    {
         

        TKey CurrentKey { get; }

        TKey PreviousKey { get; }

        TKey NextKey { get; }

        object TrySetState(TKey key);

        object TryResetState(TKey key);

        object ForceSetState(TKey key);

         
    }

   
    [HelpURL(StateExtensions.APIDocumentationURL + nameof(StateMachine<TState>) + "_2")]
    public partial class StateMachine<TKey, TState> : StateMachine<TState>, IKeyedStateMachine<TKey>, IDictionary<TKey, TState>
        where TState : class, IState
    {
         

        public IDictionary<TKey, TState> Dictionary { get; set; }

        public TKey CurrentKey { get; private set; }

         

        public TKey PreviousKey => KeyChange<TKey>.PreviousKey;

        public TKey NextKey => KeyChange<TKey>.NextKey;

         

        public StateMachine()
        {
            Dictionary = new Dictionary<TKey, TState>();
        }

        public StateMachine(IDictionary<TKey, TState> dictionary)
        {
            Dictionary = dictionary;
        }

        public StateMachine(TKey defaultKey, TState defaultState)
        {
            Dictionary = new Dictionary<TKey, TState>
            {
                { defaultKey, defaultState }
            };
            ForceSetState(defaultKey, defaultState);
        }

        public StateMachine(IDictionary<TKey, TState> dictionary, TKey defaultKey, TState defaultState)
        {
            Dictionary = dictionary;
            dictionary.Add(defaultKey, defaultState);
            ForceSetState(defaultKey, defaultState);
        }

         

        public bool TrySetState(TKey key, TState state)
        {
            if (CurrentState == state)
                return true;
            else
                return TryResetState(key, state);
        }

        public TState TrySetState(TKey key)
        {
            if (EqualityComparer<TKey>.Default.Equals(CurrentKey, key))
                return CurrentState;
            else
                return TryResetState(key);
        }

        object IKeyedStateMachine<TKey>.TrySetState(TKey key) => TrySetState(key);

         

        public bool TryResetState(TKey key, TState state)
        {
            using (new KeyChange<TKey>(this, CurrentKey, key))
            {
                if (!CanSetState(state))
                    return false;
                
                CurrentKey = key;
                ForceSetState(state);
                return true;
            }
        }

        public TState TryResetState(TKey key)
        {
            if (Dictionary.TryGetValue(key, out var state) &&
                TryResetState(key, state))
                return state;
            else
                return null;
        }

        object IKeyedStateMachine<TKey>.TryResetState(TKey key) => TryResetState(key);

         

        public void ForceSetState(TKey key, TState state)
        {
            using (new KeyChange<TKey>(this, CurrentKey, key))
            {
                CurrentKey = key;
                ForceSetState(state);
            }
        }

        public TState ForceSetState(TKey key)
        {
            Dictionary.TryGetValue(key, out var state);
            ForceSetState(key, state);
            return state;
        }

        object IKeyedStateMachine<TKey>.ForceSetState(TKey key) => ForceSetState(key);

         
        #region Dictionary Wrappers
         

        public TState this[TKey key] { get => Dictionary[key]; set => Dictionary[key] = value; }

        public bool TryGetValue(TKey key, out TState state) => Dictionary.TryGetValue(key, out state);

         

        public ICollection<TKey> Keys => Dictionary.Keys;

        public ICollection<TState> Values => Dictionary.Values;

         

        public int Count => Dictionary.Count;

         

        public void Add(TKey key, TState state) => Dictionary.Add(key, state);

        public void Add(KeyValuePair<TKey, TState> item) => Dictionary.Add(item);

         

        public bool Remove(TKey key) => Dictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TState> item) => Dictionary.Remove(item);

         

        public void Clear() => Dictionary.Clear();

         

        public bool Contains(KeyValuePair<TKey, TState> item) => Dictionary.Contains(item);

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

         

        public IEnumerator<KeyValuePair<TKey, TState>> GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

         

        public void CopyTo(KeyValuePair<TKey, TState>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

         

        bool ICollection<KeyValuePair<TKey, TState>>.IsReadOnly => Dictionary.IsReadOnly;

         
        #endregion
         

        public TState GetState(TKey key)
        {
            TryGetValue(key, out var state);
            return state;
        }

         

        public void AddRange(TKey[] keys, TState[] states)
        {
            Debug.Assert(keys.Length == states.Length,
                $"The '{nameof(keys)}' and '{nameof(states)}' arrays must be the same size.");

            for (int i = 0; i < keys.Length; i++)
            {
                Dictionary.Add(keys[i], states[i]);
            }
        }

         

        public void SetFakeKey(TKey key) => CurrentKey = key;

         

        public override string ToString()
            => $"{GetType().FullName} -> {CurrentKey} -> {(CurrentState != null ? CurrentState.ToString() : "null")}";

         
    }
}
