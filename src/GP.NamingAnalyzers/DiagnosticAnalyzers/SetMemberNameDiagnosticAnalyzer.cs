// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using GP.NamingAnalyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace GP.NamingAnalyzers.DiagnosticAnalyzers;

/// <summary>
/// An analyzer to validate set names.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SetMemberNameDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GPNA0002";
    private const string Title = "Incorrect set name";
    private const string MessageFormat = "Set '{0}' does not follow the 'uniqueXs' naming convention";
    private const string Description = "A set name should follow the 'uniqueXs' naming convention.";
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

    private static readonly Regex SetNameRegex = new("^((_?u)|U)nique[A-Z][a-zA-Z0-9]*s$", RegexOptions.Compiled);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

    /// <summary>
    /// Returns a value indicating whether the provided name is a valid set name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <returns><see langword="true"/> if the provided name is a valid set name; otherwise, <see langword="false"/>.</returns>
    public static bool IsSetNameValid(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        name.IndexOf("Set", StringComparison.OrdinalIgnoreCase) == -1 &&
        SetNameRegex.IsMatch(name);

    /// <summary>
    /// Analyzes a symbol.
    /// </summary>
    /// <param name="context">An instance of <see cref="SymbolAnalysisContext"/>.</param>
    /// <param name="setInterfaceSymbol">The set interface symbol.</param>
    private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol setInterfaceSymbol)
    {
        ISymbol symbol = context.Symbol;
        if (symbol is IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(setInterfaceSymbol) && !IsSetNameValid(fieldSymbol.Name))
            {
                Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IPropertySymbol propertySymbol)
        {
            if (propertySymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(setInterfaceSymbol) && !IsSetNameValid(propertySymbol.Name))
            {
                Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, symbol.Locations[0], propertySymbol.Name);
                context.ReportDiagnostic(diagnostic);

                return;
            }
        }

        if (symbol is IParameterSymbol parameterSymbol)
        {
            if (parameterSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(setInterfaceSymbol) && !IsSetNameValid(parameterSymbol.Name))
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
    /// <param name="context">An instance of <see cref="OperationAnalysisContext"/>.</param>
    /// <param name="setInterfaceSymbol">The set interface symbol.</param>
    private static void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol setInterfaceSymbol)
    {
        if (context.Operation is IVariableDeclarationGroupOperation variableDeclarationGroupOperation)
        {
            foreach (IVariableDeclarationOperation variableDeclarationOperation in variableDeclarationGroupOperation.Declarations)
            {
                foreach (ILocalSymbol localSymbol in variableDeclarationOperation.GetDeclaredVariables())
                {
                    if (localSymbol.Type.HasOriginalDefinitionOrImplementsNamedTypeSymbolInterface(setInterfaceSymbol) && !IsSetNameValid(localSymbol.Name))
                    {
                        Diagnostic diagnostic = Diagnostic.Create(DiagnosticDescriptor, localSymbol.Locations[0], localSymbol.Name);
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
            INamedTypeSymbol? setInterfaceSymbol = compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeof(ISet<>).FullName);
            if (setInterfaceSymbol is not null)
            {
                compilationStartAnalysisContext.RegisterSymbolAction(
                    symbolAnalysisContext => AnalyzeSymbol(symbolAnalysisContext, setInterfaceSymbol),
                    SymbolKind.Field,
                    SymbolKind.Property,
                    SymbolKind.Parameter);
                compilationStartAnalysisContext.RegisterOperationAction(
                    operationAnalysisContext => AnalyzeOperation(operationAnalysisContext, setInterfaceSymbol),
                    OperationKind.VariableDeclarationGroup);
            }
        });
    }
}
