// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace GP.NamingAnalyzers.Test.Verifiers;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
        private readonly Dictionary<string, string> _optionValuesByOptionName;

        public Test(Dictionary<string, string> optionValuesByOptionName)
        {
            _optionValuesByOptionName = optionValuesByOptionName;
            SolutionTransforms.Add((solution, projectId) =>
            {
                CompilationOptions? compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                compilationOptions = compilationOptions?.WithSpecificDiagnosticOptions(
                    compilationOptions?.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarningsByDiagnosticId));
                solution = solution.WithProjectCompilationOptions(projectId, compilationOptions!);

                return solution;
            });
        }

        protected override AnalyzerOptions GetAnalyzerOptions(Project project)
        {
            AnalyzerOptions options = base.GetAnalyzerOptions(project);
            return new AnalyzerOptions(options.AdditionalFiles, new CustomAnalyzerConfigOptionsProvider(_optionValuesByOptionName));
        }
    }
}
