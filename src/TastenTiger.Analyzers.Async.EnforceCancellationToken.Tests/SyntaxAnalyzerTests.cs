using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    TastenTiger.Analyzers.Async.EnforceCancellationToken.SyntaxAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

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

    [Fact]
    public async Task TopLevelProgram_AsyncEntryPoint_DoesNotAlertDiagnostic()
    {
        const string topLevelCode =
            """
            await System.Threading.Tasks.Task.Delay(1000);
            """;

        var test = new CSharpAnalyzerTest<SyntaxAnalyzer, DefaultVerifier>
        {
            TestCode = topLevelCode
        };
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            var compilationOptions = ((CSharpCompilationOptions)project.CompilationOptions!)
                .WithOutputKind(OutputKind.ConsoleApplication);
            var parseOptions = ((CSharpParseOptions)project.ParseOptions!)
                .WithLanguageVersion(LanguageVersion.CSharp9);
            return solution
                .WithProjectCompilationOptions(projectId, compilationOptions)
                .WithProjectParseOptions(projectId, parseOptions);
        });

        await test.RunAsync();
    }

    [Fact]
    public async Task InterfaceImplementation_WithoutCancellationToken_DoesNotAlertDiagnostic()
    {
        const string partiallyViolatingCode =
            """
            using System.Threading.Tasks;

            interface IViolatingInterface
            {
                Task ViolatingAsync();
            }

            public sealed class ComplyingImplementation : IViolatingInterface
            {
                public Task ViolatingAsync()
                {
                    return Task.CompletedTask;
                }
            }

            """;

        const string methodName = "ViolatingAsync";
        var expected = Verifier.Diagnostic()
            .WithSpan(5, 10, 5, 24)
            .WithArguments(methodName);
        await Verifier.VerifyAnalyzerAsync(partiallyViolatingCode, expected);
    }

    [Theory]
    [InlineData("ProtoBuf.Grpc.CallContext")]
    [InlineData("Other.Type, ProtoBuf.Grpc.CallContext")]
    [InlineData("  Other.Type ,ProtoBuf.Grpc.CallContext  ")]
    public async Task ConfiguredAdditionalTypeAsLastParameter_DoesNotAlertDiagnostic(string configuredTypes)
    {
        const string code =
            """
            using System.Threading.Tasks;

            namespace ProtoBuf.Grpc
            {
                public readonly struct CallContext { }
            }

            public class Service
            {
                public Task ComplyingAsync(string request, ProtoBuf.Grpc.CallContext context = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;
        var test = CreateTestWithAdditionalCancellationTokenTypes(code, configuredTypes);

        await test.RunAsync();
    }

    [Fact]
    public async Task ConfiguredNestedTypeAsLastParameter_DoesNotAlertDiagnostic()
    {
        const string code =
            """
            using System.Threading.Tasks;

            namespace Contexts
            {
                public class Outer
                {
                    public readonly struct Nested { }
                }
            }

            public class Service
            {
                public Task ComplyingAsync(Contexts.Outer.Nested context)
                {
                    return Task.CompletedTask;
                }
            }
            """;
        var test = CreateTestWithAdditionalCancellationTokenTypes(code, "Contexts.Outer+Nested");

        await test.RunAsync();
    }

    [Fact]
    public async Task UnconfiguredTypeAsLastParameter_AlertsDiagnostic()
    {
        const string code =
            """
            using System.Threading.Tasks;

            namespace ProtoBuf.Grpc
            {
                public readonly struct CallContext { }
            }

            public class Service
            {
                public Task ViolatingAsync(ProtoBuf.Grpc.CallContext context = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;
        var test = CreateTestWithAdditionalCancellationTokenTypes(code, "ProtoBuf.Grpc.OtherContext");
        test.ExpectedDiagnostics.Add(Verifier.Diagnostic().WithSpan(10, 17, 10, 31).WithArguments("ViolatingAsync"));

        await test.RunAsync();
    }

    [Fact]
    public async Task ConfiguredTypeNotAsLastParameter_AlertsDiagnostic()
    {
        const string code =
            """
            using System.Threading.Tasks;

            namespace ProtoBuf.Grpc
            {
                public readonly struct CallContext { }
            }

            public class Service
            {
                public Task ViolatingAsync(ProtoBuf.Grpc.CallContext context, string request)
                {
                    return Task.CompletedTask;
                }
            }
            """;
        var test = CreateTestWithAdditionalCancellationTokenTypes(code, "ProtoBuf.Grpc.CallContext");
        test.ExpectedDiagnostics.Add(Verifier.Diagnostic().WithSpan(10, 17, 10, 31).WithArguments("ViolatingAsync"));

        await test.RunAsync();
    }

    [Fact]
    public async Task DerivedTypeOfConfiguredTypeAsLastParameter_AlertsDiagnostic()
    {
        const string code =
            """
            using System.Threading.Tasks;

            namespace Contexts
            {
                public class BaseContext { }

                public class DerivedContext : BaseContext { }
            }

            public class Service
            {
                public Task ViolatingAsync(Contexts.DerivedContext context)
                {
                    return Task.CompletedTask;
                }
            }
            """;
        var test = CreateTestWithAdditionalCancellationTokenTypes(code, "Contexts.BaseContext");
        test.ExpectedDiagnostics.Add(Verifier.Diagnostic().WithSpan(12, 17, 12, 31).WithArguments("ViolatingAsync"));

        await test.RunAsync();
    }

    /// <summary>
    ///     Creates an analyzer test whose .editorconfig accepts <paramref name="configuredTypes" /> in place of a
    ///     CancellationToken for every source file.
    /// </summary>
    /// <param name="code">The source under test.</param>
    /// <param name="configuredTypes">The raw value of the additional cancellation token types option.</param>
    /// <returns>A test without expected diagnostics; callers add them where the code violates the rule.</returns>
    private static CSharpAnalyzerTest<SyntaxAnalyzer, DefaultVerifier> CreateTestWithAdditionalCancellationTokenTypes(
        string code, string configuredTypes)
    {
        var test = new CSharpAnalyzerTest<SyntaxAnalyzer, DefaultVerifier> { TestCode = code };
        test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig",
            $"""
             root = true

             [*]
             {SyntaxAnalyzer.AdditionalCancellationTokenTypesOptionKey} = {configuredTypes}
             """));
        return test;
    }
}