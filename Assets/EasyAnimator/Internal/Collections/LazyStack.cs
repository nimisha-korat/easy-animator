

using System.Collections.Generic;

namespace EasyAnimator
{
    
    public sealed class LazyStack<T> where T : new()
    {
         

      
        private readonly List<T> Stack;

        private int _CurrentIndex = -1;

        public T Current { get; private set; }

         

        public LazyStack()
        {
            Stack = new List<T>();
        }

        public LazyStack(int capacity)
        {
            Stack = new List<T>(capacity);
            for (int i = 0; i < capacity; i++)
                Stack[i] = new T();
        }

         

        public T Increment()
        {
            _CurrentIndex++;
            if (_CurrentIndex == Stack.Count)
            {
                Current = new T();
                Stack.Add(Current);
            }
            else
            {
                Current = Stack[_CurrentIndex];
            }

            return Current;
        }

         

        public void Decrement()
        {
            _CurrentIndex--;
            if (_CurrentIndex >= 0)
                Current = Stack[_CurrentIndex];
            else
                Current = default;
        }

         
    }
}

