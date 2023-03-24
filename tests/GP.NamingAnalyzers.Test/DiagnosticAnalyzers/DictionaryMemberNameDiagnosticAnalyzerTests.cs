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
        bool isNameValid = DictionaryMemberNameDiagnosticAnalyzer.IsDictionaryNameValid(dictionaryName);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("xsByY")]
    [InlineData("itemsById")]
    public void IsDictionaryNameValid_WhenNameIsValid_ShouldReturnTrue(string dictionaryName)
    {
        bool isNameValid = DictionaryMemberNameDiagnosticAnalyzer.IsDictionaryNameValid(dictionaryName);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = @"";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

    [Theory]
    [InlineData("IDictionary", 29, 42)]
    [InlineData("IDictionary<int, string>", 42, 55)]
    [InlineData("Dictionary<int, string>", 41, 54)]
    [InlineData("ConcurrentDictionary<long, double>", 52, 65)]
    [InlineData("ImmutableDictionary<int, string>", 50, 63)]
    [InlineData("ReadOnlyDictionary<int, string>", 49, 62)]
    public async Task Analyze_WhenDictionaryFieldNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
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
        private {fieldType} _myDictionary;
        private {fieldType} _itemsById;
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary '_myDictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(13, expectedStartColumn, 13, expectedEndColumn)
                .WithArguments("_myDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 28, 39)]
    [InlineData("IDictionary<int, string>", 41, 52)]
    [InlineData("Dictionary<int, string>", 40, 51)]
    [InlineData("ConcurrentDictionary<long, double>", 51, 62)]
    [InlineData("ImmutableDictionary<int, string>", 49, 60)]
    [InlineData("ReadOnlyDictionary<int, string>", 48, 59)]
    public async Task Analyze_WhenDictionaryPropertyNameIsInvalid_ShouldReportDiagnostic(
        string propertyType,
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
        public {propertyType} ADictionary {{ get; set; }}
        public {propertyType} ItemsById {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'ADictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(13, expectedStartColumn, 13, expectedEndColumn)
                .WithArguments("ADictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 39, 50)]
    [InlineData("IDictionary<int, string>", 52, 63)]
    [InlineData("Dictionary<int, string>", 51, 62)]
    [InlineData("ConcurrentDictionary<long, double>", 62, 73)]
    [InlineData("ImmutableDictionary<int, string>", 60, 71)]
    [InlineData("ReadOnlyDictionary<int, string>", 59, 70)]
    public async Task Analyze_WhenDictionaryParameterNameIsInvalid_ShouldReportDiagnostic(
        string parameterType,
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
        public void Check({parameterType} aDictionary)
        {{
        }}

        public void CheckOnceMore({parameterType} itemsById)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'aDictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(13, expectedStartColumn, 13, expectedEndColumn)
                .WithArguments("aDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 25, 36)]
    [InlineData("IDictionary<int, string>", 38, 49)]
    [InlineData("Dictionary<int, string>", 37, 48)]
    [InlineData("ConcurrentDictionary<long, double>", 48, 59)]
    [InlineData("ImmutableDictionary<int, string>", 46, 57)]
    [InlineData("ReadOnlyDictionary<int, string>", 45, 56)]
    public async Task Analyze_WhenDictionaryVariableNameIsInvalid_ShouldReportDiagnostic(
        string variableType,
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
            {variableType} aDictionary = null;
        }}

        public void CheckOnceMore()
        {{
            {variableType} itemsById = null;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'aDictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(15, expectedStartColumn, 15, expectedEndColumn)
                .WithArguments("aDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }
}
