.org 7000h
.word 7000h
.include "test.basm"
.repeat .word 7000h

$Test = 100h

mov [BX] %Test

mov AX #10h
mov [TEST] #10h

mov [7000h]

TEST: