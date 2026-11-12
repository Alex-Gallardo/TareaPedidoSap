$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    throw 'Se requiere el compilador de .NET Framework 4 de 64 bits.'
}

$testDirectory = Join-Path ([IO.Path]::GetTempPath()) (
    'TareaPedidoSap-SldTests-' + [Guid]::NewGuid().ToString('N')
)
[void][IO.Directory]::CreateDirectory($testDirectory)
$testExecutable = Join-Path $testDirectory 'SldConnectionTests.exe'

& $compiler /nologo /target:exe "/out:$testExecutable" `
    /reference:System.Configuration.dll /reference:System.Core.dll `
    /reference:System.Web.dll `
    (Join-Path $repositoryRoot 'TareaPedidoSap\Utils\ApplicationContext.cs') `
    (Join-Path $PSScriptRoot 'SldConnectionTests.cs')
if ($LASTEXITCODE -ne 0) {
    throw 'No se pudieron compilar las pruebas.'
}

& $testExecutable
if ($LASTEXITCODE -ne 0) {
    throw 'Fallaron las pruebas de conexion SLD.'
}
