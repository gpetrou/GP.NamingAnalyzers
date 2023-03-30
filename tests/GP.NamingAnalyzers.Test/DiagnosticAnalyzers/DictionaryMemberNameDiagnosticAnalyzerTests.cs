// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.Verifiers.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.DictionaryMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class DictionaryMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("_myDictionary")]
    [InlineData("items")]
    [InlineData("xByY")]
    [InlineData("xsByYDictionary")]
    public void IsDictionaryNameValid_WhenNameIsInvalid_ShouldReturnFalse(string dictionaryName)
    {
        bool isNameValid = DictionaryMemberNameDiagnosticAnalyzer.IsDictionaryNameValid(dictionaryName, DictionaryMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("xsByY")]
    [InlineData("itemsById")]
    public void IsDictionaryNameValid_WhenNameIsValid_ShouldReturnTrue(string dictionaryName)
    {
        bool isNameValid = DictionaryMemberNameDiagnosticAnalyzer.IsDictionaryNameValid(dictionaryName, DictionaryMemberNameDiagnosticAnalyzer.DefaultRegexPattern);

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
        private Dictionary<int, string> _myDictionary;
    }
}";
        Dictionary<string, string> optionValuesByOptionName = new() { { "dotnet_diagnostic.GPNA0001.pattern", "[" } };

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Error)
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

    public static IEnumerable<object[]> FieldMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                29,
                42
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                29,
                39
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary<int, string>",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                42,
                55
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary<int, string>",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                42,
                52
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "Dictionary<int, string>",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                41,
                54
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "Dictionary<int, string>",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                41,
                51
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ConcurrentDictionary<long, double>",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                52,
                65
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ConcurrentDictionary<long, double>",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                52,
                62
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableDictionary<int, string>",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                50,
                63
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ImmutableDictionary<int, string>",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                50,
                60
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ReadOnlyDictionary<int, string>",
                "_itemsById",
                "_myDictionary",
                "Dictionary '_myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                49,
                62
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ReadOnlyDictionary<int, string>",
                "_myDictionary",
                "_itemsById",
                "Dictionary '_itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                49,
                59
            }
        };

    [Theory]
    [MemberData(nameof(FieldMemberData))]
    public async Task Analyze_WhenDictionaryFieldNameIsInvalid_ShouldReportDiagnostic(
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, expectedStartColumn, 14, expectedEndColumn)
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
                "IDictionary",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                28,
                40
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                28,
                37
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary<int, string>",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                41,
                53
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary<int, string>",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                41,
                50
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "Dictionary<int, string>",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                40,
                52
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "Dictionary<int, string>",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                40,
                49
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ConcurrentDictionary<long, double>",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                51,
                63
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ConcurrentDictionary<long, double>",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                51,
                60
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableDictionary<int, string>",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                49,
                61
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ImmutableDictionary<int, string>",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                49,
                58
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ReadOnlyDictionary<int, string>",
                "ItemsById",
                "MyDictionary",
                "Dictionary 'MyDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                48,
                60
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ReadOnlyDictionary<int, string>",
                "MyDictionary",
                "ItemsById",
                "Dictionary 'ItemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                48,
                57
            }
        };

    [Theory]
    [MemberData(nameof(PropertyMemberData))]
    public async Task Analyze_WhenDictionaryPropertyNameIsInvalid_ShouldReportDiagnostic(
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(14, expectedStartColumn, 14, expectedEndColumn)
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
                "IDictionary",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                47,
                59
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                47,
                56
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                60,
                72
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                60,
                69
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "Dictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                59,
                71
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "Dictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                59,
                68
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ConcurrentDictionary<long, double>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                70,
                82
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ConcurrentDictionary<long, double>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                70,
                79
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                68,
                80
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ImmutableDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                68,
                77
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ReadOnlyDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                67,
                79
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ReadOnlyDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                67,
                76
            }
        };

    [Theory]
    [MemberData(nameof(ParameterMemberData))]
    public async Task Analyze_WhenDictionaryParameterNameIsInvalid_ShouldReportDiagnostic(
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(17, expectedStartColumn, 17, expectedEndColumn)
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
                "IDictionary",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                25,
                37
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                25,
                34
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "IDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                38,
                50
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "IDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                38,
                47
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "Dictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                37,
                49
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "Dictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                37,
                46
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ConcurrentDictionary<long, double>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                48,
                60
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ConcurrentDictionary<long, double>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                48,
                57
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ImmutableDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                46,
                58
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ImmutableDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                46,
                55
            },
            new object[]
            {
                new Dictionary<string, string>(),
                "ReadOnlyDictionary<int, string>",
                "itemsById",
                "myDictionary",
                "Dictionary 'myDictionary' does not follow the 'xsByY' naming convention",
                "follow the 'xsByY' naming convention",
                45,
                57
            },
            new object[]
            {
                new Dictionary<string, string>() { { "dotnet_diagnostic.GPNA0001.pattern", "^.*Dictionary" } },
                "ReadOnlyDictionary<int, string>",
                "myDictionary",
                "itemsById",
                "Dictionary 'itemsById' does not match the '^.*Dictionary' regex pattern",
                "match the '^.*Dictionary' regex pattern",
                45,
                54
            }
        };

    [Theory]
    [MemberData(nameof(VariableMemberData))]
    public async Task Analyze_WhenDictionaryVariableNameIsInvalid_ShouldReportDiagnostic(
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage(expectedMessage)
                .WithSpan(20, expectedStartColumn, 20, expectedEndColumn)
                .WithArguments(invalidVariableName, expectedSecondArgument)
        };

        await VerifyCS.VerifyAnalyzerAsync(
            sourceCode,
            uniqueAdditionalPackageIdentities: null,
            optionValuesByOptionName,
            expectedDiagnosticResults);
    }
}
