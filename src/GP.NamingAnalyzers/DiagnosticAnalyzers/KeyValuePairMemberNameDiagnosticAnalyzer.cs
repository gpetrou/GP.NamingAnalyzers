// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
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
    private const string MessageFormat = "Key/value pair '{0}' does not follow the 'xByY' naming convention";
    private const string Description = "A key/value pair name should follow the 'xByY' naming convention.";
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

    private static readonly Regex KeyValuePairNameRegex = new("^_?[a-zA-Z]+By[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid key/value pair name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns><see langword="true"/> if the provided name is a valid key/value pair name; otherwise, <see langword="false"/>.</returns>
    public static bool IsKeyValuePairNameValid(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        name.IndexOf("KeyValuePair", StringComparison.OrdinalIgnoreCase) == -1 &&
        KeyValuePairNameRegex.IsMatch(name);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="keyValuePairSymbol">The key/value pair symbol.</param>
    private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol keyValuePairSymbol)
    {
        ISymbol symbol = context.Symbol;
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(fieldSymbol.Name))
            {
                Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(propertySymbol.Name))
            {
                Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IParameterSymbol parameterSymbol)
        {
            if (parameterSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(parameterSymbol.Name))
            {
                Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], parameterSymbol.Name);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }
    }

    /// <summary>
    /// Analyzes an operation.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    /// <param name="keyValuePairSymbol">The key/value pair symbol.</param>
    private static void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol keyValuePairSymbol)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    if (localSymbol.Type.OriginalDefinition.Equals(keyValuePairSymbol, SymbolEqualityComparer.Default) && !IsKeyValuePairNameValid(localSymbol.Name))
                    {
                        Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, localSymbol.Locations[0], localSymbol.Name);
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

            if (keyValuePairSymbol is not null)
            {
                compilationStartAnalysisContext.RegisterSymbolAction(
                    symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, keyValuePairSymbol),
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Parameter);
                compilationStartAnalysisContext.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeOperation(operationAnalysisContext, keyValuePairSymbol),
                    OperationKind.VariableDeclarationGroup);
            }
        });
    }
}
