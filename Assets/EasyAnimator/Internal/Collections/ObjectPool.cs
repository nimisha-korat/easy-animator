

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyAnimator
{
    
    public static class ObjectPool
    {
         

        public static T Acquire<T>()
            where T : class, new()
            => ObjectPool<T>.Acquire();

        public static void Acquire<T>(out T item)
            where T : class, new()
            => item = ObjectPool<T>.Acquire();

         
        public static void Release<T>(T item)
            where T : class, new()
            => ObjectPool<T>.Release(item);

        public static void Release<T>(ref T item) where T : class, new()
        {
            ObjectPool<T>.Release(item);
            item = null;
        }

         

        public const string
            NotClearError = " They must be cleared before being released to the pool and not modified after that.";

         

        public static List<T> AcquireList<T>()
        {
            var list = ObjectPool<List<T>>.Acquire();
            EasyAnimatorUtilities.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
            return list;
        }

        public static void Acquire<T>(out List<T> list)
            => list = AcquireList<T>();

        public static void Release<T>(List<T> list)
        {
            list.Clear();
            ObjectPool<List<T>>.Release(list);
        }

        public static void Release<T>(ref List<T> list)
        {
            Release(list);
            list = null;
        }

         

        public static HashSet<T> AcquireSet<T>()
        {
            var set = ObjectPool<HashSet<T>>.Acquire();
            EasyAnimatorUtilities.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
            return set;
        }

        public static void Acquire<T>(out HashSet<T> set)
            => set = AcquireSet<T>();

        public static void Release<T>(HashSet<T> set)
        {
            set.Clear();
            ObjectPool<HashSet<T>>.Release(set);
        }

        public static void Release<T>(ref HashSet<T> set)
        {
            Release(set);
            set = null;
        }

         

        public static StringBuilder AcquireStringBuilder()
        {
            var builder = ObjectPool<StringBuilder>.Acquire();
            EasyAnimatorUtilities.Assert(builder.Length == 0, $"A pooled {nameof(StringBuilder)} is not empty." + NotClearError);
            return builder;
        }

        public static void Release(StringBuilder builder)
        {
            builder.Length = 0;
            ObjectPool<StringBuilder>.Release(builder);
        }

        public static string ReleaseToString(this StringBuilder builder)
        {
            var result = builder.ToString();
            Release(builder);
            return result;
        }

         

        public static class Disposable
        {
             

            
            public static ObjectPool<T>.Disposable Acquire<T>(out T item)
                where T : class, new()
                => new ObjectPool<T>.Disposable(out item);

             

           
            public static ObjectPool<List<T>>.Disposable AcquireList<T>(out List<T> list)
            {
                var disposable = new ObjectPool<List<T>>.Disposable(out list, onRelease: (l) => l.Clear());
                EasyAnimatorUtilities.Assert(list.Count == 0, "A pooled list is not empty." + NotClearError);
                return disposable;
            }

             

           
            public static ObjectPool<HashSet<T>>.Disposable AcquireSet<T>(out HashSet<T> set)
            {
                var disposable = new ObjectPool<HashSet<T>>.Disposable(out set, onRelease: (s) => s.Clear());
                EasyAnimatorUtilities.Assert(set.Count == 0, "A pooled set is not empty." + NotClearError);
                return disposable;
            }

             

           
            public static ObjectPool<GUIContent>.Disposable AcquireContent(out GUIContent content,
                string text = null, string tooltip = null, bool narrowText = true)
            {
                var disposable = new ObjectPool<GUIContent>.Disposable(out content, onRelease: (c) =>
                {
                    c.text = null;
                    c.tooltip = null;
                    c.image = null;
                });

#if UNITY_ASSERTIONS
                if (!string.IsNullOrEmpty(content.text) ||
                    !string.IsNullOrEmpty(content.tooltip) ||
                    content.image != null)
                {
                    throw new UnityEngine.Assertions.AssertionException(
                        $"A pooled {nameof(GUIContent)} is not cleared." + NotClearError,
                        $"- {nameof(content.text)} = '{content.text}'" +
                        $"\n- {nameof(content.tooltip)} = '{content.tooltip}'" +
                        $"\n- {nameof(content.image)} = '{content.image}'");
                }
#endif

                content.text = text;
                content.tooltip = tooltip;
                content.image = null;
                return disposable;
            }

             

#if UNITY_EDITOR
           
            public static ObjectPool<GUIContent>.Disposable AcquireContent(out GUIContent content,
                UnityEditor.SerializedProperty property, bool narrowText = true)
                => AcquireContent(out content, property.displayName, property.tooltip, narrowText);
#endif

             
        }

         
    }

     

   
    public static class ObjectPool<T> where T : class, new()
    {
         

        private static readonly List<T>
            Items = new List<T>();

         

        public static int Count
        {
            get => Items.Count;
            set
            {
                var count = Items.Count;
                if (count < value)
                {
                    if (Items.Capacity < value)
                        Items.Capacity = Mathf.NextPowerOfTwo(value);

                    do
                    {
                        Items.Add(new T());
                        count++;
                    }
                    while (count < value);

                }
                else if (count > value)
                {
                    Items.RemoveRange(value, count - value);
                }
            }
        }

         

        public static void IncreaseCountTo(int count)
        {
            if (Count < count)
                Count = count;
        }

         

        public static int Capacity
        {
            get => Items.Capacity;
            set
            {
                if (Items.Count > value)
                    Items.RemoveRange(value, Items.Count - value);
                Items.Capacity = value;
            }
        }

         

        public static void IncreaseCapacityTo(int capacity)
        {
            if (Capacity < capacity)
                Capacity = capacity;
        }

         

       
        public static T Acquire()
        {
            var count = Items.Count;
            if (count == 0)
            {
                return new T();
            }
            else
            {
                count--;
                var item = Items[count];
                Items.RemoveAt(count);

                return item;
            }
        }

         

        public static void Release(T item)
        {
            EasyAnimatorUtilities.Assert(item != null,
                $"Null objects must not be released into an {nameof(ObjectPool<T>)}.");

            Items.Add(item);

        }

         

        public static string GetDetails()
        {
            return
                $"{typeof(T).Name}" +
                $" ({nameof(Count)} = {Items.Count}" +
                $", {nameof(Capacity)} = {Items.Capacity}" +
                ")";
        }

         

     
        public readonly struct Disposable : IDisposable
        {
             

            public readonly T Item;

            public readonly Action<T> OnRelease;

             

        
            public Disposable(out T item, Action<T> onRelease = null)
            {
                Item = item = Acquire();
                OnRelease = onRelease;
            }

             

            void IDisposable.Dispose()
            {
                OnRelease?.Invoke(Item);
                Release(Item);
            }

             
        }

         
    }
}

