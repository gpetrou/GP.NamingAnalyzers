// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Get")]
    [InlineData("GetMyKeyValuePair")]
    [InlineData("GetXByYKeyValuePair")]
    public void IsKeyValuePairNameValid_WhenNameIsInvalid_ShouldReturnFalse(string keyValuePairName)
    {
        bool isNameValid = MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer.IsKeyValuePairMethodNameValid(keyValuePairName, MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GetXByY")]
    [InlineData("GetItemById")]
    public void IsKeyValuePairNameValid_WhenNameIsValid_ShouldReturnTrue(string keyValuePairName)
    {
        bool isNameValid = MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer.IsKeyValuePairMethodNameValid(keyValuePairName, MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

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
        public KeyValuePair<int, string> GetKeyValuePair()
        {
            return new KeyValuePair<int, string>();
        }
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0102.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0102", DiagnosticSeverity.Error)
                .WithMessage("'[' is an invalid regex pattern")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenMembersAreNotKeyValuePairs_ShouldNotReportADiagnostic()
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
            bool isEnabled = true;

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
                "GetItemById",
                "GetKeyValuePair",
                "Method 'GetKeyValuePair' does not follow the 'GetXByY' naming convention",
                "follow the 'GetXByY' naming convention",
                57
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0102.pattern", "^.*KeyValuePair$" } },
                "GetKeyValuePair",
                "GetItemById",
                "Method 'GetItemById' does not match the '^.*KeyValuePair$' regex pattern",
                "match the '^.*KeyValuePair$' regex pattern",
                53
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
        public KeyValuePair<int, string> {validMethodName}()
        {{
            return new KeyValuePair<int, string>();
        }}

        public KeyValuePair<int, string> {invalidMethodName}()
        {{
            return new KeyValuePair<int, string>();
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0102", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, 42, 14, expectedEndColumn)
                .WithArguments(invalidMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
