
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Neon.Common
{
    // <summary>
    /// Represents a method call that can be marshalled and executed in the background.
    /// Inspired by Hangfire's Job class but simplified for our use case.
    /// </summary>
    public class Job
    {

        // <summary>
        /// Gets the type that contains the method to be invoked.
        /// </summary>
        [NotNull]
        public Type Type { get; set; }
        [NotNull]
        public MethodInfo Method { get; set; }
        [NotNull]
        // <summary>
        /// Gets the arguments that should be passed to the method.
        /// </summary>
        public IReadOnlyList<object> Args { get; }
        /// <summary>
        /// Gets the default target queue for this job.
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// Creates a new Job from an expression tree
        /// </summary>
        /// <param name="method"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static Job FromExpression(Expression<Action> method, string queue)
        {

        }
        public static Job FromExpression(Expression<Func<Task>> methodCall, string? queue)
        {

        }

        /// <summary>
        /// Creates a new job with explicit type specification.
        /// </summary>
        public static Job FromExpression<TType>(Expression<Action<TType>> methodCall, string? queue)
        {

        }
        /// <summary>
        /// Creates a new job with explicit type specification for async methods.
        /// </summary>
        public static Job FromExpression<TType>(Expression<Func<TType, Task>> methodCall, string? queue = null)
        {

        }


        private static void ValidateMethod(MethodInfo method)
        {
            // runtime reflection error checks
            // Only public methods can be enqueued
            if (!method.IsPublic)
            {
                throw new NotSupportedException("Only public methods can be invoked in the background.");
            }
            //f a method has open generic parameters (e.g., DoWork<T>(T input) without specifying T),
            // it cannot be serialized/stored or resolved later.
            // only closed concrete methods are allowed
            if (method.ContainsGenericParameters)
            {
                throw new NotSupportedException("Job method cannot contain unassigned generic type parameters.");
            }
            // Jobs pointing to "orphan" methods.
            if (method.DeclaringType == null)
            {
                throw new NotSupportedException("Global methods are not supported. Use class methods instead.");
            }
            if (method.ReturnType == typeof(void))
            {

            }
            var parameters = method.GetParameters();
            // Prevents Unserialised parameter state


            foreach (var parameter in parameters)
            {
                if (parameter.IsOut)
                {
                    throw new NotSupportedException("Output parameters are not supporter");
                }
                if (parameter.ParameterType.IsByRef)
                {
                    throw new NotSupportedException("Parameters, passed by reference are not supported");
                }
            }
            

        }
        
    }
}