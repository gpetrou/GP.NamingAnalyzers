// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.KeyValuePairMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class KeyValuePairMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("_myKeyValuePair")]
    [InlineData("items")]
    [InlineData("xByYKeyValuePair")]
    public void IsKeyValuePairNameValid_WhenNameIsInvalid_ShouldReturnFalse(string keyValuePairName)
    {
        bool isNameValid = KeyValuePairMemberNameDiagnosticAnalyzer.IsKeyValuePairNameValid(keyValuePairName, KeyValuePairMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("xByY")]
    [InlineData("itemById")]
    public void IsKeyValuePairNameValid_WhenNameIsValid_ShouldReturnTrue(string keyValuePairName)
    {
        bool isNameValid = KeyValuePairMemberNameDiagnosticAnalyzer.IsKeyValuePairNameValid(keyValuePairName, KeyValuePairMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

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
        private KeyValuePair<int, string> _myKeyValuePair;
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0003.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Error)
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

    public static IEnumerable<object[]> FieldMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "_itemById",
                "_myKeyValuePair",
                "Key/value pair '_myKeyValuePair' does not follow the 'xByY' naming convention",
                "follow the 'xByY' naming convention",
                58
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0003.pattern", "^.*KeyValuePair$" } },
                "_myKeyValuePair",
                "_itemById",
                "Key/value pair '_itemById' does not match the '^.*KeyValuePair$' regex pattern",
                "match the '^.*KeyValuePair$' regex pattern",
                52
            }
        };

    [Theory]
    [MemberData(nameof(FieldMemberData))]
    public async Task Analyze_WhenKeyValuePairFieldNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validFieldName,
        string invalidFieldName,
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
        private KeyValuePair<int, string> {validFieldName};
        private KeyValuePair<int, string> {invalidFieldName};
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(10, 43, 10, expectedEndColumn)
                .WithArguments(invalidFieldName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> PropertiesMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "ItemById",
                "MyKeyValuePair",
                "Key/value pair 'MyKeyValuePair' does not follow the 'xByY' naming convention",
                "follow the 'xByY' naming convention",
                56
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0003.pattern", "^.*KeyValuePair$" } },
                "MyKeyValuePair",
                "ItemById",
                "Key/value pair 'ItemById' does not match the '^.*KeyValuePair$' regex pattern",
                "match the '^.*KeyValuePair$' regex pattern",
                50
            }
        };

    [Theory]
    [MemberData(nameof(PropertiesMemberData))]
    public async Task Analyze_WhenKeyValuePairPropertyNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validPropertyName,
        string invalidPropertyName,
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
        public KeyValuePair<int, string> {validPropertyName} {{ get; set; }}
        public KeyValuePair<int, string> {invalidPropertyName} {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(10, 42, 10, expectedEndColumn)
                .WithArguments(invalidPropertyName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> ParametersMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "itemById",
                "myKeyValuePair",
                "Key/value pair 'myKeyValuePair' does not follow the 'xByY' naming convention",
                "follow the 'xByY' naming convention",
                75
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0003.pattern", "^.*KeyValuePair$" } },
                "myKeyValuePair",
                "itemById",
                "Key/value pair 'itemById' does not match the '^.*KeyValuePair$' regex pattern",
                "match the '^.*KeyValuePair$' regex pattern",
                69
            }
        };

    [Theory]
    [MemberData(nameof(ParametersMemberData))]
    public async Task Analyze_WhenKeyValuePairParameterNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validParameterName,
        string invalidParameterName,
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
        public void Check(KeyValuePair<int, string> {validParameterName})
        {{
        }}

        public void CheckOnceMore(KeyValuePair<int, string> {invalidParameterName})
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(13, 61, 13, expectedEndColumn)
                .WithArguments(invalidParameterName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> VariablesMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "itemById",
                "myKeyValuePair",
                "Key/value pair 'myKeyValuePair' does not follow the 'xByY' naming convention",
                "follow the 'xByY' naming convention",
                53
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0003.pattern", "^.*KeyValuePair$" } },
                "myKeyValuePair",
                "itemById",
                "Key/value pair 'itemById' does not match the '^.*KeyValuePair$' regex pattern",
                "match the '^.*KeyValuePair$' regex pattern",
                47
            }
        };

    [Theory]
    [MemberData(nameof(VariablesMemberData))]
    public async Task Analyze_WhenKeyValuePairVariableNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validVariableName,
        string invalidVariableName,
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
        public void Check()
        {{
            KeyValuePair<int, string> {validVariableName};
        }}

        public void CheckOnceMore()
        {{
            KeyValuePair<int, string> {invalidVariableName};
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(16, 39, 16, expectedEndColumn)
                .WithArguments(invalidVariableName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
