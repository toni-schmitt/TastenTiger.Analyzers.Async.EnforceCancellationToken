using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable AsyncVoidMethod


namespace TastenTiger.Analyzers.Async.EnforceCancellationToken.Sample;

public class Examples
{
    public class Violating
    {
        public static async Task AsyncTaskReturningMethodWithoutCancellationToken()
        {
            await Task.Delay(1000);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithoutCancellationToken()
        {
            await Task.Delay(1000);
        }

        public static Task TaskReturningMethodWithoutCancellationToken()
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithoutCancellationToken()
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithoutCancellationToken()
        {
            await Task.Delay(1000);
            return 42;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithoutCancellationToken()
        {
            await Task.Delay(1000);
            return 42;
        }

        public static Task<int> GenericTaskReturningMethodWithoutCancellationToken()
        {
            return Task.FromResult(42);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithoutCancellationToken()
        {
            return ValueTask.FromResult(42);
        }

        public static async void AsyncMethodWithoutCancellationToken()
        {
            await Task.Delay(1000);
        }

        public static async Task AsyncTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            await Task.Delay(parameter);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            await Task.Delay(parameter);
        }

        public static Task TaskReturningMethodWithoutCancellationToken(int parameter)
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            await Task.Delay(parameter);
            return parameter;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            await Task.Delay(parameter);
            return parameter;
        }

        public static Task<int> GenericTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            return Task.FromResult(parameter);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithoutCancellationToken(int parameter)
        {
            return ValueTask.FromResult(parameter);
        }

        public static async void AsyncMethodWithoutCancellationToken(int parameter)
        {
            await Task.Delay(parameter);
        }

        public static async Task AsyncTaskReturningMethodWithoutCancellationToken(string value, int parameter = 0)
        {
            await Task.Delay(parameter);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithoutCancellationToken(string value,
            int parameter = 0)
        {
            await Task.Delay(parameter);
        }

        public static Task TaskReturningMethodWithoutCancellationToken(string value, int parameter = 0)
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithoutCancellationToken(string value, int parameter = 0)
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithoutCancellationToken(string value,
            int parameter = 0)
        {
            await Task.Delay(parameter);
            return parameter;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithoutCancellationToken(string value,
            int parameter = 0)
        {
            await Task.Delay(parameter);
            return parameter;
        }

        public static Task<int> GenericTaskReturningMethodWithoutCancellationToken(string value, int parameter = 0)
        {
            return Task.FromResult(parameter);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithoutCancellationToken(string value,
            int parameter = 0)
        {
            return ValueTask.FromResult(parameter);
        }

        public static async void AsyncMethodWithoutCancellationToken(string value, int parameter = 0)
        {
            await Task.Delay(parameter);
        }
    }

    public class Complying
    {
        public static async Task AsyncTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
        }

        public static Task TaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            return 42;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            return 42;
        }

        public static Task<int> GenericTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(42);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(42);
        }

        public static async void AsyncMethodWithCancellationToken(
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
        }

        public static async Task AsyncTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }

        public static Task TaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
            return parameter;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
            return parameter;
        }

        public static Task<int> GenericTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(parameter);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(parameter);
        }

        public static async void AsyncMethodWithCancellationToken(int parameter,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }

        public static async Task AsyncTaskReturningMethodWithCancellationToken(string value, int parameter = 0,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }

        public static async ValueTask AsyncValueTaskReturningMethodWithCancellationToken(string value,
            int parameter = 0, CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }

        public static Task TaskReturningMethodWithCancellationToken(string value, int parameter = 0,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public static ValueTask ValueTaskReturningMethodWithCancellationToken(string value, int parameter = 0,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public static async Task<int> AsyncGenericTaskReturningMethodWithCancellationToken(string value,
            int parameter = 0, CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
            return parameter;
        }

        public static async ValueTask<int> AsyncGenericValueTaskReturningMethodWithCancellationToken(string value,
            int parameter = 0, CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
            return parameter;
        }

        public static Task<int> GenericTaskReturningMethodWithCancellationToken(string value, int parameter = 0,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(parameter);
        }

        public static ValueTask<int> GenericValueTaskReturningMethodWithCancellationToken(string value,
            int parameter = 0, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(parameter);
        }

        public static async void AsyncMethodWithCancellationToken(string value, int parameter = 0,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(parameter, cancellationToken);
        }
    }
}