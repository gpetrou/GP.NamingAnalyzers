// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.MethodNameWithSetReturnTypeDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class MethodNameWithSetReturnTypeDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("GetMySet")]
    [InlineData("GetItems")]
    [InlineData("GetUniqueItemsSet")]
    public void IsSetMethodNameValid_WhenNameIsInvalid_ShouldReturnFalse(string setName)
    {
        bool isNameValid = MethodNameWithSetReturnTypeDiagnosticAnalyzer.IsSetMethodNameValid(setName, MethodNameWithSetReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GetUniqueXs")]
    [InlineData("GetUniqueItems")]
    public void IsSetMethodNameValid_WhenNameIsValid_ShouldReturnTrue(string setName)
    {
        bool isNameValid = MethodNameWithSetReturnTypeDiagnosticAnalyzer.IsSetMethodNameValid(setName, MethodNameWithSetReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

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
        public HashSet<string> GetSet()
        {
            return new HashSet<string>();
        }
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0104.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0104", DiagnosticSeverity.Error)
                .WithMessage("'[' is an invalid regex pattern")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenMembersAreNotSets_ShouldNotReportADiagnostic()
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
            bool booleanVariable = true;

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
                "ISet<string>",
                "GetUniqueItems",
                "GetHashSet",
                "Method 'GetHashSet' does not follow the 'GetUniqueXs' naming convention",
                "follow the 'GetUniqueXs' naming convention",
                29,
                39
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0104.pattern", "^.*Set" } },
                "ISet<string>",
                "GetHashSet",
                "GetUniqueItems",
                "Method 'GetUniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                29,
                43
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "HashSet<string>",
                "GetUniqueItems",
                "GetHashSet",
                "Method 'GetHashSet' does not follow the 'GetUniqueXs' naming convention",
                "follow the 'GetUniqueXs' naming convention",
                32,
                42
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0104.pattern", "^.*Set" } },
                "HashSet<string>",
                "GetHashSet",
                "GetUniqueItems",
                "Method 'GetUniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                32,
                46
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableHashSet<string>",
                "GetUniqueItems",
                "GetHashSet",
                "Method 'GetHashSet' does not follow the 'GetUniqueXs' naming convention",
                "follow the 'GetUniqueXs' naming convention",
                41,
                51
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0104.pattern", "^.*Set" } },
                "ImmutableHashSet<string>",
                "GetHashSet",
                "GetUniqueItems",
                "Method 'GetUniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                41,
                55
            }
        };

    [Theory]
    [MemberData(nameof(MethodMemberData))]
    public async Task Analyze_WhenMethodNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string returnType,
        string validMethodName,
        string invalidMethodName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedStartColumn,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace N
{{
    public class Example
    {{
        public {returnType} {validMethodName}()
        {{
            return null;
        }}

        public {returnType} {invalidMethodName}()
        {{
            return null;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0104", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(15, expectedStartColumn, 15, expectedEndColumn)
                .WithArguments(invalidMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
