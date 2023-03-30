// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using GP.NamingAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate dictionary names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DictionaryMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0001";
    private const string Title = "Incorrect dictionary name";
    private const string MessageFormat = "Dictionary '{0}' does not {1}";
    private const string Description = "A dictionary name should follow the naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private const string PatternOptionName = $"dotnet_diagnostic.{DiagnosticId}.pattern";
    private const string DefaultDiagnosticMessageEnd = "follow the 'xsByY' naming convention";
    private const string CustomRegexPatternMessageEnd = "match the '{0}' regex pattern";

    /// <summary>
    /// The default regex pattern.
    /// </summary>
    public const string DefaultRegexPattern = "^_?[a-zA-Z]+sBy[A-Z][a-zA-Z0-9]*(?<!Dictionary)$";

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
    private string _diagnosticMessageEnd = DefaultDiagnosticMessageEnd;

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid dictionary name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="regexPattern">The regex pattern to use during validation.</param>
    /// <returns><see langword="true"/> if the provided name is a valid dictionary name; otherwise, <see langword="false"/>.</returns>
    public static bool IsDictionaryNameValid(string name, string regexPattern) =>
        !string.IsNullOrWhiteSpace(name) &&
        Regex.IsMatch(name, regexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">An instance of <see cref="SymbolAnalysisContext"/>.</param>
    /// <param name="dictionarySymbols">The dictionary symbols.</param>
    private void AnalyzeSymbol(SymbolAnalysisContext context, List<INamedTypeSymbol> dictionarySymbols)
    {
        ISymbol symbol = context.Symbol;
        foreach (INamedTypeSymbol dictionarySymbol in dictionarySymbols)
        {
            if (symbol is IFieldSymbol fieldSymbol)
            {
                if (fieldSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(fieldSymbol.Name, _regexPattern))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        DiagnosticDescriptor,
                        symbol.Locations[0],
                        fieldSymbol.Name,
                        _diagnosticMessageEnd);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(propertySymbol.Name, _regexPattern))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        DiagnosticDescriptor,
                        symbol.Locations[0],
                        propertySymbol.Name,
                        _diagnosticMessageEnd);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IParameterSymbol parameterSymbol)
            {
                if (parameterSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(parameterSymbol.Name, _regexPattern))
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        DiagnosticDescriptor,
                        symbol.Locations[0],
                        parameterSymbol.Name,
                        _diagnosticMessageEnd);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Analyzes an operation.
    /// </summary>
    /// <param name="context">An instance of <see cref="OperationAnalysisContext"/>.</param>
    /// <param name="dictionarySymbols">The dictionary symbols.</param>
    private void AnalyzeOperation(OperationAnalysisContext context, List<INamedTypeSymbol> dictionarySymbols)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    foreach (INamedTypeSymbol dictionarySymbol in dictionarySymbols)
                    {
                        if (localSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(localSymbol.Name, _regexPattern))
                        {
                            Diagnostic diagnostic = Diagnostic.Create(
                                DiagnosticDescriptor,
                                localSymbol.Locations[0],
                                localSymbol.Name,
                                _diagnosticMessageEnd);
                            context.ReportDiagnostic(diagnostic);

                            break;
                        }
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
            List<INamedTypeSymbol> dictionarySymbols = new();
            INamedTypeSymbol? dictionaryInterfaceSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(IDictionary).FullName);
            if (dictionaryInterfaceSymbol is not null)
            {
                dictionarySymbols.Add(dictionaryInterfaceSymbol);
            }

            INamedTypeSymbol? genericDictionaryInterfaceSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(IDictionary<,>).FullName);
            if (genericDictionaryInterfaceSymbol is not null)
            {
                dictionarySymbols.Add(genericDictionaryInterfaceSymbol);
            }

            if (dictionarySymbols.Count > 0)
            {
                bool isRead = compilationStartAnalysisContext.TryReadRegexPattern(PatternOptionName, DiagnosticId, out string? regexPattern);
                if (isRead && regexPattern is null)
                {
                    return;
                }

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

                compilationStartAnalysisContext.RegisterSymbolAction(
                    symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, dictionarySymbols),
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Parameter);
                compilationStartAnalysisContext.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeOperation(operationAnalysisContext, dictionarySymbols),
                    OperationKind.VariableDeclarationGroup);
            }
        });
    }
}
