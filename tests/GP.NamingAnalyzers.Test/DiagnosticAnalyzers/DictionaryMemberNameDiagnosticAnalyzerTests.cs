// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.DictionaryMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class DictionaryMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("_myDictionary", false)]
    [InlineData("items", false)]
    [InlineData("xByY", false)]
    [InlineData("xsByYDictionary", false)]
    [InlineData("xsByY", true)]
    public void IsDictionaryNameValid_WhenProvidedWithParameter_ShouldReturnExpectedValue(string dictionaryName, bool expectedResult)
    {
        bool isNameValid = DictionaryMemberNameDiagnosticAnalyzer.IsDictionaryNameValid(dictionaryName);

        isNameValid.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("IDictionary", 29, 42)]
    [InlineData("IDictionary<int, string>", 42, 55)]
    [InlineData("Dictionary<int, string>", 41, 54)]
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
                .WithSpan(12, expectedStartColumn, 12, expectedEndColumn)
                .WithArguments("_myDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 28, 39)]
    [InlineData("IDictionary<int, string>", 41, 52)]
    [InlineData("Dictionary<int, string>", 40, 51)]
    [InlineData("ImmutableDictionary<int, string>", 49, 60)]
    [InlineData("ReadOnlyDictionary<int, string>", 48, 59)]
    public async Task Analyze_WhenDictionaryPropertyNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
        int expectedStartColumn,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace N
{{
    public class Example
    {{
        public {fieldType} ADictionary {{ get; set; }}
        public {fieldType} ItemsById {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'ADictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(12, expectedStartColumn, 12, expectedEndColumn)
                .WithArguments("ADictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 39, 50)]
    [InlineData("IDictionary<int, string>", 52, 63)]
    [InlineData("Dictionary<int, string>", 51, 62)]
    [InlineData("ImmutableDictionary<int, string>", 60, 71)]
    [InlineData("ReadOnlyDictionary<int, string>", 59, 70)]
    public async Task Analyze_WhenDictionaryParameterNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
        int expectedStartColumn,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace N
{{
    public class Example
    {{
        public void Check({fieldType} aDictionary)
        {{
        }}

        public void CheckOnceMore({fieldType} itemsById)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'aDictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(12, expectedStartColumn, 12, expectedEndColumn)
                .WithArguments("aDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("IDictionary", 25, 36)]
    [InlineData("IDictionary<int, string>", 38, 49)]
    [InlineData("Dictionary<int, string>", 37, 48)]
    [InlineData("ImmutableDictionary<int, string>", 46, 57)]
    [InlineData("ReadOnlyDictionary<int, string>", 45, 56)]
    public async Task Analyze_WhenDictionaryVariableNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
        int expectedStartColumn,
        int expectedEndColumn)
    {
        string sourceCode = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace N
{{
    public class Example
    {{
        public void Check()
        {{
            {fieldType} aDictionary = null;
        }}

        public void CheckOnceMore()
        {{
            {fieldType} itemsById = null;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0001", DiagnosticSeverity.Warning)
                .WithMessage("Dictionary 'aDictionary' does not follow the 'xsByY' naming convention")
                .WithSpan(14, expectedStartColumn, 14, expectedEndColumn)
                .WithArguments("aDictionary")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }
}
