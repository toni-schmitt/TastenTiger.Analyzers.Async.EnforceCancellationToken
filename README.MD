# TastenTiger.Analyzers.Async.EnforceCancellationToken

A Roslyn analyzer that ensures asynchronous methods, those returning `Task` or `ValueTask`, include a
`CancellationToken` as their final parameter.

[![Get it on Codeberg](https://codeberg.org/Codeberg/GetItOnCodeberg/raw/branch/main/get-it-on-blue-on-white.png)](https://codeberg.org/toni-schmitt/TastenTiger.Analyzers.Async.EnforceCancellationToken)

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

