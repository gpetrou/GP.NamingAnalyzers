// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace GP.NamingAnalyzers.EditorConfigGenerator;

/// <summary>
/// Handles loading analyzer assemblies and their dependencies.
/// </summary>
public sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
    /// <summary>
    /// The default instance of <see cref="AnalyzerAssemblyLoader"/>.
    /// </summary>
    public static readonly IAnalyzerAssemblyLoader Instance = new AnalyzerAssemblyLoader();

    private AnalyzerAssemblyLoader()
    {
    }

    /// <inheritdoc/>
    public void AddDependencyLocation(string fullPath)
    {
    }

    /// <inheritdoc/>
    public Assembly LoadFromPath(string fullPath) =>
        Assembly.LoadFrom(fullPath);
}
