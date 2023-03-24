// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.BooleanMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class BooleanMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("_myBoolean")]
    public void IsBooleanNameValid_WhenNameIsInvalid_ShouldReturnFalse(string booleanName)
    {
        bool isNameValid = BooleanMemberNameDiagnosticAnalyzer.IsBooleanNameValid(booleanName, BooleanMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("canBeEdited")]
    [InlineData("hasBeenEdited")]
    [InlineData("isEditing")]
    public void IsBooleanNameValid_WhenNameIsValid_ShouldReturnTrue(string booleanName)
    {
        bool isNameValid = BooleanMemberNameDiagnosticAnalyzer.IsBooleanNameValid(booleanName, BooleanMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = @"";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

    public static IEnumerable<object[]> FieldMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "_isValid",
                "_myBoolean",
                "Boolean '_myBoolean' does not follow the 'can|has|is' naming convention",
                "follow the 'can|has|is' naming convention",
                32
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0004.pattern", "^.*Boolean$" } },
                "_myBoolean",
                "_isValid",
                "Boolean '_isValid' does not match the '^.*Boolean$' regex pattern",
                "match the '^.*Boolean$' regex pattern",
                30
            }
        };

    [Theory]
    [MemberData(nameof(FieldMemberData))]
    public async Task Analyze_WhenBooleanFieldNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string validFieldName,
        string invalidFieldName,
        string expectedMessage,
        string expectedSecondArgument,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;

namespace N
{{
    public class Example
    {{
        private bool {validFieldName};
        private bool {invalidFieldName};
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(9, 22, 9, expectedEndColumn)
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
                "IsValid",
                "MyBoolean",
                "Boolean 'MyBoolean' does not follow the 'can|has|is' naming convention",
                "follow the 'can|has|is' naming convention",
                30
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0004.pattern", "^.*Boolean$" } },
                "MyBoolean",
                "IsValid",
                "Boolean 'IsValid' does not match the '^.*Boolean$' regex pattern",
                "match the '^.*Boolean$' regex pattern",
                28
            }
        };

    [Theory]
    [MemberData(nameof(PropertiesMemberData))]
    public async Task Analyze_WhenBooleanPropertyNameIsInvalid_ShouldReportDiagnostic(
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
        public bool {validPropertyName} {{ get; set; }}
        public bool {invalidPropertyName} {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(10, 21, 10, expectedEndColumn)
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
                "isValid",
                "myBoolean",
                "Boolean 'myBoolean' does not follow the 'can|has|is' naming convention",
                "follow the 'can|has|is' naming convention",
                49
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0004.pattern", "^.*Boolean$" } },
                "myBoolean",
                "isValid",
                "Boolean 'isValid' does not match the '^.*Boolean$' regex pattern",
                "match the '^.*Boolean$' regex pattern",
                47
            }
        };

    [Theory]
    [MemberData(nameof(ParametersMemberData))]
    public async Task Analyze_WhenBooleanParameterNameIsInvalid_ShouldReportDiagnostic(
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
        public void Check(bool {validParameterName})
        {{
        }}

        public void CheckOnceMore(bool {invalidParameterName})
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(13, 40, 13, expectedEndColumn)
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
                "isValid",
                "myBoolean",
                "Boolean 'myBoolean' does not follow the 'can|has|is' naming convention",
                "follow the 'can|has|is' naming convention",
                27
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0004.pattern", "^.*Boolean$" } },
                "myBoolean",
                "isValid",
                "Boolean 'isValid' does not match the '^.*Boolean$' regex pattern",
                "match the '^.*Boolean$' regex pattern",
                25
            }
        };

    [Theory]
    [MemberData(nameof(VariablesMemberData))]
    public async Task Analyze_WhenBooleanVariableNameIsInvalid_ShouldReportDiagnostic(
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
            bool {validVariableName};
        }}

        public void CheckOnceMore()
        {{
            bool {invalidVariableName};
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(16, 18, 16, expectedEndColumn)
                .WithArguments(invalidVariableName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
