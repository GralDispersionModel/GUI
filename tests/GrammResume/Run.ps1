param([Parameter(Mandatory=$true)][string]$WorkDirectory)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$out=[IO.Path]::GetFullPath($WorkDirectory)
if(Test-Path -LiteralPath $out){throw 'Use a new empty work directory.'}
New-Item -ItemType Directory -Path $out | Out-Null
function CheckExit { if($LASTEXITCODE -ne 0){throw "Command failed: $LASTEXITCODE"} }
dotnet build (Join-Path $root 'src/Gral.csproj') -c Release -o (Join-Path $out 'gui') --nologo
CheckExit
foreach($name in @('GrammResume','GrammCpuScheduler','GrammResumeRunner','GrammResumeGui')){
 $test=Join-Path $out $name
 dotnet build (Join-Path $root "tests/$name/$name.csproj") -c Release -o $test "-p:GralGuiAssembly=$out/gui/GRAL_GUI.dll" --nologo
 CheckExit
 & (Join-Path $test "$name.exe") (Join-Path $out "results/$name")
 CheckExit
}
