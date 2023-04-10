﻿// Copyright (c) gpetrou. All Rights Reserved.
// Licensed under the MIT License. See License.md in the project root for license information.

using FluentAssertions;
using GP.NamingAnalyzers.DiagnosticAnalyzers;
using GP.NamingAnalyzers.Utilities;
using Xunit;

namespace GP.NamingAnalyzers.Test.Utilities;

public sealed class StringUtilitiesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("[")]
    public void IsValidRegexPattern_WhenRegexPatternIsInvalid_ShouldReturnFalse(string regexPattern)
    {
        bool isValidRegexPattern = StringUtilities.IsValidRegexPattern(regexPattern);

        isValidRegexPattern.Should().BeFalse();
    }

    [Theory]
    [InlineData(BooleanMemberNameDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(DictionaryMemberNameDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(KeyValuePairMemberNameDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(MethodNameWithBooleanReturnTypeDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(MethodNameWithDictionaryReturnTypeDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(MethodNameWithKeyValuePairReturnTypeDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(MethodNameWithSetReturnTypeDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(MockedMemberNameDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(SetMemberNameDiagnosticAnalyzer.DefaultRegexPattern)]
    [InlineData(TestMethodNameDiagnosticAnalyzer.DefaultRegexPattern)]
    public void IsValidRegexPattern_WhenRegexPatternIsValid_ShouldReturnTrue(string regexPattern)
    {
        bool isValidRegexPattern = StringUtilities.IsValidRegexPattern(regexPattern);

        isValidRegexPattern.Should().BeTrue();
    }
}
