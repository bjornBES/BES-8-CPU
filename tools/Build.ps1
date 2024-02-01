$PATH_ASM = "asm"
$PATH_BAS = "basic"
$PATH_COM = "compiler"
$PATH_EUM = "emu"
$CS_SRC = "src/CS"

$USER_INPUT = $args[0]

Clear-Host

Set-Location ./$CS_SRC
if ($USER_INPUT -eq "") {
    Exit-PSHostProcess
}

switch ($USER_INPUT) {
    $PATH_ASM {  
        Build_ASSEMBLER
        Set-Location ..
    }
    $PATH_BAS {  
        Build_BASIC
        Set-Location ..
    }
    $PATH_COM {  
        Build_COMPILER
        Set-Location ..
    }
    $PATH_EUM {  
        Build_EMU
        Set-Location ..
    }
    "ALL" {
        BuildPROGARM $PATH_ASM
        BuildPROGARM $PATH_BAS
        BuildPROGARM $PATH_COM
        BuildPROGARM $PATH_EUM
        Set-Location ..
        Set-Location ..
    }
    Default {}
}
MoveProgram

Set-Location C:\Users\bjorn\Desktop\CPUs\BES-8-CPU

Exit-PSHostProcess

function MoveProgram {
    Get-Location 
    Move-Item -Path "./$CS_SRC/$PATH_ASM/bin/Debug/net7.0" -Destination "./tools/$PATH_ASM" -Force
    Move-Item -Path "./$CS_SRC/$PATH_BAS/bin/Debug/net7.0" -Destination "./tools/$PATH_BAS" -Force
    Move-Item -Path "./$CS_SRC/$PATH_COM/bin/Debug/net7.0" -Destination "./tools/$PATH_COM" -Force
    Move-Item -Path "./$CS_SRC/$PATH_EUM/bin/Debug/netcoreapp3.1" -Destination "./tools/$PATH_EUM" -Force
}

function BuildPROGARM {
    param (
        $PPath
    )
    Set-Location ./$PPath
    dotnet build ./$PPath.csproj -v q
    Write-Host "for $PPath ./$PPath.csproj EXIT CODE $LASTEXITCODE"
    Set-Location ..
}
function Build_ASSEMBLER {
    Set-Location .\$PATH_ASM
    dotnet build
    Write-Host "$PATH_ASM EXIT CODE $LASTEXITCODE"
    Set-Location ..
}

function Build_BASIC {
    Set-Location .\$PATH_BAS
    dotnet build
    Write-Host "$PATH_BAS EXIT CODE $LASTEXITCODE"
    Set-Location ..
}

function Build_COMPILER {
    Set-Location .\$PATH_COM
    dotnet build
    Write-Host "$PATH_COM EXIT CODE $LASTEXITCODE"
    Set-Location ..
}

function Build_EMU {
    Set-Location .\$PATH_EUM
    dotnet build
    Write-Host "$PATH_EUM EXIT CODE $LASTEXITCODE"
    Set-Location ..
}