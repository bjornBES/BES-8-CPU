" JDH-8 ASSEMBLER SYNTAX FILE

if exists("b:current_syntax")
    finish
  endif
  
  syn keyword BasmReg AX AL AH A BX BL BH B CX CL CH C DX DL DH D ZX ZL ZH Z PC SP MB X XL XH YL YH EAX EBX F
  syn keyword BasmTODO contained TODO
  syn match BasmComment ";.*$" contains=BasmTODO
  syn match BasmDirective "^\s*[.][a-zA-Z]\+"
  syn match BasmNumber "[0-9]\+"
  syn match BasmNumber "[#&][0-9]\+"
  syn match BasmNumber "[0-9A-F]\+[hb]"
  syn match BasmNumber "[#&][0-9A-F]\+[hb]"
  syn match BasmVariabal "[$][A-Za-z]\+ [*=]"
  syn match BasmVariabal "[$][A-Za-z]\+ [=]"
  syn match BasmUsedVariabal "[%][A-Za-z]\+"
  syn match BasmInstrs "[a-zA-Z]\+"
  syn match BasmToAddr "[$]"
  
  " syn region jdhOp start='^' end='$'
  syn region BasmLabel start="[a-zA-Z0-9_.]" end=":" oneline contains=BasmLabelName
  syn region BasmString start='"' end='"'
  
  syn match BasmLabelName "^[a-zA-Z0-9_\.]\+:\=" contained
  syn match BasmAddrLabel '[a-zA-Z0-9_\.]\+' contained
  
  let b:current_syntax = "basm"
  hi def link BasmInstrs Function
  hi def link BasmTODO Todo
  hi def link BasmComment Comment
  hi def link BasmLabelName Label
  hi def link BasmAddrLabel Label
  hi def link BasmDirective Macro
  hi def link BasmVariabal Macro
  hi def link BasmNumber Number
  hi def link BasmUsedVariabal Label
  hi def link BasmReg Identifier
  hi def link BasmToAddr Number
  hi def link BasmString String