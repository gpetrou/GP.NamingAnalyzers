// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.SetMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class SetMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("_mySet")]
    [InlineData("items")]
    [InlineData("unique")]
    [InlineData("uniqueXsSet")]
    public void IsSetNameValid_WhenNameIsInvalid_ShouldReturnFalse(string setName)
    {
        bool isNameValid = SetMemberNameDiagnosticAnalyzer.IsSetNameValid(setName, SetMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("uniqueXs")]
    [InlineData("uniqueItems")]
    public void IsSetNameValid_WhenNameIsValid_ShouldReturnTrue(string setName)
    {
        bool isNameValid = SetMemberNameDiagnosticAnalyzer.IsSetNameValid(setName, SetMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = "";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

    public static IEnumerable<object[]> FieldMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "ISet<string>",
                "_uniqueItems",
                "_mySet",
                "Set '_mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                30,
                36
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ISet<string>",
                "_mySet",
                "_uniqueItems",
                "Set '_uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                30,
                42
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "HashSet<string>",
                "_uniqueItems",
                "_mySet",
                "Set '_mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                33,
                39
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "HashSet<string>",
                "_mySet",
                "_uniqueItems",
                "Set '_uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                33,
                45
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableHashSet<string>",
                "_uniqueItems",
                "_mySet",
                "Set '_mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                42,
                48
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ImmutableHashSet<string>",
                "_mySet",
                "_uniqueItems",
                "Set '_uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                42,
                54
            }
        };

    [Theory]
    [MemberData(nameof(FieldMemberData))]
    public async Task Analyze_WhenSetFieldNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string fieldType,
        string validFieldName,
        string invalidFieldName,
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
        private {fieldType} {validFieldName};
        private {fieldType} {invalidFieldName};
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(11, expectedStartColumn, 11, expectedEndColumn)
                .WithArguments(invalidFieldName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> PropertyMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "ISet<string>",
                "UniqueItems",
                "MySet",
                "Set 'MySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                29,
                34
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ISet<string>",
                "MySet",
                "UniqueItems",
                "Set 'UniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                29,
                40
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "HashSet<string>",
                "UniqueItems",
                "MySet",
                "Set 'MySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                32,
                37
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "HashSet<string>",
                "MySet",
                "UniqueItems",
                "Set 'UniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                32,
                43
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableHashSet<string>",
                "UniqueItems",
                "MySet",
                "Set 'MySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                41,
                46
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ImmutableHashSet<string>",
                "MySet",
                "UniqueItems",
                "Set 'UniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                41,
                52
            }
        };

    [Theory]
    [MemberData(nameof(PropertyMemberData))]
    public async Task Analyze_WhenSetPropertyNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string propertyType,
        string validPropertyName,
        string invalidPropertyName,
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
        public {propertyType} {validPropertyName} {{ get; set; }}
        public {propertyType} {invalidPropertyName} {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(11, expectedStartColumn, 11, expectedEndColumn)
                .WithArguments(invalidPropertyName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> ParameterMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "ISet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                48,
                53
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ISet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                48,
                59
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "HashSet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                51,
                56
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "HashSet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                51,
                62
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableHashSet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                60,
                65
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ImmutableHashSet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                60,
                71
            }
        };

    [Theory]
    [MemberData(nameof(ParameterMemberData))]
    public async Task Analyze_WhenSetParameterNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string parameterType,
        string validParameterName,
        string invalidParameterName,
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
        public void Check({parameterType} {validParameterName})
        {{
        }}

        public void CheckOnceMore({parameterType} {invalidParameterName})
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, expectedStartColumn, 14, expectedEndColumn)
                .WithArguments(invalidParameterName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }

    public static IEnumerable<object[]> VariableMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "ISet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                26,
                31
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ISet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                26,
                37
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "HashSet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                29,
                34
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "HashSet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                29,
                40
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableHashSet<string>",
                "uniqueItems",
                "mySet",
                "Set 'mySet' does not follow the 'uniqueXs' naming convention",
                "follow the 'uniqueXs' naming convention",
                38,
                43
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0002.pattern", "^.*Set" } },
                "ImmutableHashSet<string>",
                "mySet",
                "uniqueItems",
                "Set 'uniqueItems' does not match the '^.*Set' regex pattern",
                "match the '^.*Set' regex pattern",
                38,
                49
            }
        };

    [Theory]
    [MemberData(nameof(VariableMemberData))]
    public async Task Analyze_WhenSetVariableNameIsInvalid_ShouldReportDiagnostic(
        Dictionary<string, string> optionValuesByOptionName,
        string variableType,
        string validVariableName,
        string invalidVariableName,
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
        public void Check()
        {{
            {variableType} {validVariableName} = null;
        }}

        public void CheckOnceMore()
        {{
            {variableType} {invalidVariableName} = null;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(17, expectedStartColumn, 17, expectedEndColumn)
                .WithArguments(invalidVariableName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
