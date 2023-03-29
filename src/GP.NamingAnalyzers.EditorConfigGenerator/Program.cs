// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GP.NamingAnalyzers.EditorConfigGenerator;

/// <summary>
/// Contains the main execution method for the application.
/// </summary>
public static class Program
{
    private static void AnalyzerLoadFailed(object? sender, AnalyzerLoadFailureEventArgs e) =>
        throw e.Exception ?? new InvalidOperationException(e.Message);

    private static string GetDiagnosticSeverityAsString(DiagnosticSeverity diagnosticSeverity) =>
        diagnosticSeverity switch
        {
            DiagnosticSeverity.Info => "suggestion",
            DiagnosticSeverity.Error => "error",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Hidden => "hidden",
            _ => throw new InvalidOperationException("Could not convert diagnostic severity."),
        };

    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">The array of arguments.</param>
    /// <returns><c>0</c> if the execution is successful; otherwise, <c>1</c>.</returns>
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length != 2)
        {
            Console.WriteLine($"The generator was invoked with {args.Length} argument(s) instead of 2.");
            return 1;
        }

        string inputFilePath = args[0];
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"'{inputFilePath}' does not exist.");
            return 1;
        }

        string outputFilePath = args[1];
        string? outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
        if (outputDirectoryPath is null)
        {
            Console.WriteLine($"Could not extract directory path from '{outputFilePath}'");
            return 1;
        }

        AnalyzerFileReference analyzerFileReference = new(inputFilePath, AnalyzerAssemblyLoader.Instance);
        analyzerFileReference.AnalyzerLoadFailed += AnalyzerLoadFailed;
        System.Collections.Immutable.ImmutableArray<DiagnosticAnalyzer> analyzers = analyzerFileReference.GetAnalyzersForAllLanguages();

        SortedList<string, DiagnosticDescriptor> diagnosticDescriptorsById = new();
        foreach (DiagnosticAnalyzer analyzer in analyzers)
        {
            diagnosticDescriptorsById.Add(analyzer.SupportedDiagnostics[0].Id, analyzer.SupportedDiagnostics[0]);
        }

        StringBuilder editorConfigStringBuilder = new();
        foreach (DiagnosticDescriptor diagnosticDescriptor in diagnosticDescriptorsById.Values)
        {
            editorConfigStringBuilder
                .Append("# ")
                .AppendLine(diagnosticDescriptor.Description.ToString(CultureInfo.InvariantCulture))
                .Append("dotnet_diagnostic.")
                .Append(diagnosticDescriptor.Id)
                .Append(".severity = ")
                .AppendLine(GetDiagnosticSeverityAsString(diagnosticDescriptor.DefaultSeverity))
                .AppendLine();
        }

        if (!Directory.Exists(outputDirectoryPath))
        {
            Directory.CreateDirectory(outputDirectoryPath);
        }

        File.WriteAllText(outputFilePath, editorConfigStringBuilder.ToString());

        Console.WriteLine($"Created .editorconfig file in '{outputFilePath}'.");

        return 0;
    }
}
