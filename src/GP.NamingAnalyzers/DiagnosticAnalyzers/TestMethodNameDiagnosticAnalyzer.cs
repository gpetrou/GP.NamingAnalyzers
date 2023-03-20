// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate test method names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestMethodNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0101";
    private const string Title = "Incorrect test method name";
    private const string MessageFormat = "Test method '{0}' does not follow the 'MethodUnderTest_When_Should' naming convention";
    private const string Description = "A test method name should follow the 'MethodUnderTest_When_Should' naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private static readonly DiagnosticDescriptor DiagnosticDescriptor = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        Description,
        HelpLinkUri);

    private static readonly HashSet<string> UniqueTestAttributeNames = new()
    {
        "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute",
        "NUnit.Framework.TestAttribute",
        "NUnit.Framework.TheoryAttribute",
        "Xunit.FactAttribute",
        "Xunit.TheoryAttribute"
    };

    private static readonly Regex TestMethodNameRegex = new("^[a-zA-Z0-9_]+_When[a-zA-Z0-9_]+_Should[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether a test method name is valid.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns><see langword="true"/> if the provided name is a valid dictionary name; otherwise, <see langword="false"/>.</returns>
    public static bool IsTestMethodNameValid(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        TestMethodNameRegex.IsMatch(name);

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol is IMethodSymbol methodSymbol)
        {
            ImmutableArray<AttributeData> attributes = methodSymbol.GetAttributes();
            foreach (AttributeData attribute in attributes)
            {
                string? attributeClassAsString = attribute.AttributeClass?.ToString();
                if (attributeClassAsString is not null && UniqueTestAttributeNames.Contains(attributeClassAsString))
                {
                    if (!IsTestMethodNameValid(methodSymbol.Name))
                    {
                        Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, methodSymbol.Locations[0], methodSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
        {
            bool foundAtLeastOneAttribute = false;
            foreach (string testAttributeName in UniqueTestAttributeNames)
            {
                INamedTypeSymbol? namedTypeSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(testAttributeName);
                if (namedTypeSymbol is not null)
                {
                    foundAtLeastOneAttribute = true;
                    break;
                }
            }

            if (!foundAtLeastOneAttribute)
            {
                return;
            }

            compilationStartAnalysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        });
    }
}
