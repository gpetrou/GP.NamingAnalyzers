// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections;
using System.Collections.Immutable;
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
    private const string MessageFormat = "Dictionary '{0}' does not follow the 'xsByY' naming convention";
    private const string Description = "A dictionary name should follow the 'xsByY' naming convention.";
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

    private static readonly Regex DictionaryNameRegex = new("^_?[a-zA-Z]+sBy[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid dictionary name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns><see langword="true"/> if the provided name is a valid dictionary name; otherwise, <see langword="false"/>.</returns>
    public static bool IsDictionaryNameValid(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        name.IndexOf("Dictionary", StringComparison.OrdinalIgnoreCase) == -1 &&
        DictionaryNameRegex.IsMatch(name);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="dictionarySymbols">The dictionary symbols.</param>
    private static void AnalyzeSymbol(SymbolAnalysisContext context, List<INamedTypeSymbol> dictionarySymbols)
    {
        ISymbol symbol = context.Symbol;
        foreach (INamedTypeSymbol dictionarySymbol in dictionarySymbols)
        {
            if (symbol is IFieldSymbol fieldSymbol)
            {
                if (fieldSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(fieldSymbol.Name))
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(propertySymbol.Name))
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], propertySymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IParameterSymbol parameterSymbol)
            {
                if (parameterSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(parameterSymbol.Name))
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], parameterSymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Analyzes an operation.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="dictionarySymbols">The dictionary symbols.</param>
    private static void AnalyzeOperation(OperationAnalysisContext context, List<INamedTypeSymbol> dictionarySymbols)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    foreach (INamedTypeSymbol dictionarySymbol in dictionarySymbols)
                    {
                        if (localSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(dictionarySymbol) && !IsDictionaryNameValid(localSymbol.Name))
                        {
                            Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, localSymbol.Locations[0], localSymbol.Name);
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
