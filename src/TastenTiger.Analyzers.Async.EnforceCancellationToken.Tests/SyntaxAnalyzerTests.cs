using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    TastenTiger.Analyzers.Async.EnforceCancellationToken.SyntaxAnalyzer>;

namespace TastenTiger.Analyzers.Async.EnforceCancellationToken.Tests;

public class SyntaxAnalyzerTests
{
    public static IEnumerable<object[]> ViolatingExamples_AlertDiagnostic_TestData()
    {
        var deserializer = new DeserializerBuilder().WithNamingConvention(NullNamingConvention.Instance).Build();
        using var textReader = File.OpenText("TestData/violations.yaml");
        var testData = deserializer.Deserialize<TestData.TestData[]>(textReader);
        return testData.Select<TestData.TestData, object[]>(x =>
        [
            x.FileContent,
            new FileLinePositionSpan(string.Empty, new LinePosition(x.Position[0], x.Position[1]),
                new LinePosition(x.Position[2], x.Position[3])),
            x.MethodName
        ]);
    }

    [Theory]
    [MemberData(nameof(ViolatingExamples_AlertDiagnostic_TestData))]
    public async Task ViolatingExamples_AlertDiagnostic(string testCode,
        FileLinePositionSpan expectedLinePosition,
        string methodName)
    {
        var expected = Verifier.Diagnostic()
            .WithSpan(expectedLinePosition.StartLinePosition.Line, expectedLinePosition.StartLinePosition.Character,
                expectedLinePosition.EndLinePosition.Line, expectedLinePosition.EndLinePosition.Character)
            .WithArguments(methodName);
        await Verifier.VerifyAnalyzerAsync(testCode, expected);
    }
}