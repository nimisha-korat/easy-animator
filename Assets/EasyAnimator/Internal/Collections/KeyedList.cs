

using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyAnimator
{
    
    public class Key : Key.IListItem
    {
         

       
        public interface IListItem
        {
            Key Key { get; }
        }

         

        public const int NotInList = -1;

        private int _Index = -1;

        public static int IndexOf(Key key) => key._Index;

        public static bool IsInList(Key key) => key._Index != NotInList;

         

        Key IListItem.Key => this;

         

        
        public sealed class KeyedList<T> : IList<T>, ICollection where T : class, IListItem
        {
             

            private const string
                SingleUse = "Each item can only be used in one " + nameof(KeyedList<T>) + " at a time.",
                NotFound = "The specified item does not exist in this " + nameof(KeyedList<T>) + ".";

             

            private readonly List<T> Items;

             

            public KeyedList() => Items = new List<T>();

            public KeyedList(int capacity) => Items = new List<T>(capacity);


             

            public int Count => Items.Count;

            public int Capacity
            {
                get => Items.Capacity;
                set => Items.Capacity = value;
            }

             

            public T this[int index]
            {
                get => Items[index];
                set
                {
                    var key = value.Key;

                    // Make sure it isn't already in a list.
                    if (key._Index != NotInList)
                        throw new ArgumentException(SingleUse);

                    // Remove the old item at that index.
                    Items[index].Key._Index = NotInList;

                    // Set the index of the new item and add it at that index.
                    key._Index = index;
                    Items[index] = value;
                }
            }

             

            public bool Contains(T item)
            {
                if (item == null)
                    return false;

                var index = item.Key._Index;
                return
                    (uint)index < (uint)Items.Count &&
                    Items[index] == item;
            }

             

            public int IndexOf(T item)
            {
                if (item == null)
                    return NotInList;

                var index = item.Key._Index;
                if ((uint)index < (uint)Items.Count &&
                    Items[index] == item)
                    return index;
                else
                    return NotInList;
            }

             

            public void Add(T item)
            {
                var key = item.Key;

                // Make sure it isn't already in a list.
                if (key._Index != NotInList)
                    throw new ArgumentException(SingleUse);

                // Set the index of the new item and add it to the list.
                key._Index = Items.Count;
                Items.Add(item);
            }

            public void AddNew(T item)
            {
                if (!Contains(item))
                    Add(item);
            }

             

            public void Insert(int index, T item)
            {
                for (int i = index; i < Items.Count; i++)
                    Items[i].Key._Index++;

                item.Key._Index = index;
                Items.Insert(index, item);
            }

             

            public void RemoveAt(int index)
            {
                // Adjust the indices of all items after the target.
                for (int i = index + 1; i < Items.Count; i++)
                    Items[i].Key._Index--;

                // Mark the key as removed and remove the item.
                Items[index].Key._Index = NotInList;
                Items.RemoveAt(index);
            }

          
            public void RemoveAtSwap(int index)
            {
                // Mark the item as removed.
                Items[index].Key._Index = NotInList;

                // If it wasn't the last item, move the last item over it.
                var lastIndex = Items.Count - 1;
                if (lastIndex > index)
                {
                    var lastItem = Items[lastIndex];
                    lastItem.Key._Index = index;
                    Items[index] = lastItem;
                }

                // Remove the last item from the list.
                Items.RemoveAt(lastIndex);
            }

             

            public bool Remove(T item)
            {
                var key = item.Key;
                var index = key._Index;

                // If it isn't in a list, do nothing.
                if (index == NotInList)
                    return false;

                // Make sure the item is actually in this list at the index it says.
                // Otherwise it must be in a different list.
                if (Items[index] != item)
                    throw new ArgumentException(NotFound, nameof(item));

                // Remove the item.
                RemoveAt(index);
                return true;
            }

             

          
            public bool RemoveSwap(T item)
            {
                var key = item.Key;
                var index = key._Index;

                // If it isn't in a list, do nothing.
                if (index == NotInList)
                    return false;

                // Make sure the item is actually in this list at the index it says.
                // Otherwise it must be in a different list.
                if (Items[index] != item)
                    throw new ArgumentException(NotFound, nameof(item));

                // Remove the item.
                RemoveAtSwap(index);
                return true;
            }

             

            public void Clear()
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                    Items[i].Key._Index = NotInList;

                Items.Clear();
            }

             

            public void CopyTo(T[] array, int index) => Items.CopyTo(array, index);

            void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

            bool ICollection<T>.IsReadOnly => false;

            public List<T>.Enumerator GetEnumerator() => Items.GetEnumerator();

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

             

            bool ICollection.IsSynchronized => ((ICollection)Items).IsSynchronized;

            object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

             
        }

         
    }
}

