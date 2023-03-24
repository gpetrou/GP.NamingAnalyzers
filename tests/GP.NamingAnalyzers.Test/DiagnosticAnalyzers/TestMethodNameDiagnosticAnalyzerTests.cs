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
        bool isNameValid = TestMethodNameDiagnosticAnalyzer.IsTestMethodNameValid(testMethodName);

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
        bool isNameValid = TestMethodNameDiagnosticAnalyzer.IsTestMethodNameValid(testMethodName);

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

    [Fact]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithXUnitFactAttribute_ShouldReturnOneDiagnostic()
    {
        const string sourceCode = @"
using Xunit;

namespace WrongTestMethodNameExample
{
    public class UnitTests
    {
        [Fact]
        public void PassingTest()
        {
        }

        [Fact]
        public void GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException()
        {
        }
    }
}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage("Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention")
                .WithSpan(9, 21, 9, 32)
                .WithArguments("PassingTest")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            null,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithNUnitTestAttribute_ShouldReturnOneDiagnostic()
    {
        const string sourceCode = @"
using NUnit.Framework;

namespace WrongTestMethodNameExample
{
    public class UnitTests
    {
        [Test]
        public void PassingTest()
        {
        }

        [Test]
        public void GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException()
        {
        }
    }
}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage("Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention")
                .WithSpan(9, 21, 9, 32)
                .WithArguments("PassingTest")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            null,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithMSTestMethodAttribute_ShouldReturnOneDiagnostic()
    {
        const string sourceCode = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WrongTestMethodNameExample
{
    public class UnitTests
    {
        [TestMethod]
        public void PassingTest()
        {
        }

        [TestMethod]
        public void GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException()
        {
        }
    }
}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage("Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention")
                .WithSpan(9, 21, 9, 32)
                .WithArguments("PassingTest")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            null,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithXUnitTheoryAttribute_ShouldReturnOneDiagnostic()
    {
        const string sourceCode = @"
using Xunit;

namespace WrongTestMethodNameExample
{
    public class UnitTests
    {
        [Theory]
        [InlineData(1)]
        public void PassingTest(int value)
        {
        }

        [Theory]
        [InlineData(1)]
        public void GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException(int index)
        {
        }
    }
}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage("Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention")
                .WithSpan(10, 21, 10, 32)
                .WithArguments("PassingTest")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            null,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task TestMethodNameAnalyzer_WhenAnalyzingTestMethodWithNUnitTheoryAttribute_ShouldReturnOneDiagnostic()
    {
        const string sourceCode = @"
using NUnit.Framework;

namespace WrongTestMethodNameExample
{
    public class UnitTests
    {
        [Theory]
        public void PassingTest(int value)
        {
        }

        [Theory]
        public void GetValues_WhenInputIsInvalid_ShouldThrowInvalidOperationException(int index)
        {
        }
    }
}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0101", DiagnosticSeverity.Warning)
                .WithMessage("Test method 'PassingTest' does not follow the 'MethodUnderTest_When_Should' naming convention")
                .WithSpan(9, 21, 9, 32)
                .WithArguments("PassingTest")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            _uniqueAdditionalPackageIdentities,
            null,
            expectedDiagnosticResults);
    }
}
