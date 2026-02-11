using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TastenTiger.Analyzers.Async.EnforceCancellationToken.CodeFix;

/// <summary>
///     A code fix provider that automatically adds an optional CancellationToken as the last parameter to any Method that
///     violates TA0001
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(CodeFixProvider))]
[Shared]
public class CodeFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(SyntaxAnalyzer.DiagnosticId);

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics[0];

        var methodDecl = root?.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create("Add optional CancellationToken parameter",
                c => AddCancellationTokenAsync(context.Document, methodDecl, c),
                "AddOptionalCancellationToken"), diagnostic);
    }

    private static async Task<Document> AddCancellationTokenAsync(Document document, MethodDeclarationSyntax methodDecl,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var optionalCancellationTokenParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
            .WithType(SyntaxFactory.ParseTypeName("System.Threading.CancellationToken")).WithDefault(
                SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)));

        var newParameterList = methodDecl.ParameterList.AddParameters(optionalCancellationTokenParameter);
        var newMethodDecl = methodDecl.WithParameterList(newParameterList);

        return document.WithSyntaxRoot(root.ReplaceNode(methodDecl, newMethodDecl));
    }
}