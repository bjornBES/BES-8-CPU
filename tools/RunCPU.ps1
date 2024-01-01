Set-Location .\emu

dotnet build

Set-Location ..

./emu\bin\Debug\net7.0\emu $args

Write-Host $LASTEXITCODE