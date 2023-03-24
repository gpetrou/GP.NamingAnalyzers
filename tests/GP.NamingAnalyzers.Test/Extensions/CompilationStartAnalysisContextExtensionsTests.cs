// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using System.Collections.Immutable;
using FluentAssertions;
using GP.NamingAnalyzers.Extensions;
using GP.NamingAnalyzers.Test.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq;
using Xunit;

namespace GP.NamingAnalyzers.Test.Extensions;

public sealed class CompilationStartAnalysisContextExtensionsTests
{
    [Fact]
    public void ReadRegexPattern_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => CompilationStartAnalysisContextExtensions.ReadRegexPattern(
            null!,
            string.Empty,
            string.Empty);

        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'context')");
    }

    private static Mock<CompilationStartAnalysisContext> CreateMockedCompilationStartAnalysisContext(Dictionary<string, string> optionValuesByOptionName)
    {
        CompilationUnitSyntax compilationUnitSyntax = SyntaxFactory.CompilationUnit();
        SyntaxTree syntaxTree = SyntaxFactory.SyntaxTree(compilationUnitSyntax);
        CSharpCompilation compilation = CSharpCompilation.Create("test");
        compilation = compilation.AddSyntaxTrees(syntaxTree);
        CustomAnalyzerConfigOptions customAnalyzerConfigOptions = new(optionValuesByOptionName);
        Mock<AnalyzerConfigOptionsProvider> mockedAnalyzerConfigOptionsProvider = new(MockBehavior.Strict);
        mockedAnalyzerConfigOptionsProvider.Setup(p => p.GetOptions(syntaxTree)).Returns(customAnalyzerConfigOptions);
        AnalyzerOptions analyzerOptions = new(ImmutableArray<AdditionalText>.Empty, mockedAnalyzerConfigOptionsProvider.Object);
        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = new(compilation, analyzerOptions, CancellationToken.None);

        return mockedCompilationStartAnalysisContext;
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("InvalidOptionName")]
    [InlineData("optionName")]
    public void ReadRegexPattern_WhenRegexPatternDoesNotExistOrIsInvalid_ShouldReturnNull(string patternOptionName)
    {
        Dictionary<string, string> optionValuesByOptionName = new()
        {
            { "optionName", "[" }
        };

        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = CreateMockedCompilationStartAnalysisContext(optionValuesByOptionName);

        string? result = CompilationStartAnalysisContextExtensions.ReadRegexPattern(
            mockedCompilationStartAnalysisContext.Object,
            patternOptionName,
            "DiagnosticId");

        result.Should().BeNull();
    }

    [Fact]
    public void ReadRegexPattern_WhenRegexPatternExists_ShouldReturnRegexPatternValue()
    {
        Dictionary<string, string> optionValuesByOptionName = new()
        {
            { "optionName", "optionValue" }
        };

        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = CreateMockedCompilationStartAnalysisContext(optionValuesByOptionName);

        string? result = CompilationStartAnalysisContextExtensions.ReadRegexPattern(
            mockedCompilationStartAnalysisContext.Object,
            "optionName",
            string.Empty);

        result.Should().Be("optionValue");
    }
}
