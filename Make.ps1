# Copyright (c) gpetrou. All Rights Reserved.
# Licensed under the MIT License. See Licence.md in the project root for license information.

[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet(
        "clean",
        "build",
        "test",
        "coverage",
        "pack",
        "all",
        "help")]
    [string] $action = "")

# Establish and enforce coding rules in expressions, scripts and script blocks.
Set-StrictMode -Version Latest
$errorActionPreference = "Stop"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:IsTreatWarningsAsErrorsEnabled = $true

Set-Variable SolutionName -Option Constant -Value "GP.NamingAnalyzers" -Force -ErrorAction SilentlyContinue
Set-Variable SolutionPath -Option Constant -Value "$PSScriptRoot/$SolutionName.sln" -Force -ErrorAction SilentlyContinue
Set-Variable ArtifactsPath -Option Constant -Value "$PSScriptRoot/artifacts" -Force -ErrorAction SilentlyContinue
Set-Variable BinlogPath -Option Constant -Value "$ArtifactsPath/Build/Release/$SolutionName.binlog" -Force -ErrorAction SilentlyContinue
Set-Variable TestResultsPath -Option Constant -Value "$ArtifactsPath/TestResults" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoveragePath -Option Constant -Value "$ArtifactsPath/Coverage" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageResultsPath -Option Constant -Value "$TestCoveragePath/Results" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageReportPath -Option Constant -Value "$TestCoveragePath/Report" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageHistoryPath -Option Constant -Value "$TestCoveragePath/History" -Force -ErrorAction SilentlyContinue
Set-Variable PackageProjectPath -Option Constant -Value "src/GP.NamingAnalyzers.Package/GP.NamingAnalyzers.Package.csproj" -Force -ErrorAction SilentlyContinue

function Exec([ScriptBlock] $cmd) {
    try {
        & $cmd
        if ($LASTEXITCODE -ne 0) {
            throw "Last exit code was $LASTEXITCODE while executing the command."
        }
    } catch {
        Write-Error $_ -Category InvalidOperation
        Exit $LASTEXITCODE
    }
}

function DeleteDirectory([string] $directoryPath) {
    if (Test-Path $directoryPath) {
        Remove-Item $directoryPath -Force -Recurse
    }
}

function Clean() {
    Exec {
        dotnet restore $SolutionPath
    }

    Exec {
        dotnet clean $SolutionPath `
            --configuration Release
    }

    DeleteDirectory $ArtifactsPath
}

function Build() {
    Exec {
        dotnet build $SolutionPath `
            --configuration Release `
            -bl:$BinlogPath
    }
}

function Test() {
    $testProjectPath = "$PSScriptRoot/tests/GP.NamingAnalyzers.Test/GP.NamingAnalyzers.Test.csproj"
    Exec {
        dotnet test $testProjectPath `
            --configuration Release `
            --no-build `
            --collect:"XPlat Code Coverage" `
            --logger "junit;LogFilePath=$TestResultsPath/{assembly}.xml" `
            --results-directory "$TestCoverageResultsPath"
    }
}

function CreateCoverageReport() {
    Exec {
        dotnet tool restore
    }

    $gitCommitShortSha1 = git rev-parse --short HEAD

    Exec {
        & dotnet reportgenerator `
            -reports:"$TestCoverageResultsPath/**/*.xml" `
            -targetdir:"$TestCoverageReportPath" `
            -historydir:"$TestCoverageHistoryPath" `
            -tag:"$gitCommitShortSha1" `
            -reporttypes:"Badges;Html;CsvSummary" `
            -title:"Naming Analyzers"
    }

    Write-Output ([Environment]::NewLine)
    Write-Output "Coverage summary ('GP.NamingAnalyzers' namespace has been omitted)"
    Get-Content -Path "$TestCoverageReportPath/Summary.csv" |
        Select-Object -Skip 1 |
        ForEach-Object { $_ -replace 'GP.NamingAnalyzers.', [string]::Empty } |
        ConvertFrom-Csv -Delimiter ";" |
        Format-Table -AutoSize -HideTableHeaders
}

function Pack() {
    Exec {
        dotnet pack $PackageProjectPath `
            --configuration Release `
            --no-build
    }
}

function DisplayHelp() {
    $availableActions = @(
        @{ Name = "clean"; Description = "Cleans the solution artifacts" },
        @{ Name = "build"; Description = "Builds the solution" },
        @{ Name = "test"; Description = "Runs the tests" },
        @{ Name = "coverage"; Description = "Generates the coverage report" },
        @{ Name = "pack"; Description = "Generates the NuGet package" },
        @{ Name = "all"; Description = "Runs all the above actions" },
        @{ Name = "help"; Description = "Displays this content" })

    Write-Output ([Environment]::NewLine)
    Write-Output "Usage: ./Make.ps1 [action]"
    Write-Output ([Environment]::NewLine)
    Write-Output "Available actions:"
    $availableActions | % { [PSCustomObject]$_ } | Format-Table -HideTableHeaders
}

switch -wildcard ($action) {
    "clean" {
        Clean
        break
    }

    "build" {
        Build
        break
    }

    "test" {
        Test
        break
    }

    "coverage" {
        CreateCoverageReport
        break
    }

    "pack" {
        Pack
        break
    }

    "all" {
        Clean
        Build
        Test
        CreateCoverageReport
        Pack
        break
    }

    "help" {
        DisplayHelp
        break
    }

    default {
        DisplayHelp
        Write-Error "Could not find action '$action'." -Category InvalidArgument
        Exit -1
    }
}