# Starts the RecipeManager backend API and frontend client in two new windows.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host 'Starting RecipeManager...' -ForegroundColor Cyan

# Backend API -> http://localhost:5027
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location `"$root`"; Write-Host 'API  -> http://localhost:5027' -ForegroundColor Green; dotnet run --project src/RecipeManager.Api"
)

# Frontend client -> http://localhost:5173
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location `"$root\client`"; Write-Host 'Web  -> http://localhost:5173' -ForegroundColor Green; npm run dev"
)

Write-Host ''
Write-Host 'Two windows launched:' -ForegroundColor Cyan
Write-Host '  API  -> http://localhost:5027/api/v1'
Write-Host '  Web  -> http://localhost:5173   (open this in your browser)'
Write-Host ''
Write-Host 'Run scripts/stop.ps1 to shut them down.'
