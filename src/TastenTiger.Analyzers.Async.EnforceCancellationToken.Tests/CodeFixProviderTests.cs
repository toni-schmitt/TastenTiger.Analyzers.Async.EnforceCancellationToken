using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    TastenTiger.Analyzers.Async.EnforceCancellationToken.SyntaxAnalyzer,
    TastenTiger.Analyzers.Async.EnforceCancellationToken.CodeFixProvider>;

namespace TastenTiger.Analyzers.Async.EnforceCancellationToken.Tests;

public class CodeFixProviderTests
{
    public static IEnumerable<object[]>
        MissingOptionalCancellationToken_AddsOptionalCancellationTokenAsLastParameter_TestData()
    {
        var deserializer = new DeserializerBuilder().WithNamingConvention(NullNamingConvention.Instance).Build();

        var violating = deserializer.Deserialize<TestData.TestData[]>(File.ReadAllText("TestData/violations.yaml"));
        var complying = deserializer.Deserialize<TestData.TestData[]>(File.ReadAllText("TestData/complying.yaml"));

        if (violating.Length != complying.Length) throw new InvalidOperationException();

        for (var i = 0; i < violating.Length; i++)
            yield return
            [
                violating[i].FileContent,
                new FileLinePositionSpan(string.Empty,
                    new LinePosition(violating[i].Position[0], violating[i].Position[1]),
                    new LinePosition(violating[i].Position[2], violating[i].Position[3])),
                violating[i].MethodName,
                complying[i].FileContent
            ];
    }

    [Theory]
    [MemberData(nameof(MissingOptionalCancellationToken_AddsOptionalCancellationTokenAsLastParameter_TestData))]
    public async Task MissingOptionalCancellationToken_AddsOptionalCancellationTokenAsLastParameter(
        string violatingTestCode, FileLinePositionSpan expectedLinePosition, string methodName,
        string complyingTestCode)
    {
        var expected = Verifier.Diagnostic()
            .WithLocation(expectedLinePosition.StartLinePosition.Line, expectedLinePosition.StartLinePosition.Character)
            .WithArguments(methodName);
        await Verifier.VerifyCodeFixAsync(violatingTestCode, expected, complyingTestCode).ConfigureAwait(false);
    }
}