// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using GP.NamingAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate mocked member names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MockedMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0005";
    private const string Title = "Incorrect mocked member name";
    private const string MessageFormat = "Mocked member '{0}' does not {1}";
    private const string Description = "A mocked member name should follow the naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private const string PatternOptionName = $"dotnet_diagnostic.{DiagnosticId}.pattern";
    private const string DefaultDiagnosticMessageEnd = "follow the 'mocked' naming convention";
    private const string CustomRegexPatternMessageEnd = "match the '{0}' regex pattern";

    /// <summary>
    /// The default regex pattern.
    /// </summary>
    public const string DefaultRegexPattern = "^_?((M|m)ocked)[A-Z][a-zA-Z0-9]*$";

    private static readonly DiagnosticDescriptor DiagnosticDescriptor = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        Description,
        HelpLinkUri);

    private static readonly Dictionary<string, string> _mockMethodNamesByNamespaceAndClass = new()
    {
        { "Moq.Mock", "Mock" },
        { "NSubstitute.Substitute", "For" },
        { "FakeItEasy.A", "Fake" }
    };

    private string _regexPattern = DefaultRegexPattern;
    private string _diagnosticMessageEnd = DefaultDiagnosticMessageEnd;

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid mocked member name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="regexPattern">The regex pattern to use during validation.</param>
    /// <returns><see langword="true"/> if the provided name is a valid mocked member name; otherwise, <see langword="false"/>.</returns>
    public static bool IsMockedMemberNameValid(string name, string regexPattern) =>
        !string.IsNullOrWhiteSpace(name) &&
        Regex.IsMatch(name, regexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Analyzes a syntax node.
    /// </summary>
    /// <param name="context">An instance of <see cref="SyntaxNodeAnalysisContext"/>.</param>
    /// <param name="uniqueMockMethodNames">The hash set of mock method names to check.</param>
    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context, HashSet<string> uniqueMockMethodNames)
    {
        SemanticModel semanticModel = context.SemanticModel;
        string? methodName = null;
        SyntaxNode? parent = null;
        if (context.Node is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
        {
            ISymbol? symbol = semanticModel.GetSymbolInfo(objectCreationExpressionSyntax.Type).Symbol;
            if (symbol is not null)
            {
                methodName = symbol.Name;
            }

            parent = objectCreationExpressionSyntax.Parent;
        }

        if (context.Node is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            if (semanticModel.GetSymbolInfo(invocationExpressionSyntax.Expression).Symbol is not IMethodSymbol methodSymbol)
            {
                return;
            }

            methodName = methodSymbol.Name;
            parent = invocationExpressionSyntax.Parent;
        }

        if (methodName is null ||
            !uniqueMockMethodNames.Contains(methodName))
        {
            return;
        }

        if (parent is not EqualsValueClauseSyntax equalsValueClauseSyntax)
        {
            return;
        }

        if (equalsValueClauseSyntax.Parent is VariableDeclaratorSyntax variableDeclaratorSyntax)
        {
            if (!IsMockedMemberNameValid(variableDeclaratorSyntax.Identifier.ValueText, _regexPattern))
            {
                ISymbol? symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorSyntax);
                if (symbol is null)
                {
                    return;
                }

                Diagnostic diagnostic = Diagnostic.Create(
                    DiagnosticDescriptor,
                    symbol.Locations[0],
                    variableDeclaratorSyntax.Identifier.ValueText,
                    _diagnosticMessageEnd);
                context.ReportDiagnostic(diagnostic);
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
            HashSet<string> uniqueMethodNames = new();
            foreach (KeyValuePair<string, string> mockMethodNameByNamespaceAndClass in _mockMethodNamesByNamespaceAndClass)
            {
                INamedTypeSymbol? namedTypeSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(mockMethodNameByNamespaceAndClass.Key);
                if (namedTypeSymbol is not null)
                {
                    uniqueMethodNames.Add(mockMethodNameByNamespaceAndClass.Value);
                }
            }

            if (uniqueMethodNames.Count == 0)
            {
                return;
            }

            string? regexPattern = compilationStartAnalysisContext.ReadRegexPattern(PatternOptionName, DiagnosticId);
            if (regexPattern is not null && _regexPattern != regexPattern)
            {
                _regexPattern = regexPattern;
                _diagnosticMessageEnd = string.Format(CultureInfo.InvariantCulture, CustomRegexPatternMessageEnd, _regexPattern);
            }
            else if (_regexPattern != DefaultRegexPattern)
            {
                _regexPattern = DefaultRegexPattern;
                _diagnosticMessageEnd = DefaultDiagnosticMessageEnd;
            }

            compilationStartAnalysisContext.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockAnalysisContext =>
            {
                if (codeBlockAnalysisContext.OwningSymbol.Kind != SymbolKind.Method)
                {
                    return;
                }

                codeBlockAnalysisContext.RegisterSyntaxNodeAction(
                   syntaxNodeAnalysisContext => AnalyzeSyntaxNode(syntaxNodeAnalysisContext, uniqueMethodNames),
                   SyntaxKind.ObjectCreationExpression,
                   SyntaxKind.InvocationExpression);
            });
        });
    }
}
