[CmdletBinding()]
param(
    [string]$ConnectionString
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = Read-Host 'SQL Server connection string'
}
if ([string]::IsNullOrWhiteSpace($ConnectionString) -or
    $ConnectionString -notmatch '(?i)(server|data source)\s*=' -or
    $ConnectionString -notmatch '(?i)(database|initial catalog)\s*=') {
    throw 'Érvényes SQL Server connection string szükséges (Server/Data Source és Database/Initial Catalog).'
}

$names = @('Database__Provider', 'ConnectionStrings__SqlServerConnection')
$old = @{}
foreach ($name in $names) {
    $item = Get-Item -Path "Env:$name" -ErrorAction SilentlyContinue
    $old[$name] = if ($null -eq $item) { $null } else { $item.Value }
}
try {
    $env:Database__Provider = 'SqlServer'
    $env:ConnectionStrings__SqlServerConnection = $ConnectionString
    Push-Location $repositoryRoot
    dotnet ef database update --context SqlServerDbContext --project Persistence/Persistence.csproj --startup-project API/API.csproj
    if ($LASTEXITCODE -ne 0) { throw "Az EF migration sikertelen (exit code: $LASTEXITCODE)." }
}
finally {
    Pop-Location
    foreach ($name in $names) {
        if ($null -eq $old[$name]) { Remove-Item -Path "Env:$name" -ErrorAction SilentlyContinue }
        else { Set-Item -Path "Env:$name" -Value $old[$name] }
    }
}
