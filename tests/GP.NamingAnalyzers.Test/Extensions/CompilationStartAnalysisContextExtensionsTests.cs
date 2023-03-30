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
    public void TryReadRegexPattern_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => CompilationStartAnalysisContextExtensions.TryReadRegexPattern(
            null!,
            string.Empty,
            string.Empty,
            out string? regexPattern);

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

    public void TryReadRegexPattern_WhenRegexPatternDoesNotExist_ShouldReturnFalse(string patternOptionName)
    {
        Dictionary<string, string> optionValuesByOptionName = new()
        {
            { "optionName", "[" }
        };

        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = CreateMockedCompilationStartAnalysisContext(optionValuesByOptionName);

        bool isRead = CompilationStartAnalysisContextExtensions.TryReadRegexPattern(
            mockedCompilationStartAnalysisContext.Object,
            patternOptionName,
            "DiagnosticId",
            out string? regexPattern);

        isRead.Should().BeFalse();
        regexPattern.Should().BeNull();
    }

    [Fact]
    public void TryReadRegexPattern_WhenRegexPatternIsInvalid_ShouldReturnTrue()
    {
        Dictionary<string, string> optionValuesByOptionName = new()
        {
            { "optionName", "[" }
        };

        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = CreateMockedCompilationStartAnalysisContext(optionValuesByOptionName);

        bool isRead = CompilationStartAnalysisContextExtensions.TryReadRegexPattern(
            mockedCompilationStartAnalysisContext.Object,
            "optionName",
            "DiagnosticId",
            out string? regexPattern);

        isRead.Should().BeTrue();
        regexPattern.Should().BeNull();
    }

    [Fact]
    public void TryReadRegexPattern_WhenRegexPatternExists_ShouldReturnTrue()
    {
        Dictionary<string, string> optionValuesByOptionName = new()
        {
            { "optionName", "optionValue" }
        };

        Mock<CompilationStartAnalysisContext> mockedCompilationStartAnalysisContext = CreateMockedCompilationStartAnalysisContext(optionValuesByOptionName);

        bool isRead = CompilationStartAnalysisContextExtensions.TryReadRegexPattern(
            mockedCompilationStartAnalysisContext.Object,
            "optionName",
            "DiagnosticId",
            out string? regexPattern);

        isRead.Should().BeTrue();
        regexPattern.Should().Be("optionValue");
    }
}
