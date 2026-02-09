namespace TastenTiger.Analyzers.Async.EnforceCancellationToken.Tests.TestData;

public sealed record TestData
{
    public required string MethodName { get; init; }
    public required int[] Position { get; init; }
    public required string FileContent { get; init; }
}