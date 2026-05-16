$unity = "C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe"
$project = Join-Path $PSScriptRoot "Unity2DStarter"

Start-Process -FilePath $unity -ArgumentList @("-projectPath", $project)
