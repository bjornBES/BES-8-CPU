$PATH_ASM = "assembler"
$PATH_BAS = "Basic_Interpreter"
$PATH_COM = "compiler"
$PATH_EUM = "emu"

$USER_INPUT = $args[0]


Set-Location ..

switch ($USER_INPUT) {
    $PATH_ASM {  
        Build_ASSEMBLER
    }
    $PATH_BAS {  
        Build_BASIC
    }
    $PATH_COM {  
        Build_COMPILER
    }
    $PATH_EUM {  
        Build_EMU
    }
    "ALL" {
        BuildPROGARM
    }
    Default {}
}

Set-Location C:\Users\bjorn\Desktop\CPUs\BES-8-CPU

Exit-PSHostProcess

function BuildPROGARM {
    Build_ASSEMBLER
    Build_BASIC
    Build_COMPILER
    Build_EMU
}
function Build_ASSEMBLER {
    Set-Location .\$PATH_ASM
    ./build.ps1
    Write-Host "$PATH_ASM EXIT CODE $LASTEXITCODE"
}

function Build_BASIC {
    Set-Location .\$PATH_BAS
    ./build.ps1
    Write-Host "$PATH_BAS EXIT CODE $LASTEXITCODE"
}

function Build_COMPILER {
    Set-Location .\$PATH_COM
    ./build.ps1
    Write-Host "$PATH_COM EXIT CODE $LASTEXITCODE"
}

function Build_EMU {
    Set-Location .\$PATH_EUM
    ./build.ps1
    Write-Host "$PATH_EUM EXIT CODE $LASTEXITCODE"
}