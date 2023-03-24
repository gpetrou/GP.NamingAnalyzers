// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace GP.NamingAnalyzers.Test.Verifiers;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

    public static async Task VerifyAnalyzerAsync(
        string source,
        HashSet<PackageIdentity>? uniqueAdditionalPackageIdentities = null,
        Dictionary<string, string>? optionValuesByOptionName = null,
        params DiagnosticResult[] expected)
    {
        ReferenceAssemblies referenceAssemblies = ReferenceAssemblies.Default;
        if (uniqueAdditionalPackageIdentities is not null)
        {
            referenceAssemblies = referenceAssemblies.AddPackages(ImmutableArray.CreateRange(uniqueAdditionalPackageIdentities));
        }

        optionValuesByOptionName ??= new Dictionary<string, string>();

        CSharpAnalyzerVerifier<TAnalyzer>.Test test = new(optionValuesByOptionName)
        {
            ReferenceAssemblies = referenceAssemblies,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }
}
