

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

namespace EasyAnimator
{
   
    public interface IAnimationClipCollection
    {

        void GatherAnimationClips(ICollection<AnimationClip> clips);

    }


    public static partial class EasyAnimatorUtilities
    {
       
        public static void Gather(this ICollection<AnimationClip> clips, AnimationClip clip)
        {
            if (clip != null && !clips.Contains(clip))
                clips.Add(clip);
        }

      
        public static void Gather(this ICollection<AnimationClip> clips, IList<AnimationClip> gatherFrom)
        {
            if (gatherFrom == null)
                return;

            for (int i = gatherFrom.Count - 1; i >= 0; i--)
                clips.Gather(gatherFrom[i]);
        }

       
        public static void Gather(this ICollection<AnimationClip> clips, IEnumerable<AnimationClip> gatherFrom)
        {
            if (gatherFrom == null)
                return;

            foreach (var clip in gatherFrom)
                clips.Gather(clip);
        }

       
        public static void GatherFromAsset(this ICollection<AnimationClip> clips, PlayableAsset asset)
        {
            if (asset == null)
                return;


            var method = asset.GetType().GetMethod("GetRootTracks");
            if (method != null &&
                typeof(IEnumerable).IsAssignableFrom(method.ReturnType) &&
                method.GetParameters().Length == 0)
            {
                var rootTracks = method.Invoke(asset, null);
                GatherFromTracks(clips, rootTracks as IEnumerable);
            }
        }

       
        private static void GatherFromTracks(ICollection<AnimationClip> clips, IEnumerable tracks)
        {
            if (tracks == null)
                return;

            foreach (var track in tracks)
            {
                if (track == null)
                    continue;

                var trackType = track.GetType();

                var getClips = trackType.GetMethod("GetClips");
                if (getClips != null &&
                    typeof(IEnumerable).IsAssignableFrom(getClips.ReturnType) &&
                    getClips.GetParameters().Length == 0)
                {
                    var trackClips = getClips.Invoke(track, null) as IEnumerable;
                    if (trackClips != null)
                    {
                        foreach (var clip in trackClips)
                        {
                            var animationClip = clip.GetType().GetProperty("animationClip");
                            if (animationClip != null &&
                                animationClip.PropertyType == typeof(AnimationClip))
                            {
                                var getClip = animationClip.GetGetMethod();
                                clips.Gather(getClip.Invoke(clip, null) as AnimationClip);
                            }
                        }
                    }
                }

                var getChildTracks = trackType.GetMethod("GetChildTracks");
                if (getChildTracks != null &&
                    typeof(IEnumerable).IsAssignableFrom(getChildTracks.ReturnType) &&
                    getChildTracks.GetParameters().Length == 0)
                {
                    var childTracks = getChildTracks.Invoke(track, null);
                    GatherFromTracks(clips, childTracks as IEnumerable);
                }
            }
        }

       
        public static void GatherFromSource(this ICollection<AnimationClip> clips, IAnimationClipSource source)
        {
            if (source == null)
                return;

            var list = ObjectPool.AcquireList<AnimationClip>();
            source.GetAnimationClips(list);
            clips.Gather(list);
            ObjectPool.Release(list);
        }

       
        public static void GatherFromSource(this ICollection<AnimationClip> clips, IEnumerable source)
        {
            if (source != null)
                foreach (var item in source)
                    clips.GatherFromSource(item);
        }

       
        public static bool GatherFromSource(this ICollection<AnimationClip> clips, object source)
        {
            if (TryGetWrappedObject(source, out AnimationClip clip))
            {
                clips.Gather(clip);
                return true;
            }

            if (TryGetWrappedObject(source, out IAnimationClipCollection collectionSource))
            {
                collectionSource.GatherAnimationClips(clips);
                return true;
            }

            if (TryGetWrappedObject(source, out IAnimationClipSource listSource))
            {
                clips.GatherFromSource(listSource);
                return true;
            }

            if (TryGetWrappedObject(source, out IEnumerable enumerable))
            {
                clips.GatherFromSource(enumerable);
                return true;
            }

            return false;
        }

       
        public static bool TryGetFrameRate(object clipSource, out float frameRate)
        {
            using (ObjectPool.Disposable.AcquireSet<AnimationClip>(out var clips))
            {
                clips.GatherFromSource(clipSource);
                if (clips.Count == 0)
                {
                    frameRate = float.NaN;
                    return false;
                }

                frameRate = float.NaN;

                foreach (var clip in clips)
                {
                    if (float.IsNaN(frameRate))
                    {
                        frameRate = clip.frameRate;
                    }
                    else if (frameRate != clip.frameRate)
                    {
                        frameRate = float.NaN;
                        return false;
                    }
                }

                return frameRate > 0;
            }
        }

    }
}

