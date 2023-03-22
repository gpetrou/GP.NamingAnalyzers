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
        bool isNameValid = BooleanMemberNameDiagnosticAnalyzer.IsBooleanNameValid(booleanName);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("canBeEdited")]
    [InlineData("hasBeenEdited")]
    [InlineData("isEditing")]
    public void IsBooleanNameValid_WhenNameIsValid_ShouldReturnTrue(string booleanName)
    {
        bool isNameValid = BooleanMemberNameDiagnosticAnalyzer.IsBooleanNameValid(booleanName);

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

    [Fact]
    public async Task Analyze_WhenBooleanFieldNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;

namespace N
{{
    public class Example
    {{
        private bool _myBoolean;
        private bool _isValid;
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage("Boolean '_myBoolean' does not follow the 'can|has|is' naming convention")
                .WithSpan(8, 22, 8, 32)
                .WithArguments("_myBoolean")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenBooleanPropertyNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        public bool MyBoolean {{ get; set; }}
        public bool IsValid {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage("Boolean 'MyBoolean' does not follow the 'can|has|is' naming convention")
                .WithSpan(9, 21, 9, 30)
                .WithArguments("MyBoolean")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenBooleanParameterNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        public void Check(bool myBoolean)
        {{
        }}

        public void CheckOnceMore(bool isValid)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage("Boolean 'myBoolean' does not follow the 'can|has|is' naming convention")
                .WithSpan(9, 32, 9, 41)
                .WithArguments("myBoolean")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenBooleanVariableNameIsInvalid_ShouldReportDiagnostic()
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
            bool myBoolean;
        }}

        public void CheckOnceMore()
        {{
            bool isValid;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0004", DiagnosticSeverity.Warning)
                .WithMessage("Boolean 'myBoolean' does not follow the 'can|has|is' naming convention")
                .WithSpan(11, 18, 11, 27)
                .WithArguments("myBoolean")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, expectedDiagnosticResults);
    }
}
