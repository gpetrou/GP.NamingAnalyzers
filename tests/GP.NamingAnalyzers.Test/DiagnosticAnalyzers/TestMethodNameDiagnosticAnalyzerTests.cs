// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.TestMethodNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class TestMethodNameDiagnosticAnalyzerTests
{
    private static readonly HashSet<PackageIdentity> _uniqueAdditionalPackageIdentities = new()
    {
        new PackageIdentity("xunit", "2.4.2"),
        new PackageIdentity("Nunit", "3.13.3"),
        new PackageIdentity("MSTest.TestFramework", "3.0.2")
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("TestNameWithoutUnderscores")]
    [InlineData("Mtd_When_Should")]
    [InlineData("Method_Whn_Should")]
    [InlineData("Method_When_Shd")]
    [InlineData("Method_WhnCondition_Should")]
    public void IsTestMethodNameValid_WhenNameIsInvalid_ShouldReturnFalse(string testMethodName)
    {
        bool isNameValid = TestMethodNameDiagnosticAnalyzer.IsTestMethodNameValid(testMethodName, TestMethodNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("MethodName_WhenCondition_ShouldThrowException")]
    [InlineData("MethodName_WhenCondition_ShouldNotThrowException")]
    [InlineData("MethodName_WhenCondition_ShouldReturnTrue")]
    [InlineData("MethodName_WhenCondition_ShouldNotReturnFalse")]
    [InlineData("IsValidTestName_WhenHavingIncorrectName_ShouldReturnFalse")]
    [InlineData("IsValidTestName_WhenHavingCorrectName_ShouldReturnTrue")]
    public void IsTestMethodNameValid_WhenNameIsValid_ShouldReturnTrue(string testMethodName)
    {
        bool isNameValid = TestMethodNameDiagnosticAnalyzer.IsTestMethodNameValid(testMethodName, TestMethodNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = "";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    [Fact]
    public async Task Analyze_WhenCustomRegexPatternOptionIsInvalid_ShouldReportADiagnostic()
    {
        string sourceCode = @"
using Xunit;

namespace NonTestMethodNameExample
{
    public class UnitTests
    {
        [Fact]
        public void NonTestMethod()
        {
        }
    }
}";

        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0101.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Error)
                .WithMessage("'[' is an invalid regex pattern")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenThereIsNoTestAttribute_ShouldNotReturnADiagnostic()
    {
        const string sourceCode = @"
using Xunit;

namespace NonTestMethodNameExample
{
    public class UnitTests
    {
        public void NonTestMethod()
        {
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities);
    }

    public static IEnumerable<object[]> TestMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException",
                "PassingTest",
                "Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention",
                "follow the 'can|has|is' naming convention",
                32
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0101.pattern", "^.*Test" } },
                "PassingTest",
                "GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException",
                "Test method 'GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException' does not match the '^.*Test' regex pattern",
                "match the '^.*Test' regex pattern",
                86
            }
        };

    [Theory]
    [MemberData(nameof(TestMemberData))]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithXUnitFactAttribute_ShouldReturnOneDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validTestMethodName,
        string invalidTestMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using Xunit;

namespace WrongTestMethodNameExample
{{
    public class UnitTests
    {{
        [Fact]
        public void {validTestMethodName}()
        {{
        }}

        [Fact]
        public void {invalidTestMethodName}()
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 21, 14, expectedEndColumn)
                .WithArguments(invalidTestMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Theory]
    [MemberData(nameof(TestMemberData))]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithNUnitTestAttribute_ShouldReturnOneDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validTestMethodName,
        string invalidTestMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using NUnit.Framework;

namespace WrongTestMethodNameExample
{{
    public class UnitTests
    {{
        [Test]
        public void {validTestMethodName}()
        {{
        }}

        [Test]
        public void {invalidTestMethodName}()
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 21, 14, expectedEndColumn)
                .WithArguments(invalidTestMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Theory]
    [MemberData(nameof(TestMemberData))]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithMSTestMethodAttribute_ShouldReturnOneDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validTestMethodName,
        string invalidTestMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WrongTestMethodNameExample
{{
    public class UnitTests
    {{
        [TestMethod]
        public void {validTestMethodName}()
        {{
        }}

        [TestMethod]
        public void {invalidTestMethodName}()
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 21, 14, expectedEndColumn)
                .WithArguments(invalidTestMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Theory]
    [MemberData(nameof(TestMemberData))]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithXUnitTheoryAttribute_ShouldReturnOneDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validTestMethodName,
        string invalidTestMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using Xunit;

namespace WrongTestMethodNameExample
{{
    public class UnitTests
    {{
        [Theory]
        [InlineData(1)]
        public void {validTestMethodName}(int value)
        {{
        }}

        [Theory]
        [InlineData(1)]
        public void {invalidTestMethodName}(int index)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(16, 21, 16, expectedEndColumn)
                .WithArguments(invalidTestMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Theory]
    [MemberData(nameof(TestMemberData))]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithNUnitTheoryAttribute_ShouldReturnOneDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validTestMethodName,
        string invalidTestMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using NUnit.Framework;

namespace WrongTestMethodNameExample
{{
    public class UnitTests
    {{
        [Theory]
        public void {validTestMethodName}(int value)
        {{
        }}

        [Theory]
        public void {invalidTestMethodName}(int index)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 21, 14, expectedEndColumn)
                .WithArguments(invalidTestMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
