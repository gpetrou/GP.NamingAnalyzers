// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using GP.NamingAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate boolean names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BooleanMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0004";
    private const string Title = "Incorrect boolean name";
    private const string MessageFormat = "Boolean '{0}' does not {1}";
    private const string Description = "A boolean name should follow the naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private const string PatternOptionName = $"dotnet_diagnostic.{DiagnosticId}.pattern";

    private const string DefaultNamingConvention = "follow the 'can|has|is' naming convention";

    private const string CustomRegexPatternEndMessage = "match the '{0}' regex pattern";

    /// <summary>
    /// The default regex pattern.
    /// </summary>
    public const string DefaultRegexPattern = "^_?(((C|c)an)|((H|h)as)|((I|i)s))[A-Z][a-zA-Z0-9]*$";

    private static readonly DiagnosticDescriptor DiagnosticDescriptor = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        Description,
        HelpLinkUri);

    private string _regexPattern = DefaultRegexPattern;

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid boolean name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="regexPattern">The regex pattern to use during validation.</param>
    /// <returns><see langword="true"/> if the provided name is a valid boolean name; otherwise, <see langword="false"/>.</returns>
    public static bool IsBooleanNameValid(string name, string regexPattern) =>
        !string.IsNullOrWhiteSpace(name) &&
        Regex.IsMatch(name, regexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="booleanSymbol">The boolean symbol.</param>
    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol booleanSymbol)
    {
        ISymbol symbol = context.Symbol;

        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.Type.OriginalDefinition.Equals(booleanSymbol, SymbolEqualityComparer.Default) && !IsBooleanNameValid(fieldSymbol.Name, _regexPattern))
            {
                string endMessage = _regexPattern == DefaultRegexPattern
                    ? DefaultNamingConvention
                    : string.Format(CultureInfo.InvariantCulture, CustomRegexPatternEndMessage, _regexPattern);
                Diagnostic diagnostic = Diagnostic.Create(
                    DiagnosticDescriptor,
                    symbol.Locations[0],
                    fieldSymbol.Name,
                    endMessage);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type.OriginalDefinition.Equals(booleanSymbol, SymbolEqualityComparer.Default) && !IsBooleanNameValid(propertySymbol.Name, _regexPattern))
            {
                string endMessage = _regexPattern == DefaultRegexPattern
                    ? DefaultNamingConvention
                    : string.Format(CultureInfo.InvariantCulture, CustomRegexPatternEndMessage, _regexPattern);
                Diagnostic diagnostic = Diagnostic.Create(
                    DiagnosticDescriptor,
                    symbol.Locations[0],
                    propertySymbol.Name,
                    endMessage);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IParameterSymbol parameterSymbol)
        {
            if (parameterSymbol.Type.OriginalDefinition.Equals(booleanSymbol, SymbolEqualityComparer.Default) && !IsBooleanNameValid(parameterSymbol.Name, _regexPattern))
            {
                string endMessage = _regexPattern == DefaultRegexPattern
                    ? DefaultNamingConvention
                    : string.Format(CultureInfo.InvariantCulture, CustomRegexPatternEndMessage, _regexPattern);
                Diagnostic diagnostic = Diagnostic.Create(
                    DiagnosticDescriptor,
                    symbol.Locations[0],
                    parameterSymbol.Name,
                    endMessage);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }
    }

    /// <summary>
    /// Analyzes an operation.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="booleanSymbol">The boolean symbol.</param>
    private void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol booleanSymbol)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    if (localSymbol.Type.OriginalDefinition.Equals(booleanSymbol, SymbolEqualityComparer.Default) && !IsBooleanNameValid(localSymbol.Name, _regexPattern))
                    {
                        string endMessage = _regexPattern == DefaultRegexPattern
                            ? DefaultNamingConvention
                            : string.Format(CultureInfo.InvariantCulture, CustomRegexPatternEndMessage, _regexPattern);

                        Diagnostic diagnostic = Diagnostic.Create(
                            DiagnosticDescriptor,
                            localSymbol.Locations[0],
                            localSymbol.Name,
                            endMessage);
                        context.ReportDiagnostic(diagnostic);

                        break;
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
            INamedTypeSymbol? booleanSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(bool).FullName);

            if (booleanSymbol is not null)
            {
                string? regexPattern = compilationStartAnalysisContext.ReadRegexPattern(PatternOptionName, DiagnosticId);
                _regexPattern = regexPattern is not null
                    ? regexPattern
                    : DefaultRegexPattern;

                compilationStartAnalysisContext.RegisterSymbolAction(
                    symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, booleanSymbol),
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Parameter);
                compilationStartAnalysisContext.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeOperation(operationAnalysisContext, booleanSymbol),
                    OperationKind.VariableDeclarationGroup);
            }
        });
    }
}
