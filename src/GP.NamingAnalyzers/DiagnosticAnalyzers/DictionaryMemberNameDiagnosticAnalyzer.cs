// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DictionaryMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0001";
    private const string Title = "Incorrect dictionary name";
    private const string MessageFormat = "Dictionary '{0}' does not follow the 'xsByY' naming convention";
    private const string Description = "A dictionary name should follow the 'xsByY' naming convention.";
    private const string Category = "Naming";
    private const string HelpLinkUri = "https://github.com/gpetrou/GP.NamingAnalyzers/tree/main/docs/GPNA0001.md";

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

    public static bool IsNamedTypeSymbolOrImplementsNamedTypeSymbolInterface(ITypeSymbol typeSymbol, INamedTypeSymbol namedTypeSymbol)
    {
        bool isNamedTypeSymbol = typeSymbol.OriginalDefinition.Equals(namedTypeSymbol, SymbolEqualityComparer.Default);
        bool implementsDictionaryInterface = typeSymbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.Equals(namedTypeSymbol, SymbolEqualityComparer.Default));

        return isNamedTypeSymbol || implementsDictionaryInterface;
    }

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
                if (IsNamedTypeSymbolOrImplementsNamedTypeSymbolInterface(fieldSymbol.Type, dictionarySymbol) && !IsDictionaryNameValid(fieldSymbol.Name))
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                if (IsNamedTypeSymbolOrImplementsNamedTypeSymbolInterface(propertySymbol.Type, dictionarySymbol) && !IsDictionaryNameValid(propertySymbol.Name))
                {
                    Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], propertySymbol.Name);
                    context.ReportDiagnostic(diagnostic);

                    return;
                }
            }

            if (symbol is IParameterSymbol parameterSymbol)
            {
                if (IsNamedTypeSymbolOrImplementsNamedTypeSymbolInterface(parameterSymbol.Type, dictionarySymbol) && !IsDictionaryNameValid(parameterSymbol.Name))
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
                        if (IsNamedTypeSymbolOrImplementsNamedTypeSymbolInterface(localSymbol.Type, dictionarySymbol) && !IsDictionaryNameValid(localSymbol.Name))
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
