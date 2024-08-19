
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Sequence = EasyAnimator.EasyAnimatorEvent.Sequence;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;

namespace EasyAnimator.Editor
{
    
    public sealed class EventSequenceDrawer
    {
         

        private static readonly ConditionalWeakTable<Sequence, EventSequenceDrawer>
            SequenceToDrawer = new ConditionalWeakTable<Sequence, EventSequenceDrawer>();

        public static EventSequenceDrawer Get(Sequence events)
        {
            if (events == null)
                return null;

            if (!SequenceToDrawer.TryGetValue(events, out var drawer))
                SequenceToDrawer.Add(events, drawer = new EventSequenceDrawer());

            return drawer;
        }

         

        public static float CalculateHeight(int lineCount)
            => lineCount == 0 ? 0 :
                EasyAnimatorGUI.LineHeight * lineCount +
                EasyAnimatorGUI.StandardSpacing * (lineCount - 1);

         

        public float CalculateHeight(Sequence events)
            => CalculateHeight(CalculateLineCount(events));

        public int CalculateLineCount(Sequence events)
        {
            if (events == null)
                return 0;

            if (!_IsExpanded)
                return 1;

            var count = 1;

            for (int i = 0; i < events.Count; i++)
            {
                count++;
                count += CalculateLineCount(events[i].callback);
            }

            count++;
            count += CalculateLineCount(events.endEvent.callback);

            return count;
        }

         

        private bool _IsExpanded;

        private static ConversionCache<int, string> _EventNumberCache;

        private static float _LogButtonWidth = float.NaN;

         

        public void Draw(ref Rect area, Sequence events, GUIContent label)
        {
            if (events == null)
                return;

            area.height = EasyAnimatorGUI.LineHeight;

            var headerArea = area;

            const string LogLabel = "Log";
            if (float.IsNaN(_LogButtonWidth))
                _LogButtonWidth = EditorStyles.miniButton.CalculateWidth(LogLabel);
            var logArea = EasyAnimatorGUI.StealFromRight(ref headerArea, _LogButtonWidth);
            if (GUI.Button(logArea, LogLabel, EditorStyles.miniButton))
                Debug.Log(events.DeepToString());

            _IsExpanded = EditorGUI.Foldout(headerArea, _IsExpanded, GUIContent.none, true);
            using (ObjectPool.Disposable.AcquireContent(out var summary, GetSummary(events)))
                EditorGUI.LabelField(headerArea, label, summary);

            EasyAnimatorGUI.NextVerticalArea(ref area);

            if (!_IsExpanded)
                return;

            var enabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUI.indentLevel++;

            for (int i = 0; i < events.Count; i++)
            {
                var name = events.GetName(i);
                if (string.IsNullOrEmpty(name))
                {
                    if (_EventNumberCache == null)
                        _EventNumberCache = new ConversionCache<int, string>((index) => $"Event {index}");

                    name = _EventNumberCache.Convert(i);
                }

                Draw(ref area, name, events[i]);
            }

            Draw(ref area, "End Event", events.endEvent);

            EditorGUI.indentLevel--;

            GUI.enabled = enabled;
        }

         

        private static readonly ConversionCache<int, string>
            SummaryCache = new ConversionCache<int, string>((count) => $"[{count}]"),
            EndSummaryCache = new ConversionCache<int, string>((count) => $"[{count}] + End");

        public static string GetSummary(Sequence events)
        {
            var cache =
                float.IsNaN(events.endEvent.normalizedTime) &&
                EasyAnimatorEvent.IsNullOrDummy(events.endEvent.callback)
                ? SummaryCache : EndSummaryCache;
            return cache.Convert(events.Count);
        }

         

        private static ConversionCache<float, string> _EventTimeCache;

        public static void Draw(ref Rect area, string name, EasyAnimatorEvent EasyAnimatorEvent)
        {
            area.height = EasyAnimatorGUI.LineHeight;

            if (_EventTimeCache == null)
                _EventTimeCache = new ConversionCache<float, string>((time)
                    => float.IsNaN(time) ? "Time = Auto" : $"Time = {time.ToStringCached()}x");

            EditorGUI.LabelField(area, name, _EventTimeCache.Convert(EasyAnimatorEvent.normalizedTime));

            EasyAnimatorGUI.NextVerticalArea(ref area);

            EditorGUI.indentLevel++;
            DrawInvocationList(ref area, EasyAnimatorEvent.callback);
            EditorGUI.indentLevel--;
        }

         

        public static float CalculateHeight(MulticastDelegate del)
            => CalculateHeight(CalculateLineCount(del));

        public static int CalculateLineCount(MulticastDelegate del)
        {
            if (del == null)
                return 1;

            var delegates = GetInvocationListIfMulticast(del);
            return delegates == null ? 2 : delegates.Length * 2;
        }

         

        public static void DrawInvocationList(ref Rect area, MulticastDelegate del)
        {
            if (del == null)
            {
                EditorGUI.LabelField(area, "Delegate", "Null");
                EasyAnimatorGUI.NextVerticalArea(ref area);
                return;
            }

            var delegates = GetInvocationListIfMulticast(del);
            if (delegates == null)
            {
                Draw(ref area, del);
            }
            else
            {
                for (int i = 0; i < delegates.Length; i++)
                    Draw(ref area, delegates[i]);
            }
        }

         

        private static Delegate[] GetInvocationListIfMulticast(MulticastDelegate del)
            => EasyAnimatorUtilities.TryGetInvocationListNonAlloc(del, out var delegates) ? delegates : del.GetInvocationList();

         

        public static void Draw(ref Rect area, Delegate del)
        {
            area.height = EasyAnimatorGUI.LineHeight;

            if (del == null)
            {
                EditorGUI.LabelField(area, "Callback", "Null");
                EasyAnimatorGUI.NextVerticalArea(ref area);
                return;
            }

            var method = del.Method;
            EditorGUI.LabelField(area, "Method", method.Name);

            EasyAnimatorGUI.NextVerticalArea(ref area);

            var target = del.Target;
            if (target is Object obj)
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;

                EditorGUI.ObjectField(area, "Target", obj, obj.GetType(), true);

                GUI.enabled = enabled;
            }
            else if (target != null)
            {
                EditorGUI.LabelField(area, "Target", target.ToString());
            }
            else
            {
                EditorGUI.LabelField(area, "Declaring Type", method.DeclaringType.GetNameCS());
            }

            EasyAnimatorGUI.NextVerticalArea(ref area);
        }

         
    }
}

#endif

