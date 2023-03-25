// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.MockedMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class MockedMemberNameDiagnosticAnalyzerTests
{
    private static readonly HashSet<PackageIdentity> _uniqueAdditionalPackageIdentities = new()
    {
        new PackageIdentity("Moq", "4.18.4"),
        new PackageIdentity("NSubstitute", "5.0.0"),
        new PackageIdentity("FakeItEasy", "7.3.1")
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("item")]
    [InlineData("MockItem")]
    [InlineData("mocked")]
    public void IsMockedMemberNameValid_WhenNameIsInvalid_ShouldReturnFalse(string testMethodName)
    {
        bool isNameValid = MockedMemberNameDiagnosticAnalyzer.IsMockedMemberNameValid(testMethodName, MockedMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("MockedMember")]
    [InlineData("mockedItem")]
    public void IsMockedMemberNameValid_WhenNameIsValid_ShouldReturnTrue(string testMethodName)
    {
        bool isNameValid = MockedMemberNameDiagnosticAnalyzer.IsMockedMemberNameValid(testMethodName, MockedMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = "";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    [Fact]
    public async Task Analyze_WhenThereIsNoTestAttribute_ShouldNotReturnADiagnostic()
    {
        const string sourceCode = @"
namespace NoMockedMemberNameExample
{
    public class UnitTests
    {
        public void TestMethodWithoutMockedMember()
        {
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities);
    }

    public static IEnumerable<object[]> VariableMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "Moq",
                "new Mock",
                "mockedItem",
                "fakedItem",
                "Mocked member 'fakedItem' does not follow the 'mocked' naming convention",
                "follow the 'mocked' naming convention",
                17,
                26
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0005.pattern", "^faked.*" } },
                "Moq",
                "new Mock",
                "fakedItem",
                "mockedItem",
                "Mocked member 'mockedItem' does not match the '^faked.*' regex pattern",
                "match the '^faked.*' regex pattern",
                17,
                27
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "NSubstitute",
                "Substitute.For",
                "mockedItem",
                "fakedItem",
                "Mocked member 'fakedItem' does not follow the 'mocked' naming convention",
                "follow the 'mocked' naming convention",
                17,
                26
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0005.pattern", "^faked.*" } },
                "NSubstitute",
                "Substitute.For",
                "fakedItem",
                "mockedItem",
                "Mocked member 'mockedItem' does not match the '^faked.*' regex pattern",
                "match the '^faked.*' regex pattern",
                17,
                27
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "FakeItEasy",
                "A.Fake",
                "mockedItem",
                "fakedItem",
                "Mocked member 'fakedItem' does not follow the 'mocked' naming convention",
                "follow the 'mocked' naming convention",
                17,
                26
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0005.pattern", "^faked.*" } },
                "FakeItEasy",
                "A.Fake",
                "fakedItem",
                "mockedItem",
                "Mocked member 'mockedItem' does not match the '^faked.*' regex pattern",
                "match the '^faked.*' regex pattern",
                17,
                27
            }
        };

    [Theory]
    [MemberData(nameof(VariableMemberData))]
    public async Task Analyze_WhenMockedMemberVariableNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string namespaceUsing,
        string substituteMethod,
        string validVariableName,
        string invalidVariableName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedStartColumn,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using {namespaceUsing};

namespace N
{{
    interface IItem
    {{
    }}

    public class Example
    {{
        public void Check()
        {{
            var {validVariableName} = {substituteMethod}<IItem>();
        }}

        public void CheckOnceMore()
        {{
            var {invalidVariableName} = {substituteMethod}<IItem>();
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0005", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(20, expectedStartColumn, 20, expectedEndColumn)
                .WithArguments(invalidVariableName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
