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

        if (method.IsImplicitlyDeclared)
            return;

        if (method.MethodKind is not MethodKind.Ordinary)
            return;

        if (method.IsOverride || method.ExplicitInterfaceImplementations.Length > 0)
            return;

        if (IsTaskReturning(method) is false && method.IsAsync is false)
            return;

        if (HasCancellationTokenAsLastParameter(method))
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
}