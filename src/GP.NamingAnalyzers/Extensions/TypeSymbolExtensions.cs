// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using Microsoft.CodeAnalysis;

namespace GP.NamingAnalyzers.Extensions;

/// <summary>
/// Contains extension methods for <see cref="ITypeSymbol"/>.
/// </summary>
public static class TypeSymbolExtensions
{
    /// <summary>
    /// Returns a value indicating whether the provided <see cref="ITypeSymbol"/> original definition is the provided <see cref="INamedTypeSymbol"/> or implements it.
    /// </summary>
    /// <param name="typeSymbol">An instance of <see cref="ITypeSymbol"/>.</param>
    /// <param name="namedTypeSymbol">An instance of <see cref="INamedTypeSymbol"/>.</param>
    /// <returns><see langword="true"/> if the provided <see cref="ITypeSymbol"/> original definition is the provided <see cref="INamedTypeSymbol"/> or implements it; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">If the provided <see cref="ITypeSymbol"/> is <see langword="null"/>.</exception>
    public static bool HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(this ITypeSymbol typeSymbol, INamedTypeSymbol namedTypeSymbol)
    {
        if (typeSymbol is null)
        {
            throw new ArgumentNullException(nameof(typeSymbol));
        }

        bool isNamedTypeSymbol = typeSymbol.OriginalDefinition.Equals(namedTypeSymbol, SymbolEqualityComparer.Default);
        bool implementsInterface = typeSymbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(namedTypeSymbol, SymbolEqualityComparer.Default));

        return isNamedTypeSymbol || implementsInterface;
    }
}
