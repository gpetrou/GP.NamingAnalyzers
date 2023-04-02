// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class MethodNameWithDictionaryReturnTypeDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("GetMyDictionary")]
    [InlineData("GetItems")]
    [InlineData("GetXByY")]
    [InlineData("GetXsByYDictionary")]
    public void IsDictionaryMethodNameValid_WhenNameIsInvalid_ShouldReturnFalse(string dictionaryName)
    {
        bool isNameValid = MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer.IsDictionaryMethodNameValid(dictionaryName, MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GetXsByY")]
    [InlineData("GetItemsById")]
    public void IsDictionaryMethodNameValid_WhenNameIsValid_ShouldReturnTrue(string dictionaryName)
    {
        bool isNameValid = MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer.IsDictionaryMethodNameValid(dictionaryName, MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer.DefaultRegexPattern);

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
        public Dictionary<int, string> GetDictionary()
        {
            return new Dictionary<int, string>();
        }
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0103.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0103", DiagnosticSeverity.Error)
                .WithMessage("'[' is an invalid regex pattern")
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenMembersAreNotDictionaries_ShouldNotReportADiagnostic()
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
                "IDictionary",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                28,
                41
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "IDictionary",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                28,
                40
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary<int, string>",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                41,
                54
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "IDictionary<int, string>",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                41,
                53
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "Dictionary<int, string>",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                40,
                53
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "Dictionary<int, string>",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                40,
                52
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ConcurrentDictionary<int, string>",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                50,
                63
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "ConcurrentDictionary<int, string>",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                50,
                62
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableDictionary<int, string>",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                49,
                62
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "ImmutableDictionary<int, string>",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                49,
                61
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ReadOnlyDictionary<int, string>",
                "GetItemsById",
                "GetDictionary",
                "Method 'GetDictionary' does not follow the 'GetXsByY' naming convention",
                "follow the 'GetXsByY' naming convention",
                48,
                61
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0103.pattern", "^.*Dictionary$" } },
                "ReadOnlyDictionary<int, string>",
                "GetDictionary",
                "GetItemsById",
                "Method 'GetItemsById' does not match the '^.*Dictionary$' regex pattern",
                "match the '^.*Dictionary$' regex pattern",
                48,
                60
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            new DiagnosticResult("GPNA0103", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(18, expectedStartColumn, 18, expectedEndColumn)
                .WithArguments(invalidMethodName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
