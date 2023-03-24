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
        bool isNameValid = KeyValuePairMemberNameDiagnosticAnalyzer.IsKeyValuePairNameValid(keyValuePairName);

        isNameValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("xByY")]
    [InlineData("itemById")]
    public void IsKeyValuePairNameValid_WhenNameIsValid_ShouldReturnTrue(string keyValuePairName)
    {
        bool isNameValid = KeyValuePairMemberNameDiagnosticAnalyzer.IsKeyValuePairNameValid(keyValuePairName);

        isNameValid.Should().BeTrue();
    }

    [Fact]
    public async Task Analyze_WhenCodeIsEmpty_ShouldNotReportADiagnostic()
    {
        const string sourceCode = @"";

        await VerifyCS.VerifyAnalyzerAsync(sourceCode);
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

    [Fact]
    public async Task Analyze_WhenKeyValuePairFieldNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        private KeyValuePair<int, string> _myKeyValuePair;
        private KeyValuePair<int, string> _itemsById;
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage("Key/value pair '_myKeyValuePair' does not follow the 'xByY' naming convention")
                .WithSpan(9, 43, 9, 58)
                .WithArguments("_myKeyValuePair")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenKeyValuePairPropertyNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        public KeyValuePair<int, string> AKeyValuePair {{ get; set; }}
        public KeyValuePair<int, string> ItemsById {{ get; set; }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage("Key/value pair 'AKeyValuePair' does not follow the 'xByY' naming convention")
                .WithSpan(9, 42, 9, 55)
                .WithArguments("AKeyValuePair")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenKeyValuePairParameterNameIsInvalid_ShouldReportDiagnostic()
    {
        string sourceCode = $@"
using System;
using System.Collections.Generic;

namespace N
{{
    public class Example
    {{
        public void Check(KeyValuePair<int, string> aKeyValuePair)
        {{
        }}

        public void CheckOnceMore(KeyValuePair<int, string> itemsById)
        {{
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage("Key/value pair 'aKeyValuePair' does not follow the 'xByY' naming convention")
                .WithSpan(9, 53, 9, 66)
                .WithArguments("aKeyValuePair")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }

    [Fact]
    public async Task Analyze_WhenKeyValuePairVariableNameIsInvalid_ShouldReportDiagnostic()
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
            KeyValuePair<int, string> aKeyValuePair;
        }}

        public void CheckOnceMore()
        {{
            KeyValuePair<int, string> itemsById;
        }}
    }}
}}";

        DiagnosticResult[] expectedDiagnosticResults = new DiagnosticResult[]
        {
            new DiagnosticResult("GPNA0003", DiagnosticSeverity.Warning)
                .WithMessage("Key/value pair 'aKeyValuePair' does not follow the 'xByY' naming convention")
                .WithSpan(11, 39, 11, 52)
                .WithArguments("aKeyValuePair")
        };

        await VerifyCS.VerifyAnalyzerAsync(sourceCode, null, null, expectedDiagnosticResults);
    }
}
