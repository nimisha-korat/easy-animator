
using System;

namespace EasyAnimator
{
    
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DrawAfterEventsAttribute : Attribute { }
}

