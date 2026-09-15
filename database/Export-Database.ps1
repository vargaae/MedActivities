param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'MedActivities',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'exports/20260915'),
    [string]$SchemaFile = (Join-Path $PSScriptRoot 'exports/20260915/schema.sql')
)
$ErrorActionPreference = 'Stop'
if ($Database -notmatch '^[A-Za-z0-9_]+$') { throw 'Invalid database name' }
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($outputPath) | Out-Null
if (!(Test-Path -LiteralPath $SchemaFile)) { throw 'Generate schema.sql with dotnet ef migrations script first.' }
$backup = Join-Path $outputPath 'MedActivities.bak'
$mdf = Join-Path $outputPath 'MedActivities.mdf'
$ldf = Join-Path $outputPath 'MedActivities_log.ldf'
$dump = Join-Path $outputPath 'MedActivities.sql'
foreach ($file in @($backup,$mdf,$ldf,$dump)) { if (Test-Path -LiteralPath $file) { throw "Export already exists: $file" } }
$clone = 'MedActivities_Export_' + [Guid]::NewGuid().ToString('N')
function SqlLiteral([string]$value) { return "N'" + $value.Replace("'", "''") + "'" }
function Connect([string]$name) {
    $cn = [System.Data.SqlClient.SqlConnection]::new("Server=$Server;Database=$name;Integrated Security=True;Encrypt=False;TrustServerCertificate=True")
    $cn.Open()
    return $cn
}
function Execute($cn, [string]$sql) {
    $cmd = $cn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    try { [void]$cmd.ExecuteNonQuery() } finally { $cmd.Dispose() }
}
function ReadTable($cn, [string]$sql) {
    $cmd = $cn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($cmd)
    $table = [System.Data.DataTable]::new()
    try { [void]$adapter.Fill($table); return ,$table } finally { $adapter.Dispose(); $cmd.Dispose() }
}
function ValueSql($v) {
    if ($v -is [DBNull]) { return 'NULL' }
    if ($v -is [byte[]]) { return '0x' + [Convert]::ToHexString($v) }
    if ($v -is [bool]) { return $(if ($v) { '1' } else { '0' }) }
    if ($v -is [DateTime]) { return SqlLiteral $v.ToString('yyyy-MM-ddTHH:mm:ss.fffffff', [Globalization.CultureInfo]::InvariantCulture) }
    if ($v -is [TimeSpan]) { return SqlLiteral $v.ToString('c') }
    if ($v -is [string] -or $v -is [Guid] -or $v -is [DateTimeOffset]) { return SqlLiteral ([string]$v) }
    return [Convert]::ToString($v, [Globalization.CultureInfo]::InvariantCulture)
}
$master = Connect 'master'
try {
    Execute $master "BACKUP DATABASE [$Database] TO DISK=$(SqlLiteral $backup) WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=$(SqlLiteral $backup) WITH CHECKSUM;"
    $files = ReadTable $master "RESTORE FILELISTONLY FROM DISK=$(SqlLiteral $backup)"
    $dataName = ($files.Rows | Where-Object Type -eq 'D' | Select-Object -First 1).LogicalName
    $logName = ($files.Rows | Where-Object Type -eq 'L' | Select-Object -First 1).LogicalName
    Execute $master "RESTORE DATABASE [$clone] FROM DISK=$(SqlLiteral $backup) WITH MOVE $(SqlLiteral $dataName) TO $(SqlLiteral $mdf), MOVE $(SqlLiteral $logName) TO $(SqlLiteral $ldf), RECOVERY;"
    $cn = Connect $clone
    try {
        $tables = ReadTable $cn "SELECT s.name SchemaName,t.name TableName FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE t.is_ms_shipped=0 AND t.name<>'__EFMigrationsHistory' ORDER BY s.name,t.name"
        $writer = [IO.StreamWriter]::new($dump, $false, [Text.UTF8Encoding]::new($true))
        try {
            $writer.WriteLine('-- Full schema + data. Run against a NEW EMPTY database with sqlcmd -b -f 65001.')
            $writer.WriteLine('IF EXISTS (SELECT 1 FROM sys.tables) THROW 51000, ''Target database must be empty.'', 1;')
            $writer.WriteLine('GO')
            $writer.WriteLine([IO.File]::ReadAllText([IO.Path]::GetFullPath($SchemaFile)))
            $writer.WriteLine('GO')
            $writer.WriteLine('SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF; SET XACT_ABORT ON; BEGIN TRANSACTION;')
            foreach ($table in $tables.Rows) { $writer.WriteLine("ALTER TABLE [$($table.SchemaName)].[$($table.TableName)] NOCHECK CONSTRAINT ALL;") }
            foreach ($table in $tables.Rows) {
                $name = "[$($table.SchemaName)].[$($table.TableName)]"
                $columns = ReadTable $cn "SELECT name,is_identity FROM sys.columns WHERE object_id=OBJECT_ID('$name') AND is_computed=0 AND system_type_id<>189 ORDER BY column_id"
                $fields = ($columns.Rows | ForEach-Object { '[' + $_.name + ']' }) -join ','
                $rows = ReadTable $cn "SELECT $fields FROM $name"
                $identity = @($columns.Rows | Where-Object is_identity -eq $true).Count -gt 0
                if ($identity) { $writer.WriteLine("SET IDENTITY_INSERT $name ON;") }
                foreach ($row in $rows.Rows) {
                    $values = ($columns.Rows | ForEach-Object { ValueSql $row[$_.name] }) -join ','
                    $writer.WriteLine("INSERT INTO $name ($fields) VALUES ($values);")
                }
                if ($identity) { $writer.WriteLine("SET IDENTITY_INSERT $name OFF;") }
                Write-Output "$name : $($rows.Rows.Count) rows exported"
            }
            foreach ($table in $tables.Rows) { $writer.WriteLine("ALTER TABLE [$($table.SchemaName)].[$($table.TableName)] WITH CHECK CHECK CONSTRAINT ALL;") }
            $writer.WriteLine('COMMIT;')
        } finally { $writer.Dispose() }
    } finally { $cn.Dispose() }
    [System.Data.SqlClient.SqlConnection]::ClearAllPools()
    Execute $master "ALTER DATABASE [$clone] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; EXEC sp_detach_db @dbname=N'$clone';"
    Write-Output "Export complete: $outputPath"
} finally { $master.Dispose() }
