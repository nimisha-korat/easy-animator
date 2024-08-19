
#if UNITY_EDITOR



using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyAnimator.Editor
{
   
    public sealed class ConversionCache<TKey, TValue>
    {
         

        private sealed class CachedValue
        {
            public int lastFrameAccessed;
            public TValue value;
        }

         

        private readonly Dictionary<TKey, CachedValue>
            Cache = new Dictionary<TKey, CachedValue>();
        private readonly List<TKey>
            Keys = new List<TKey>();
        private readonly Func<TKey, TValue>
            Converter;

        private int _LastCleanupFrame;

         

        public ConversionCache(Func<TKey, TValue> converter) => Converter = converter;

         

      
        public TValue Convert(TKey key)
        {
            if (key == null)
                return default;

            CachedValue cached;

            // The next time a value is retrieved after at least 100 frames, clear out any old ones.
            var frame = Time.frameCount;
            if (_LastCleanupFrame + 100 < frame)
            {

                for (int i = Keys.Count - 1; i >= 0; i--)
                {
                    var checkKey = Keys[i];
                    if (!Cache.TryGetValue(checkKey, out cached) ||
                        cached.lastFrameAccessed <= _LastCleanupFrame)
                    {
                        Cache.Remove(checkKey);
                        Keys.RemoveAt(i);
                    }
                }

                _LastCleanupFrame = frame;

            }

            if (!Cache.TryGetValue(key, out cached))
            {
                Cache.Add(key, cached = new CachedValue { value = Converter(key) });
                Keys.Add(key);

            }

            cached.lastFrameAccessed = frame;

            return cached.value;
        }

         
    }
}

#endif

