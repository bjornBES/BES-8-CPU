# CPU Specifications:

1. **Name:** BES-8-CPU
2. **Data Bus:** 20 bits
3. **Address Bus:** 20 bits
4. **Registers:**
   - **General-Purpose Registers:**
      - **AX:** 16 bits (can be split into **AL** and **AH** - 8 bits each)
      - **BX:** 16 bits (can be split into **BL** and **BH** - 8 bits each)
      - **CX:** 16 bits (can be split into **CL** and **CH** - 8 bits each)
      - **DX:** 16 bits (can be split into **DL** and **DH** - 8 bits each)
      - **ZX:** 16 bits (can be split into **ZL** and **ZH** - 8 bits each)
      - **X:** 16 bits (can be split into **XL** and **XH** - 8 bits each)
      - **Y:** 16 bits (can be split into **YL** and **YH** - 8 bits each)
   - **Flags Register:**
       0. ZERO
        - 
       1. EQUAL
        - 
       2. LESS
        - 
       3. CARRY
        - 
       4. HALT
        - 
       5. SING
        - 
       6. PARITY
        - 
       7. ERROR
        -
   - **Stack Pointer (SP):** 
      - 20 bits
      - The stack pointer keeps track of the top of the stack in memory.
      - Stack operations (PUSH and POP) affect the stack pointer.
   - **Program Counter (PC):** 
      - 20 bits
      - The program counter holds the memory address of the next instruction to be executed.
      - It increments automatically after fetching each instruction.
5. **Core Count:** 0
6. **Clock Speed:** 0 (MHz)
7. **Cache:**
  - Cache 1:
      - Size: **131071** bytes
      - Type: File Reading
      - Location: Bank 2 (``0x10000 to 0x2FFFF``)
8. **Memory Layout and Ports:**
  - Bank 0: 
    - ROM Range: ``0x00000 to 0xFFFFF``
  - Bank 1:
    - Memory-Based Ports Range: ``0x00000 to 0x001FF``
  - Bank 2:
    - Cache: Range: ``0x10000 to 0x2FFFF``
9.  **Instruction Set:**

   1.   **NOP** 
        - No Operation

   2.   **MOV** `source`, `destination`
        - Move the value from `source` to `destination` `(destination can be an address or a register)`

   3.   **PUSH** `source`
        - Push the value from `source` onto the stack and decrease the stack pointer

   4.   **POP** `destination`
        - Pop a value off the stack into the `destination` and increase the stack pointer

   5.   **ADD** `source`, `destination`
         - Add the value from the `source` to the `destination` and store the result in the `destination`

   6.   **SUB** `source`, `destination`
        - Subtract the value from the `source` from the `destination` and store the result in the `destination`

   7.   **MUL** `source`, `destination`
        - Multiply the value in `source` by the value in `destination` and store the result in `destination`

   8.   **DIV** `source`, `destination`
        - Divide the value in `source` by the value in `destination`, storing the quotient in `destination` and the remainder in `source`

   9.   **AND** `source`, `destination`
        - Bitwise AND operation between the values in `source` and `destination`, storing the result in `destination`

   10.  **OR** `source`, `destination`
        - Bitwise OR operation between the values in `source` and `destination`, storing the result in `destination`

   11.  **NOT** `source`
        - Bitwise NOT operation on the value in `source`, storing the result in `source`

   12.  **NOR** `source`, `destination`
        - Bitwise NOR operation between the values in `source` and `destination`, storing the result in `destination`

   13.  **ROL** `source`, `destination`
        - Rotate the bits in `source` to the left by the number of positions specified in `destination`

   14.  **ROR** `source`, `destination`
        - Rotate the bits in `source` to the right by the number of positions specified in `destination`

   15.  **JMP** `address`
        - Jump to the specified `address`

   16.  **CMP** `operand1`, `operand2`
        - Compare `operand1` and `operand2` and set flags in the Flags Register for conditional jumps

   17.  **JLE** `address`
        - Jump to the specified `address` if less than or equal to (<=) flag is set

   18.  **JE** `address`
         - Jump to the specified `address` if equal (==) flag is set

   19.  **JGE** `address`
         - Jump to the specified `address` if less than or equal to (>=) flag is not set

   20.  **JG** `address`
         - Jump to the specified `address` if less than (>) flag is not set

   21.  **JNE** `address`
         - Jump to the specified `address` if not equal (!=) flag is set

   22.  **JL** `address`
         - Jump to the specified `address` if lees (<) flag is set

   23.  **JER** `address`
         - Jump to the specified `address` if the ERROR flag is set

   24.  **INT** `interrupt_number`
         - Generate a software interrupt with the specified interrupt number

   25.  **CALL** `address`
        - Call a function at the specified ``address``

   26.  **RTS**
         - Return from a function without an offset

   27.  **RET** `offset`
         - Return from a function with the specified ``offset``

   28.  **PUSHR**
         - Push all registers (AX, BX, CX, DX, ZX, X, Y) onto the stack

   29.  **POPR**
         - Pop all registers (Y, X, DX, CX, BX, AX) from the stack

   30.  **INC** `destination`
         - Increment the value in `destination`

   31.  **DEC** `destination`
         - Decrement the value in `destination`

   32.  **IN** `port`, `operand`
         - Input the value from the specified `port` into the `operand`

   33.  **OUT** `port`, `operand`
         - Output the value in the `operand` to the specified `port`

   34.  **CLF** `flag`
         - Clear the specified ``flag`` in the Flags Register

   35.  **SEF** `flag`
         - Set the specified ``flag`` in the Flags Register
  
1.   **BIOS Interrupts**

| Number  |       NAME     | Description|AL|AH|BL|BH|CL|CH|DL|DH|
|---------|----------------|------------|--|--|--|--|--|--|--|--|
| **0x0** | Interrupt Name | BISO Interrupt 1         |OP AL|OP AH|OP BL|OP BH|OP CL|OP CH|OP DL|OP DH
| **0x1** | Interrupt Name | BISO Interrupt 2         |OP AL|OP AH|OP BL|OP BH|OP CL|OP CH|OP DL|OP DH
| **0x2** | Interrupt Name | BISO Interrupt 3         |OP AL|OP AH|OP BL|OP BH|OP CL|OP CH|OP DL|OP DH
| **0x3** | Interrupt Name | BISO Interrupt 4         |OP AL|OP AH|OP BL|OP BH|OP CL|OP CH|OP DL|OP DH
| **0x4** | Interrupt Name | BISO Interrupt 5         |OP AL|OP AH|OP BL|OP BH|OP CL|OP CH|OP DL|OP DH
|**...**  | Interrupt Name | BISO Interrupt 6         |     |     |     |     |     |     |     |     
