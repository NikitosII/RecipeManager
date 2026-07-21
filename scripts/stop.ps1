# Stops the RecipeManager API and client.

$ports = @(5027, 5173)

foreach ($port in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $conns) {
        Write-Host "Port $port : nothing running." -ForegroundColor DarkGray
        continue
    }
    foreach ($procId in ($conns.OwningProcess | Select-Object -Unique)) {
        $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
        $name = if ($proc) { $proc.ProcessName } else { 'unknown' }
        Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
        Write-Host "Port $port : stopped $name (PID $procId)." -ForegroundColor Yellow
    }
}

Write-Host 'RecipeManager stopped.' -ForegroundColor Cyan
