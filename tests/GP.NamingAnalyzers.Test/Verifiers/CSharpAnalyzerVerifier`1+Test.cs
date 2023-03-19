// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace GP.NamingAnalyzers.Test;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    private class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
        public Test() =>
            SolutionTransforms.Add((solution, projectId) =>
            {
                Microsoft.CodeAnalysis.CompilationOptions? compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                compilationOptions = compilationOptions?.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                return solution;
            });
    }
}
