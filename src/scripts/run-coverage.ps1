Write-Host "Running tests with code coverage..." -ForegroundColor Cyan

$scriptDir = $PSScriptRoot
$testProject = Join-Path $scriptDir "..\..\CoreCodeCamp.Tests\CoreCodeCamp.Tests.csproj"
$resultsDir = Join-Path $scriptDir "..\..\CoreCodeCamp.Tests\TestResults"

# Resolve to absolute paths
$testProject = (Resolve-Path $testProject).Path
$resultsDir = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($resultsDir)

Write-Host "Test project: $testProject"
Write-Host "Results dir:  $resultsDir"

# Clean previous results
if (Test-Path $resultsDir) { Remove-Item -Recurse -Force $resultsDir }

# Run tests with coverage
dotnet test $testProject --collect:"XPlat Code Coverage" --results-directory $resultsDir

# Display coverage summary
$coverageFile = Get-ChildItem -Path $resultsDir -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
if ($coverageFile) {
    $xml = [xml](Get-Content $coverageFile.FullName)
    $lineRate = [math]::Round([double]$xml.coverage.'line-rate' * 100, 2)
    $branchRate = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 2)
    $linesCovered = $xml.coverage.'lines-covered'
    $linesValid = $xml.coverage.'lines-valid'
    
    Write-Host ""
    Write-Host "=== Code Coverage Summary ===" -ForegroundColor Green
    Write-Host "Line Coverage:   $lineRate% ($linesCovered / $linesValid lines)" -ForegroundColor White
    Write-Host "Branch Coverage: $branchRate%" -ForegroundColor White
    Write-Host "Report: $($coverageFile.FullName)" -ForegroundColor Gray
} else {
    Write-Host "No coverage file found." -ForegroundColor Red
}
