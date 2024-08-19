
using System;
using UnityEngine;

namespace EasyAnimator.FSM
{

    public interface IState
    {
        bool CanEnterState { get; }


        bool CanExitState { get; }


        void OnEnterState();


        void OnExitState();
    }

     


    public interface IOwnedState<TState> : IState where TState : class, IState
    {
        StateMachine<TState> OwnerStateMachine { get; }
    }

     


    public abstract class State : IState
    {
         

        public virtual bool CanEnterState => true;

        public virtual bool CanExitState => true;

        public virtual void OnEnterState() { }

        public virtual void OnExitState() { }

         
    }

     


    [HelpURL(APIDocumentationURL + nameof(StateExtensions))]
    public static class StateExtensions
    {
         

        public const string APIDocumentationURL = "";

         

        public static TState GetPreviousState<TState>(this TState state)
            where TState : class, IState
            => StateChange<TState>.PreviousState;

        public static TState GetNextState<TState>(this TState state)
            where TState : class, IState
            => StateChange<TState>.NextState;

         

        public static bool IsCurrentState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.CurrentState == state;

         

        public static bool TryEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.TrySetState(state);

         

        public static bool TryReEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.TryResetState(state);

         

        public static void ForceEnterState<TState>(this TState state)
            where TState : class, IOwnedState<TState>
            => state.OwnerStateMachine.ForceSetState(state);

         
#pragma warning disable IDE0079 // Remove unnecessary suppression.
#pragma warning disable CS1587 // XML comment is not placed on a valid language element.
#pragma warning restore IDE0079 // Remove unnecessary suppression.
        // Copy this #region into a class which implements IOwnedState to give it the state extension methods as regular members.
        // This will avoid any issues with the compiler inferring the wrong generic argument in the extension methods.
        // 
        //#region State Extensions
        // 

        //public bool IsCurrentState() => OwnerStateMachine.CurrentState == this;

        // 

        //public bool TryEnterState() => OwnerStateMachine.TrySetState(this);

        // 

        //public bool TryReEnterState() => OwnerStateMachine.TryResetState(this);

        // 

        //public void ForceEnterState() => OwnerStateMachine.ForceSetState(this);

        // 
        //#endregion
        // 

#if UNITY_ASSERTIONS
        internal static string GetChangeError(Type stateType, Type machineType, string changeType = "State")
        {
            Type previousType = null;
            Type baseStateType = null;
            System.Collections.Generic.HashSet<Type> activeChangeTypes = null;

            var stackTrace = new System.Diagnostics.StackTrace(1, false).GetFrames();
            for (int i = 0; i < stackTrace.Length; i++)
            {
                var type = stackTrace[i].GetMethod().DeclaringType;
                if (type != previousType &&
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == machineType)
                {
                    var argument = type.GetGenericArguments()[0];
                    if (argument.IsAssignableFrom(stateType))
                    {
                        baseStateType = argument;
                        break;
                    }
                    else
                    {
                        if (activeChangeTypes == null)
                            activeChangeTypes = new System.Collections.Generic.HashSet<Type>();

                        if (!activeChangeTypes.Contains(argument))
                            activeChangeTypes.Add(argument);
                    }
                }

                previousType = type;
            }

            var text = new System.Text.StringBuilder()
                .Append("Attempted to access ")
                .Append(changeType)
                .Append("Change<")
                .Append(stateType.FullName)
                .Append($"> but no {nameof(StateMachine<IState>)} of that type is currently changing its ")
                .Append(changeType)
                .AppendLine(".");

            if (baseStateType != null)
            {
                text.Append(" - ")
                    .Append(changeType)
                    .Append(" changes must be accessed using the base ")
                    .Append(changeType)
                    .Append(" type, which is ")
                    .Append(changeType)
                    .Append("Change<")
                    .Append(baseStateType.FullName)
                    .AppendLine("> in this case.");

                var caller = stackTrace[1].GetMethod();
                if (caller.DeclaringType == typeof(StateExtensions))
                {
                    var propertyName = stackTrace[0].GetMethod().Name;
                    propertyName = propertyName.Substring(4, propertyName.Length - 4);// Remove the "get_".

                    text.Append(" - This may be caused by the compiler incorrectly inferring the generic argument of the Get")
                        .Append(propertyName)
                        .Append(" method, in which case it must be manually specified like so: state.Get")
                        .Append(propertyName)
                        .Append('<')
                        .Append(baseStateType.FullName)
                        .AppendLine(">()");
                }
            }
            else
            {
                if (activeChangeTypes == null)
                {
                    text.Append(" - No other ")
                        .Append(changeType)
                        .AppendLine(" changes are currently occurring either.");
                }
                else
                {
                    if (activeChangeTypes.Count == 1)
                    {
                        text.Append(" - There is 1 ")
                            .Append(changeType)
                            .AppendLine(" change currently occurring:");
                    }
                    else
                    {
                        text.Append(" - There are ")
                            .Append(activeChangeTypes.Count)
                            .Append(' ')
                            .Append(changeType)
                            .AppendLine(" changes currently occurring:");
                    }

                    foreach (var type in activeChangeTypes)
                    {
                        text.Append("     - ")
                            .AppendLine(type.FullName);
                    }
                }
            }

            text.Append(" - ")
                .Append(changeType)
                .Append("Change<")
                .Append(stateType.FullName)
                .AppendLine($">.{nameof(StateChange<IState>.IsActive)} can be used to check if a change of that type is currently occurring.")
                .AppendLine(" - See the documentation for more information: " +
                    "");

            return text.ToString();
        }
#endif

         
    }
}
