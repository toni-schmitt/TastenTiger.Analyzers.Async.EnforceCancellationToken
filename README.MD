# TastenTiger.Analyzers.Async.EnforceCancellationToken

A Roslyn analyzer that ensures asynchronous methods, those returning `Task` or `ValueTask`, include a
`CancellationToken` as their final parameter.

[![Get it on Coeberg](./docs/img/get-it-on-codeberg.svg)](https://codeberg.org/toni-schmitt/TastenTiger.Analyzers.Async.EnforceCancellationToken)

[![NuGet Version](https://img.shields.io/nuget/v/TastenTiger.Analyzers.Async.EnforceCancellationToken)](https://www.nuget.org/packages/TastenTiger.Analyzers.Async.EnforceCancellationToken)

## Why enforce `CancellationToken` usage?

Asynchronous operations often involve I/O, long-running work, or external dependencies. When a caller no longer needs
the result, continuing the operation wastes CPU time, memory, and other server resources.

Most asynchronous .NET Methods already include a `CancellationToken` as their last parameter. This analyzer enforces
this best practice via compilation warning.

Requiring a `CancellationToken`:

- Allows callers to cancel work early
- Reduces an unnecessary load on your system
- Encourages predictable async APIs
- Enforces best practices

## What this analyzer does

- Produces the `TA0001` compilation warning for methods that
    - use the `async` keyword
    - return a `Task`
    - return a `ValueTask`
- Provides a bulk quick fix to automatically add `System.Threading.CancellationToken cancellationToken = default`
  parameter to the end of the parameter list

## Examples

```csharp
public async Task DoWorkAsync() // <-- violating TA0001 
{
}

public async Task DoWorkAsync(CancellationToken cancellationToken = default) // <-- complying TA0001
{
}
```

> For more examples see `TastenTiger.Analyzers.Async.EnforceCancellationToken.Sample.Examples.cs`

## Configuration

### Accepting other types in place of `CancellationToken`

Some frameworks pass the cancellation token wrapped in their own context type, such as `ProtoBuf.Grpc.CallContext` or
`Grpc.Core.ServerCallContext`. List these types in your `.editorconfig` (or `.globalconfig`) to accept them as the last
parameter:

```ini
[*.cs]
dotnet_diagnostic.TA0001.additional_cancellation_token_types = ProtoBuf.Grpc.CallContext, Grpc.Core.ServerCallContext
```

- The value is a comma-separated list of full type names, as reported by `Type.FullName`: nested types use `+`
  (`Outer+Nested`), generic types use their arity (``Context`1``).
- Names are matched exactly. Types derived from a listed type are not accepted.
- The type still has to be the last parameter.

```csharp
public Task<Reply> GetAsync(Request request, CallContext context = default) // <-- complying TA0001 when configured
```

