﻿// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using GP.NamingAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate the name of methods that return a set.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodNameWithSetReturnTypeDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0104";
    private const string Title = "Incorrect name of method that returns a set";
    private const string MessageFormat = "Method '{0}' does not {1}";
    private const string Description = "A method that returns a set should follow the naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = $"https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/{DiagnosticId}.md";

    private const string PatternOptionName = $"dotnet_diagnostic.{DiagnosticId}.pattern";
    private const string DefaultDiagnosticMessageEnd = "follow the 'GetUniqueXs' naming convention";
    private const string CustomRegexPatternMessageEnd = "match the '{0}' regex pattern";

    /// <summary>
    /// The default regex pattern.
    /// </summary>
    public const string DefaultRegexPattern = "^GetUnique[A-Z][a-zA-Z0-9]*s(?<!Set)$";

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
    /// Returns a value indicating whether the provided name of a method that returns a set is valid.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="regexPattern">The regex pattern to use during validation.</param>
    /// <returns><see langword="true"/> if the provided name of a method that returns a set is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsSetMethodNameValid(string name, string regexPattern) =>
        !string.IsNullOrWhiteSpace(name) &&
        Regex.IsMatch(name, regexPattern, RegexOptions.Compiled);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">An instance of <see cref="SymbolAnalysisContext"/>.</param>
    /// <param name="setInterfaceSymbol">The set interface symbol.</param>
    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol setInterfaceSymbol)
    {
        ISymbol symbol = context.Symbol;
        if (symbol is IMethodSymbol methodSymbol &&
            methodSymbol.MethodKind == MethodKind.Ordinary &&
            !methodSymbol.IsOverride &&
            methodSymbol.ReturnType.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(setInterfaceSymbol) &&
            !IsSetMethodNameValid(methodSymbol.Name, _regexPattern))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                DiagnosticDescriptor,
                symbol.Locations[0],
                methodSymbol.Name,
                _diagnosticMessageEnd);
            context.ReportDiagnostic(diagnostic);
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
            INamedTypeSymbol? setInterfaceSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(ISet<>).FullName);
            if (setInterfaceSymbol is null)
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
                symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, setInterfaceSymbol),
                SymbolKind.Method);
        });
    }
}
