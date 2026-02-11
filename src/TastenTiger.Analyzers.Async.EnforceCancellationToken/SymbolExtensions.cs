using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TastenTiger.Analyzers.Async.EnforceCancellationToken;

internal static class SymbolExtensions
{
    // Source - https://stackoverflow.com/a/65068997
    // Posted by SENya, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-02-11, License - CC BY-SA 4.0
    public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(this ISymbol symbol)
    {
        if (symbol.Kind != SymbolKind.Method && symbol.Kind != SymbolKind.Property && symbol.Kind != SymbolKind.Event)
            return ImmutableArray<ISymbol>.Empty;

        var containingType = symbol.ContainingType;
        var query = from iface in containingType.AllInterfaces
            from interfaceMember in iface.GetMembers()
            let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
            where SymbolEqualityComparer.Default.Equals(symbol, impl)
            select interfaceMember;
        return query.ToImmutableArray();
    }
}