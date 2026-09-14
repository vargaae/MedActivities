[CmdletBinding()]
param([string]$Email = 'admin@example.undefined')
$ErrorActionPreference = 'Stop'
$demoSecret = Read-Host 'Demóadmin jelszava (nem kerül fájlba)' -AsSecureString
$names = @('DemoAdmin__Email','DemoAdmin__Password','ASPNETCORE_ENVIRONMENT')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name) }
$plainPointer = [IntPtr]::Zero
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    $plainPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($demoSecret)
    $env:DemoAdmin__Password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($plainPointer)
    $env:DemoAdmin__Email = $Email
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    dotnet run --project API/API.csproj --no-build --no-launch-profile -- --configure-demo-admin
    if ($LASTEXITCODE -ne 0) { throw 'A demóadmin konfigurálása sikertelen.' }
} finally {
    if ($plainPointer -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($plainPointer) }
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) }
    Pop-Location
}
