Set-Location .\asm

dotnet build

Set-Location ..

./asm\bin\Debug\net7.0\asm $args

Write-Host $LASTEXITCODE