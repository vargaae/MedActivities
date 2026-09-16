param(
    [string]$ExportDirectory = (Join-Path $PSScriptRoot 'exports/20260916'),
    [string]$Server = '(localdb)\MSSQLLocalDB'
)
$ErrorActionPreference = 'Stop'
$dump = (Resolve-Path (Join-Path $ExportDirectory 'MedActivities.sql')).Path
$testDatabase = 'MedActivities_ExportVerify_' + [Guid]::NewGuid().ToString('N')
$created = $false
try {
    & sqlcmd -S $Server -E -C -b -d master -Q "CREATE DATABASE [$testDatabase]"
    if ($LASTEXITCODE -ne 0) { throw 'Could not create export verification database.' }
    $created = $true
    $importOutput = & sqlcmd -S $Server -E -C -b -f 65001 -d $testDatabase -i $dump 2>&1
    if ($LASTEXITCODE -ne 0) { $importOutput | Select-Object -Last 20; throw 'SQL dump import failed.' }
    & sqlcmd -S $Server -E -C -b -d $testDatabase -Q "SET NOCOUNT ON; DBCC CHECKDB WITH NO_INFOMSGS; SELECT 'Patients' AS Entity,COUNT(*) AS Records FROM Patients UNION ALL SELECT 'Practitioners',COUNT(*) FROM Practitioners UNION ALL SELECT 'Activities',COUNT(*) FROM Activities UNION ALL SELECT 'Appointments',COUNT(*) FROM Appointments UNION ALL SELECT 'Users',COUNT(*) FROM AspNetUsers; SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;"
    if ($LASTEXITCODE -ne 0) { throw 'Imported database verification failed.' }
} finally {
    if ($created -and $testDatabase -match '^MedActivities_ExportVerify_[a-f0-9]{32}$') {
        & sqlcmd -S $Server -E -C -b -d master -Q "ALTER DATABASE [$testDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$testDatabase];"
        if ($LASTEXITCODE -ne 0) { throw "Remove temporary database manually: $testDatabase" }
    }
}
