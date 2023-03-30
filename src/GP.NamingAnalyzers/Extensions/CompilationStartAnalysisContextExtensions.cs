// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using GP.NamingAnalyzers.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.Extensions;

/// <summary>
/// Contains extension methods for <see cref="CompilationStartAnalysisContext"/>.
/// </summary>
public static class CompilationStartAnalysisContextExtensions
{
    /// <summary>
    /// Reads a regex pattern from an option.
    /// </summary>
    /// <param name="context">A <see cref="CompilationStartAnalysisContext"/> instance.</param>
    /// <param name="patternOptionName">The pattern option name.</param>
    /// <param name="diagnosticId">The diagnostic ID.</param>
    /// <param name="regexPattern">The value of the option if it set and valid; otherwise, <see langword="null" />.</param>
    /// <returns><see langword="true"/> if the regex pattern was successfully loaded; otherwise, <see langword="false"/>.</returns>
    public static bool TryReadRegexPattern(
        this CompilationStartAnalysisContext context,
        string patternOptionName,
        string diagnosticId,
        out string? regexPattern)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SyntaxTree syntaxTree = context.Compilation.SyntaxTrees.First();
        AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        if (options.TryGetValue(patternOptionName, out regexPattern))
        {
            if (!StringUtilities.IsValidRegexPattern(regexPattern))
            {
                Diagnostic diagnostic = Diagnostic.Create(
                    diagnosticId,
                    "Naming",
                    $"'{regexPattern}' is an invalid regex pattern",
                    DiagnosticSeverity.Error,
                    DiagnosticSeverity.Error,
                    true,
                    0);

                context.RegisterCompilationEndAction(compilationAnalysisContext =>
                {
                    compilationAnalysisContext.ReportDiagnostic(diagnostic);
                });

                regexPattern = null;
            }

            return true;
        }

        return false;
    }
}
