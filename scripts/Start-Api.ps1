[CmdletBinding()]
param([switch]$Restart)
$ErrorActionPreference = 'Stop'
$apiRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\API'))
$apiDll = Join-Path $apiRoot 'bin\Debug\net10.0\API.dll'
$apiExe = Join-Path $apiRoot 'bin\Debug\net10.0\API.exe'
$running = @(Get-CimInstance Win32_Process | Where-Object {
    $_.ExecutablePath -eq $apiExe -or
    ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -and
        ($_.CommandLine.Contains('"' + $apiDll + '"') -or $_.CommandLine.Contains('dotnet" ' + $apiDll)))
})
if ($running.Count -gt 0 -and !$Restart) {
    throw 'Ez az API már fut. Állítsd le Ctrl+C-vel, vagy futtasd: pwsh -File scripts/Start-Api.ps1 -Restart'
}
foreach ($apiProcess in $running) {
    Write-Host "A projekt API-jának leállítása: $($apiProcess.ProcessId)"
    Stop-Process -Id $apiProcess.ProcessId -ErrorAction Stop
    Wait-Process -Id $apiProcess.ProcessId -Timeout 15 -ErrorAction SilentlyContinue
}
Push-Location (Split-Path -Parent $apiRoot)
try {
    dotnet build API/API.csproj
    if ($LASTEXITCODE -ne 0) { throw 'A build sikertelen. Ellenőrizd, hogy Visual Studio alatt nem fut-e még az API.' }
    dotnet run --project API/API.csproj --no-build --launch-profile https
} finally { Pop-Location }
