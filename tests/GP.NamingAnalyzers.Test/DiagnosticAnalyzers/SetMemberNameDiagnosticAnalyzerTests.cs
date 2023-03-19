// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = GP.NamingAnalyzers.Test.CSharpAnalyzerVerifier<GP.NamingAnalyzers.DiagnosticAnalyzers.SetMemberNameDiagnosticAnalyzer>;

namespace GP.NamingAnalyzers.Test.DiagnosticAnalyzers;

public sealed class SetMemberNameDiagnosticAnalyzerTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("_mySet", false)]
    [InlineData("items", false)]
    [InlineData("unique", false)]
    [InlineData("uniqueXsSet", false)]
    [InlineData("uniqueXs", true)]
    public void IsSetNameValid_WhenProvidedWithParameter_ShouldReturnExpectedValue(string setName, bool expectedResult)
    {
        bool isNameValid = SetMemberNameDiagnosticAnalyzer.IsSetNameValid(setName);

        isNameValid.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Theory]
    [InlineData("ISet<string>", 30, 36)]
    [InlineData("HashSet<string>", 33, 39)]
    [InlineData("IImmutableSet<string>", 39, 45)]
    public async Task Analyze_WhenSetFieldNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
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
        private {fieldType} _items;
        private {fieldType} _uniqueItems;
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage("Set '_items' does not follow the 'uniqueXs' naming convention")
                .WithSpan(10, expectedStartColumn, 10, expectedEndColumn)
                .WithArguments("_items")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 29, 33)]
    [InlineData("HashSet<string>", 32, 36)]
    [InlineData("IImmutableSet<string>", 38, 42)]
    public async Task Analyze_WhenSetPropertyNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
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
        public {fieldType} ASet {{ get; set; }}
        public {fieldType} UniqueItems {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage("Set 'ASet' does not follow the 'uniqueXs' naming convention")
                .WithSpan(10, expectedStartColumn, 10, expectedEndColumn)
                .WithArguments("ASet")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 40, 44)]
    [InlineData("HashSet<string>", 43, 47)]
    [InlineData("IImmutableSet<string>", 49, 53)]
    public async Task Analyze_WhenSetParameterNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
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
        public void Check({fieldType} aSet)
        {{
        }}

        public void CheckOnceMore({fieldType} uniqueItems)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage("Set 'aSet' does not follow the 'uniqueXs' naming convention")
                .WithSpan(10, expectedStartColumn, 10, expectedEndColumn)
                .WithArguments("aSet")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 26, 30)]
    [InlineData("HashSet<string>", 29, 33)]
    [InlineData("IImmutableSet<string>", 35, 39)]
    public async Task Analyze_WhenSetVariableNameIsInvalid_ShouldReportDiagnostic(
        string fieldType,
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
            {fieldType} aSet = null;
        }}

        public void CheckOnceMore()
        {{
            {fieldType} uniqueItems = null;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage("Set 'aSet' does not follow the 'uniqueXs' naming convention")
                .WithSpan(12, expectedStartColumn, 12, expectedEndColumn)
                .WithArguments("aSet")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }
}
