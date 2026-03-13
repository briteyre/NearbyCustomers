Write-Host "Running Stryker mutation testing..." -ForegroundColor Cyan

Push-Location ..\..\CoreCodeCamp.Tests

dotnet stryker -f stryker-config.json

Pop-Location
