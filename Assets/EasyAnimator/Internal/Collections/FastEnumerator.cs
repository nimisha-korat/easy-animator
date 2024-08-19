

using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyAnimator
{
  
    public struct FastEnumerator<T> : IList<T>, IEnumerator<T>
    {
         

        private readonly IList<T> List;

         

        private int _Count;

       
        public int Count
        {
            get => _Count;
            set
            {
                AssertCount(value);
                _Count = value;
            }
        }

         

        private int _Index;

        public int Index
        {
            get => _Index;
            set
            {
                AssertIndex(value);
                _Index = value;
            }
        }

         

        public T Current
        {
            get
            {
                AssertCount(_Count);
                AssertIndex(_Index);
                return List[_Index];
            }
            set
            {
                AssertCount(_Count);
                AssertIndex(_Index);
                List[_Index] = value;
            }
        }

        object IEnumerator.Current => Current;

         

       
        public FastEnumerator(IList<T> list)
            : this(list, list.Count)
        { }

      
        public FastEnumerator(IList<T> list, int count)
        {
            List = list;
            _Count = count;
            _Index = -1;
            AssertCount(count);
        }

         

        public bool MoveNext()
        {
            _Index++;
            if ((uint)_Index < (uint)_Count)
            {
                return true;
            }
            else
            {
                _Index = int.MinValue;
                return false;
            }
        }

         

        
        public bool MovePrevious()
        {
            if (_Index > 0)
            {
                _Index--;
                return true;
            }
            else
            {
                _Index = -1;
                return false;
            }
        }

         

        public void Reset()
        {
            _Index = -1;
        }

         

        void IDisposable.Dispose() { }

         
        // IEnumerator.
         

        public FastEnumerator<T> GetEnumerator() => this;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

         

        public int IndexOf(T item) => List.IndexOf(item);

        public T this[int index]
        {
            get
            {
                AssertIndex(index);
                return List[index];
            }
            set
            {
                AssertIndex(index);
                List[index] = value;
            }
        }

        public void Insert(int index, T item)
        {
            AssertIndex(index);
            List.Insert(index, item);
            if (_Index >= index)
                _Index++;
            _Count++;
        }

        public void RemoveAt(int index)
        {
            AssertIndex(index);
            List.RemoveAt(index);
            if (_Index >= index)
                _Index--;
            _Count--;
        }

         
        // ICollection.
         

        public bool IsReadOnly => List.IsReadOnly;

        public bool Contains(T item) => List.Contains(item);

        public void Add(T item)
        {
            List.Add(item);
            _Count++;
        }

        public bool Remove(T item)
        {
            var index = List.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            else return false;
        }

        public void Clear()
        {
            List.Clear();
            _Index = -1;
            _Count = 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _Count; i++)
                array[arrayIndex + i] = List[i];
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        private void AssertIndex(int index)
        {
#if UNITY_ASSERTIONS
            if ((uint)index > (uint)_Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"{nameof(FastEnumerator<T>)}.{nameof(Index)}" +
                    $" must be within 0 <= {nameof(Index)} ({index}) < {nameof(Count)} ({_Count}).");
#endif
        }

         

        [System.Diagnostics.Conditional(Strings.Assertions)]
        private void AssertCount(int count)
        {
#if UNITY_ASSERTIONS
            if (List == null)
            {
                if (count != 0)
                    throw new ArgumentOutOfRangeException(nameof(count),
                        $"Must be within 0 since the {nameof(List)} is null.");
            }
            else
            {
                if ((uint)count > (uint)List.Count)
                    throw new ArgumentOutOfRangeException(nameof(count),
                        $"Must be within 0 <= {nameof(count)} ({count}) < {nameof(List)}.{nameof(List.Count)} ({List.Count}).");
            }
#endif
        }

         
    }
}

