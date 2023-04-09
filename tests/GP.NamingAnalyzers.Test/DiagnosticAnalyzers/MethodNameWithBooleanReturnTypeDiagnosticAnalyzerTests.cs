// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.MethodNameWithBooleanReturnTypeDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class MethodNameWithBooleanReturnTypeDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("GetBoolean")]
    public void IsBooleanMethodNameValid_WhenNameIsInvalid_ShouldReturnFalse(string keyValuePairName)
    {
        bool isNameValid = MethodNameWithBooleanReturnTypeDiagnosticAnalyzer.IsBooleanMethodNameValid(keyValuePairName, MethodNameWithBooleanReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("CanBeEdited")]
    [InlineData("HasBeenEdited")]
    [InlineData("IsEditing")]
    public void IsBooleanMethodNameValid_WhenNameIsValid_ShouldReturnTrue(string keyValuePairName)
    {
        bool isNameValid = MethodNameWithBooleanReturnTypeDiagnosticAnalyzer.IsBooleanMethodNameValid(keyValuePairName, MethodNameWithBooleanReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = @"";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    [Fact]
    public async Task Analyze_WhenCustomRegexPatternOptionIsInvalid_ShouldReportADiagnostic()
    {
        string sourceCode = @"
using System;
using System.Collections.Generic;

namespace N
{
    public class Example
    {
        public bool GetBoolean()
        {
            return false;
        }
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0105.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0105", DiagnosticSeverity.Error)
                .WithMessage("'[' is an invalid regex pattern")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenMembersAreNotBoolean_ShouldNotReportADiagnostic()
    {
        string sourceCode = @"
using System;
using System.Collections.Generic;

namespace N
{
    public class Example
    {
        private int IntegerField;
        private string StringProperty { get; set; }

        public float Check(List<double> listParameter)
        {
            long[] values;

            return 1.0f;
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
    }

    public static IEnumerable<object[]> MethodMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "IsValid",
                "GetBoolean",
                "Method 'GetBoolean' does not follow the 'Can|Has|Is' naming convention",
                "follow the 'Can|Has|Is' naming convention",
                31
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0105.pattern", "^.*Boolean$" } },
                "GetBoolean",
                "IsValid",
                "Method 'IsValid' does not match the '^.*Boolean$' regex pattern",
                "match the '^.*Boolean$' regex pattern",
                28
            }
        };

    [Theory]
    [MemberData(nameof(MethodMemberData))]
    public async Task Analyze_WhenMethodNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validMethodName,
        string invalidMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        public bool {validMethodName}()
        {{
            return false;
        }}

        public bool {invalidMethodName}()
        {{
            return false;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0105", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 21, 14, expectedEndColumn)
                .WithArguments(invalidMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
