﻿// Copyright (c) gpetrou. All Rights Reserved.
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
/// An analyzer to validate key/value pair names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class KeyValuePairMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0003";
    private const string Title = "Incorrect key/value pair name";
    private const string MessageFormat = "Key/value pair '{0}' does not {1}";
    private const string Description = "A key/value pair name should follow the naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private const string PatternOptionName = $"dotnet_diagnostic.{DiagnosticId}.pattern";
    private const string DefaultDiagnosticMessageEnd = "follow the 'xByY' naming convention";
    private const string CustomRegexPatternMessageEnd = "match the '{0}' regex pattern";

    /// <summary>
    /// The default regex pattern.
    /// </summary>
    public const string DefaultRegexPattern = "^_?[a-zA-Z]+By[A-Z][a-zA-Z0-9]*(?<!KeyValuePair)$";

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
    /// Returns a value indicating whether the provided name is a valid key/value pair name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="regexPattern">The regex pattern to use during validation.</param>
    /// <returns><see langword="true"/> if the provided name is a valid key/value pair name; otherwise, <see langword="false"/>.</returns>
    public static bool IsKeyValuePairNameValid(string name, string regexPattern) =>
        !string.IsNullOrWhiteSpace(name) &&
        Regex.IsMatch(name, regexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">An instance of <see cref="SymbolAnalysisContext"/>.</param>
    /// <param name="keyValuePairSymbol">The key/value pair symbol.</param>
    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol keyValuePairSymbol)
    {
        ISymbol symbol = context.Symbol;
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(fieldSymbol.Name, _regexPattern))
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
            if (propertySymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(propertySymbol.Name, _regexPattern))
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
            if (parameterSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(parameterSymbol.Name, _regexPattern))
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

    /// <summary>
    /// Analyzes an operation.
    /// </summary>
    /// <param name="context">An instance of <see cref="OperationAnalysisContext"/>.</param>
    /// <param name="keyValuePairSymbol">The key/value pair symbol.</param>
    private void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol keyValuePairSymbol)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    if (localSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(localSymbol.Name, _regexPattern))
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
            INamedTypeSymbol? keyValuePairSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(KeyValuePair<,>).FullName);
            if (keyValuePairSymbol is null)
            {
                return;
            }

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
                symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, keyValuePairSymbol),
                SymbolKind.Field,
                SymbolKind.Property,
                SymbolKind.Parameter);
            compilationStartAnalysisContext.RegisterOperationAction(
                operationAnalysisContext => AnalyzeOperation(operationAnalysisContext, keyValuePairSymbol),
                OperationKind.VariableDeclarationGroup);
        });
    }
}
