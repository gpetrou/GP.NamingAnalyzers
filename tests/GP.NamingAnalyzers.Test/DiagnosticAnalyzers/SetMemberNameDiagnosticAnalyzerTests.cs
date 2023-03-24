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
        bool isNameValid = SetMemberNameDiagnosticAnalyzer.IsSetNameValid(setName);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("uniqueXs")]
    [InlineData("uniqueItems")]
    public void IsSetNameValid_WhenNameIsValid_ShouldReturnTrue(string setName)
    {
        bool isNameValid = SetMemberNameDiagnosticAnalyzer.IsSetNameValid(setName);

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

    [Theory]
    [InlineData("ISet<string>", 30, 36)]
    [InlineData("HashSet<string>", 33, 39)]
    [InlineData("ImmutableHashSet<string>", 42, 48)]
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 29, 33)]
    [InlineData("HashSet<string>", 32, 36)]
    [InlineData("ImmutableHashSet<string>", 41, 45)]
    public async Task Analyze_WhenSetPropertyNameIsInvalid_ShouldReportDiagnostic(
        string propertyType,
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
        public {propertyType} ASet {{ get; set; }}
        public {propertyType} UniqueItems {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0002", DiagnosticSeverity.Warning)
                .WithMessage("Set 'ASet' does not follow the 'uniqueXs' naming convention")
                .WithSpan(10, expectedStartColumn, 10, expectedEndColumn)
                .WithArguments("ASet")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 40, 44)]
    [InlineData("HashSet<string>", 43, 47)]
    [InlineData("ImmutableHashSet<string>", 52, 56)]
    public async Task Analyze_WhenSetParameterNameIsInvalid_ShouldReportDiagnostic(
        string parameterType,
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
        public void Check({parameterType} aSet)
        {{
        }}

        public void CheckOnceMore({parameterType} uniqueItems)
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Theory]
    [InlineData("ISet<string>", 26, 30)]
    [InlineData("HashSet<string>", 29, 33)]
    [InlineData("ImmutableHashSet<string>", 38, 42)]
    public async Task Analyze_WhenSetVariableNameIsInvalid_ShouldReportDiagnostic(
        string variableType,
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
            {variableType} aSet = null;
        }}

        public void CheckOnceMore()
        {{
            {variableType} uniqueItems = null;
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

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }
}
