NOP dose not thing <br>
MOV Move the value from the ``source`` to ``destination``<br>**operand1**``destination``<br>**operand2**``source``
PUSH Push the value from ``source`` onto the stack and decrease the stack pointer<br>**operand1**``source``
POP Increase the stack pointer and pop a value off the stack into the ``destination``<br>**operand1**``destination``
ADD Add the value from the ``source`` to the ``destination`` and store the result in the ``destination``<br>**operand1**``destination``<br>**operand2**``source``
SUB Subtract the value from the ``source`` from the ``destination`` and store the result in the ``destination``<br>**operand1**``destination``<br>**operand2**``source``
MUL Multiply the value in ``source`` by the value in ``destination`` and store the result in ``destination``<br>**operand1**``destination``<br>**operand2**``source``
DIV Divide the value in ``source`` by the value in ``destination``, storing the quotient in ``destination`` and the remainder in ``source``<br>**operand1**``destination``<br>**operand2**``source``
AND Bitwise AND operation between the values in ``source`` and ``destination``, storing the result in ``destination``<br>**operand1**``destination``<br>**operand2**``source``
OR Bitwise OR operation between the values in ``source`` and ``destination``, storing the result in ``destination``<br>**operand1**``destination``<br>**operand2**``source``
NOT Bitwise NOT operation on the value in ``destination``, storing the result in ``destination``<br>**operand1**``destination``
NOR Bitwise NOR operation between the values in ``source`` and ``destination``, storing the result in ``destination``<br>**operand1**``destination``<br>**operand2**``source``
ROL Rotate the bits in the ``destination`` right by the number of positions specified in ``operand1``. The CARRY flag is used in the rotation<br>**operand1**``destination``<br>**operand2**``operand1``
ROR Rotate the bits in the ``destination`` left by the number of positions specified in ``operand1``. The CARRY flag is used in the rotation<br>**operand1**``destination``<br>**operand2**``operand1``
JMP Jump to the specified ``address``<br>**operand1**``address``
CMP Compare ``operand1`` and ``operand2`` and set flags in the Flags Register for conditional jumps<br>**operand1**``operand1``<br>**operand2**``operand2``
JLE Jump to the specified ``address`` if less than or equal to (<=) flag is set<br>**operand1**``address``
JE Jump to the specified ``address`` if equal (==) flag is set<br>**operand1**``address``
JGE Jump to the specified ``address`` if less than or equal to (>=) flag is not set<br>**operand1**``address``
JG Jump to the specified ``address`` if less than (>) flag is not set<br>**operand1**``address``
JNE Jump to the specified ``address`` if not equal (!=) flag is set<br>**operand1**``address``
JL Jump to the specified ``address`` if lees (<) flag is set<br>**operand1**``address``
JER Jump to the specified ``address`` if the ERROR flag is set<br>**operand1**``address``
JMC Jump to the specified ``address`` if the CARRY flag is set<br>**operand1**``address``
JMZ Jump to the specified ``address`` if the ZERO flag is set<br>**operand1**``address``
JNC Jump to the specified ``address`` if the CARRY flag is not set<br>**operand1**``address``
INT Generate a software interrupt with the specified ``interrupt number``<br>**operand1**``interrupt_number``
CALL Call a function at the specified ``address``<br>**operand1**``address``
RTS Return from a function without an offset<br>
RET Return from a function with the specified ``offset``<br>**operand1**``offset``
PUSHR Push all registers (AX, BX, CX, DX, ZX, X, Y) onto the stack <br>
POPR Pop all registers (Y, X, DX, CX, BX, AX) from the stack <br>
INC Increment the value in ``destination`` <br>**operand1**``destination``
DEC Decrement the value in ``destination`` <br>**operand1**``destination``
IN Input the value from the specified ``port`` into the ``destination``<br>**operand1**``port``<br>**operand2**``destination``
OUT Output the value in the ``source`` to the specified ``port``<br>**operand1**``port``<br>**operand2**``source``
CLF Clear the specified ``flag`` in the Flags Register<br>**operand1**``flag``
SEF Set the specified ``flag`` in the Flags Register<br>**operand1**``flag``
XOR Bitwise XOR operation between the values in ``source`` and ``destination``, storing the result in ``destination``<br>**operand1**``destination``<br>**operand2**``source``
JMS Jump to the specified ``address`` if the SING flag is set<br>**operand1**``address``
JNS Jump to the specified ``address`` if the SING flag is not set<br>**operand1**``address``
SHL Shift the bits in the ``destination`` left by the number of positions specified in ``operand1``. The CARRY flag receives the overflowed bit, and a zero bit is shifted in from the right.<br>**operand1**``destination``<br>**operand2**``operand1``
SHR Shift the bits in the ``destination`` right by the number of positions specified in ``operand1``. The CARRY flag receives the overflowed bit, and a zero bit is shifted in from the left.<br>**operand1**``destination``<br>**operand2**``operand1``
HALT Sets the HALT flag<br>
AX Is a ``16 bits GP-register`` that is build op of two 8 bit registers
BX Is a ``16 bits GP-register`` that is build op of two 8 bit registers
CX Is a ``16 bits GP-register`` that is build op of two 8 bit registers
DX Is a ``16 bits GP-register`` that is build op of two 8 bit registers
ZX Is a ``16 bits GP-register`` that is build op of two 8 bit registers
X Is a ``16 bits GP-register`` that is build op of two 8 bit registers
Y Is a ``16 bits GP-register`` that is build op of two 8 bit registers
R1 Is a ``16 bits GP-register`` that is build op of two 8 bit registers
R2 Is a ``16 bits GP-register`` that is build op of two 8 bit registers
R3 Is a ``16 bits GP-register`` that is build op of two 8 bit registers
R4 Is a ``16 bits GP-register`` that is build op of two 8 bit registers
MB The ``Memory Bank Register`` is an 16 bit bank register that controls what the bank is
SP The ``Stack Pointer`` is a 16 bit index register that Decrements when pushing a value on the stack and Incrementing when popping a value off the stack
PC The ``Program Counter`` is a 20 bit register that points to the current address of the program
F The ``Flags`` is a 16 bit register that holds information about the CPU
AL Is the low 8 bit part of the ``AX`` register
BL Is the low 8 bit part of the ``BX`` register
CL Is the low 8 bit part of the ``CX`` register
DL Is the low 8 bit part of the ``DX`` register
ZL Is the low 8 bit part of the ``ZX`` register
XL Is the low 8 bit part of the ``X`` register
YL Is the low 8 bit part of the ``Y`` register
R1L Is the low 8 bit part of the ``R1`` register
R2L Is the low 8 bit part of the ``R2`` register
R3L Is the low 8 bit part of the ``R3`` register
R4L Is the low 8 bit part of the ``R4`` register
AH Is the high 8 bit part of the ``AX`` register
BH Is the high 8 bit part of the ``BX`` register
CH Is the high 8 bit part of the ``CX`` register
DH Is the high 8 bit part of the ``DX`` register
ZH Is the high 8 bit part of the ``ZX`` register
XH Is the high 8 bit part of the ``X`` register
YH Is the high 8 bit part of the ``Y`` register
R1H Is the high 8 bit part of the ``R1`` register
R2H Is the high 8 bit part of the ``R2`` register
R3H Is the high 8 bit part of the ``R3`` register
R4H Is the high 8 bit part of the ``R4`` register