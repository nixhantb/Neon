
using System.Collections.ObjectModel;
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
        public IReadOnlyList<object> Args { get; set; } = Array.Empty<object>();
        /// <summary>
        /// Gets the default target queue for this job.
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// Creates a new Job from an expression tree
        /// </summary>
        /// <param name="method"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static Job FromExpression(Expression<Action> methodCall, string queue)
        {
            return FromExpression(methodCall, null, queue);
        }

        public static Job FromExpression(Expression<Func<Task>> methodCall, string? queue)
        {
            return FromExpression(methodCall, null, queue);
        }

        public static Job FromExpression<TType>(Expression<Action<TType>> methodCall, string? queue)
        {
            return FromExpression(methodCall, typeof(TType), queue);
        }

        public static Job FromExpression<TType>(Expression<Func<TType, Task>> methodCall, string? queue = null)
        {
            return FromExpression(methodCall, typeof(TType), queue);
        }


        public static Job FromExpression([NotNull] LambdaExpression methodCall, Type explicitType, string queue)
        {

            if (methodCall == null)
            {
                throw new ArgumentNullException(nameof(methodCall));
            }

            if (methodCall.Body is not MethodCallExpression callExpression)
            {
                throw new ArgumentException("Expression body should be of type MethodCallExpression", nameof(methodCall));
            }

            var type = explicitType ?? callExpression.Method.DeclaringType;
            var method = callExpression.Method;

            // Handle instance method calls with object evaluation
            if (explicitType == null && callExpression.Object != null)
            {
                var objectValue = GetExpressionValue(callExpression.Object);
                if (objectValue == null)
                {
                    throw new InvalidOperationException("Expression object should not be null");
                }

                type = objectValue.GetType();
                method = type.GetMethod(callExpression.Method.Name,
                    callExpression.Method.GetParameters().Select(p => p.ParameterType).ToArray()) ?? callExpression.Method;
            }

            ValidateMethod(method);

            return new Job
            {
                Type = type ?? throw new InvalidOperationException("Could not determine the job type"),
                Method = method,
                Args = GetExpressionValues(callExpression.Arguments),
                Queue = queue
            };
        }

        private static object[] GetExpressionValues(ReadOnlyCollection<Expression> expressions)
        {

            var result = expressions.Count > 0 ? new Object[expressions.Count] : [];

            int index = 0;
            foreach (var expression in expressions)
            {
                result[index++] = GetExpressionValue(expression);
            }
            return result;
        }

        private static object GetExpressionValue(Expression expression)
        {

            if (expression is ConstantExpression constantExpression && constantExpression.Value != null)
            {
                return constantExpression.Value;
            }

            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
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

                var parameterTypeInfo = parameter.ParameterType.GetTypeInfo();

                if (parameterTypeInfo.IsSubclassOf(typeof(Delegate)) || parameterTypeInfo.IsSubclassOf(typeof(Expression)))
                {

                    throw new NotSupportedException(
                        "Anonymous functions, delegates and lambda expressions aren't supported in job method parameters.");
                }
            }

        }

    }
}