Set-Location .\basic

dotnet build

Set-Location ..

./basic\bin\Debug\net7.0\basic $args

Write-Host $LASTEXITCODE