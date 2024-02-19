" JDH-8 ASSEMBLER SYNTAX FILE

if exists("b:current_syntax")
    finish
  endif

  syn keyword BasmReg AX AL AH BX BL BH CX CL CH DX DL DH ZX ZL ZH PC SP MB X XL XH Y YL YH F
  syn keyword BasmTODO contained TODO

  syn match BasmMarco           "@[A-Za-z]\+\s%[i a r]\+"

  syn match BasmComment         ";\s.*$" contains=BasmTODO
  syn match BasmArgsComment     ";\sarg\s[0-9]\+\s[A-Za-z]\+\sStack"
  syn match BasmReturnREComment ";\sreturn\s[A-Za-z]\+\sin\s[A-Z]\+"

  syn match BasmDirective       "^\s*[.][bits word byte org bits str strz]\+"
  
  syn match BasmNumber          "[0-9A-F]\+[bh]"
  syn match BasmNumber          "[#&][0-9A-F]\+"
  syn match BasmNumber          "[#&][0-9A-F]\+[bh]"

  syn match BasmInstr           "[a-zA-Z]\+\s"
  syn match BasmKeyBoardWord    "#[NULL KW_BS KW_SP KW_ENT KW_ESC]\+"
  
  syn match BasmLabelName       "^[a-zA-Z0-9_\.]\+:"
  syn match BasmLabelName       ".global\s[a-zA-Z0-9_\.]\+:"

  syn match BasmIndexed         "\[[A-Za-z]\+\],"
  syn match BasmIndexed         "\[[A-Za-z]\+\],[0-9]\+"

  syn match BasmAtAddr          "[$]"
  syn match BasmAtAddr          "[$][+-][0-9]\+"

  syn match BasmUseVariabels    "%[A-Za-z]\+"

  syn match BasmVariabels       "$[A-Za-z]\+\s[= #=]"
  
  
  syn region BasmChar           start="#\'" end='\'' 
  syn region BasmString         start='"' end='"'
  syn region BasmAddr           start='\[' end='\]'
  
  
  let b:current_syntax = "basm"

  hi BasmVariabels ctermfg=Yellow
  
  hi BasmAddr ctermfg=DarkMagenta
  hi BasmChar ctermfg=DarkMagenta
  hi BasmNumber ctermfg=DarkMagenta
  
  hi BasmReg ctermfg=DarkGray
  
  hi BasmAtAddr ctermfg=DarkMagenta
  
  hi BasmIndexed ctermfg=DarkMagenta

  hi BasmUseVariabels ctermfg=DarkYellow
  hi BasmKeyBoardWord ctermfg=DarkYellow
  
  hi BasmMarco ctermfg=LightYellow
  " instrs
  hi def link BasmInstr Statement
  
  hi def link BasmString String
  
  hi BasmArgsComment ctermfg=LightGreen ctermbg=DarkGreen
  hi BasmReturnREComment ctermfg=LightGreen ctermbg=DarkGreen

  hi BasmComment ctermfg=DarkGreen
  hi BasmTODO ctermfg=LightYellow ctermbg=Yellow
  hi BasmLabelName ctermfg=LightYellow
  hi BasmDirective ctermfg=DarkRed