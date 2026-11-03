using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TastenTiger.Analyzers.Async.EnforceCancellationToken;

/// <summary>
///     A syntax analyzer that reports any violations of Methods that are async or Task-/ValueTask-Returning without a
///     CancellationToken parameter
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SyntaxAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TA0001";

    /// <summary>
    ///     The .editorconfig key that lists further types accepted in place of a <c>CancellationToken</c> as the last
    ///     parameter, such as <c>ProtoBuf.Grpc.CallContext</c>. The value is a comma-separated list of full metadata
    ///     names; see <see cref="GetFullMetadataName" /> for the exact format.
    /// </summary>
    /// <remarks>
    ///     The list is opt-in rather than derived from the type's shape: a type merely exposing a
    ///     <c>CancellationToken</c> member (a request DTO, a <c>CancellationTokenSource</c>) does not make a method
    ///     cancellable, so each accepted type has to be a deliberate decision of the consuming project.
    /// </remarks>
    public const string AdditionalCancellationTokenTypesOptionKey =
        $"dotnet_diagnostic.{DiagnosticId}.additional_cancellation_token_types";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Missing CancellationToken",
        "{0} is missing a CancellationToken as the last parameter",
        "Usage",
        DiagnosticSeverity.Warning,
        true,
        "All async or task-returning methods must have a CancellationToken as the last parameter."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();


        context.RegisterSymbolAction(Action, SymbolKind.Method);
    }

    private static void Action(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method)
            return;

        if (method.IsImplicitlyDeclared || method.Name is "<Main>$")
            return;

        if (method.MethodKind is not MethodKind.Ordinary)
            return;

        if (method.IsOverride || method.ExplicitInterfaceImplementations.Length > 0)
            return;

        if (method.ExplicitOrImplicitInterfaceImplementations().Length > 0)
            return;

        if (IsTaskReturning(method) is false && method.IsAsync is false)
            return;

        if (HasCancellationTokenAsLastParameter(method))
            return;

        if (HasAdditionalCancellationTokenTypeAsLastParameter(method, context.Options))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));

        return;

        static bool IsTaskReturning(IMethodSymbol method)
        {
            if (method.ReturnType is not INamedTypeSymbol named)
                return false;

            if (named.ContainingNamespace.ToDisplayString() != "System.Threading.Tasks")
                return false;

            return named.Name is "Task" or "ValueTask";
        }

        static bool HasCancellationTokenAsLastParameter(IMethodSymbol method)
        {
            if (method.Parameters.Length == 0)
                return false;

            var lastParam = method.Parameters.Last();

            if (lastParam.Type is not INamedTypeSymbol namedType)
                return false;

            return namedType.Name == "CancellationToken" &&
                   namedType.ContainingNamespace.ToDisplayString() == "System.Threading";
        }
    }

    /// <summary>
    ///     Checks whether the last parameter of <paramref name="method" /> is one of the types listed under
    ///     <see cref="AdditionalCancellationTokenTypesOptionKey" /> in the .editorconfig that applies to the method's
    ///     source file.
    /// </summary>
    /// <param name="method">The task-returning or async method being analyzed.</param>
    /// <param name="options">The analyzer options carrying the .editorconfig values of the compilation.</param>
    /// <returns>
    ///     <see langword="true" /> if the last parameter's type exactly matches a configured name; derived types and
    ///     unconfigured projects never match.
    /// </returns>
    private static bool HasAdditionalCancellationTokenTypeAsLastParameter(IMethodSymbol method,
        AnalyzerOptions options)
    {
        if (method.Parameters.Length is 0)
            return false;

        if (method.Parameters.Last().Type is not INamedTypeSymbol lastParameterType)
            return false;

        var syntaxTree = method.Locations[0].SourceTree;
        if (syntaxTree is null)
            return false;

        var configOptions = options.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        if (configOptions.TryGetValue(AdditionalCancellationTokenTypesOptionKey, out var configuredTypes) is false)
            return false;

        var lastParameterTypeName = GetFullMetadataName(lastParameterType);
        return configuredTypes
            .Split(',')
            .Any(configuredType => configuredType.Trim() == lastParameterTypeName);
    }

    /// <summary>
    ///     Builds the name a type is configured by: the namespace-qualified metadata name as reported by
    ///     <see cref="System.Type.FullName" />, e.g. <c>ProtoBuf.Grpc.CallContext</c>, <c>Outer+Nested</c> for nested
    ///     types and <c>Context`1</c> for generic types.
    /// </summary>
    /// <remarks>
    ///     The metadata name is used instead of the C# display name so that a configured name identifies a generic
    ///     type definition independent of its type arguments.
    /// </remarks>
    /// <param name="type">The type to name.</param>
    /// <returns>The full metadata name of <paramref name="type" />, without assembly information.</returns>
    private static string GetFullMetadataName(INamedTypeSymbol type)
    {
        var name = type.MetadataName;
        for (var containingType = type.ContainingType;
             containingType is not null;
             containingType = containingType.ContainingType)
            name = $"{containingType.MetadataName}+{name}";

        if (type.ContainingNamespace.IsGlobalNamespace)
            return name;

        return $"{type.ContainingNamespace.ToDisplayString()}.{name}";
    }
}