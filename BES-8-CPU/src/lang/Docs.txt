# Introduction

This language provides a faster alternative to writing code for the BES-8 than assembly language. It aims to offer improved productivity and readability while maintaining performance.

## Expressions

Expressions in this language are fundamental building blocks of code, representing computations or operations on data.

Certainly! Here's the combined Operators section with the requested operators:

## Operators

Operators are symbols that perform operations on one or more operands. They are the building blocks for creating expressions.

### Arithmetic Operators

Arithmetic operators perform arithmetic operations on numeric operands.

- `+` (Addition): Adds two operands.
  
``` BEc
syntax
word result = operand1 + operand2;
```

- `-` (Subtraction): Subtracts the second operand from the first.

``` BEc
syntax
word result = operand1 - operand2;
```

- `*` (Multiplication): Multiplies two operands.

``` BEc
syntax
word result = operand1 * operand2;
```

- `/` (Division): Divides the first operand by the second.

``` BEc
syntax
word result = operand1 / operand2;
```

- `%` (Modulus): Returns the remainder of the division of the first operand by the second.

``` BEc
syntax
word remainder = operand1 % operand2;
```

### Comparison Operators

Comparison operators compare two operands and return a boolean value indicating the result of the comparison.

- `==` (Equals): Checks if two operands are equal.

``` BEc
syntax
bool isEqual = operand1 == operand2;
```

- `!=` (Not Equals): Checks if two operands are not equal.

``` BEc
syntax
bool isNotEqual = operand1 != operand2;
```

- `>` (Greater Than): Checks if the first operand is greater than the second.

``` BEc
syntax
bool isGreaterThan = operand1 > operand2;
```

- `<` (Less Than): Checks if the first operand is less than the second.

``` BEc
syntax
bool isLessThan = operand1 < operand2;
```

- `<=` (Less or Equals): Checks if the first operand is less than or equal to the second.

``` BEc
syntax
bool isLessOrEqual = operand1 <= operand2;
```

### Logical Operators

Logical operators perform logical operations on boolean operands.

- `&&` (Logical AND): Returns true if both operands are true.

``` BEc
syntax
bool result = operand1 && operand2;
```

- `||` (Logical OR): Returns true if at least one operand is true.

``` BEc
syntax
bool result = operand1 || operand2;
```

- `!` (Logical NOT): Returns true if the operand is false, and vice versa.

``` BEc
syntax
bool result = !operand;
```

### Bitwise Operators

Bitwise operators perform bit-level operations on integer operands.

- `&` (Bitwise AND): Performs a bitwise AND operation.

``` BEc
syntax
word result = operand1 & operand2;
```

- `|` (Bitwise OR): Performs a bitwise OR operation.

``` BEc
syntax
word result = operand1 | operand2;
```

- `^` (Bitwise XOR): Performs a bitwise XOR (exclusive OR) operation.

``` BEc
syntax
word result = operand1 ^ operand2;
```

- `~` (Bitwise NOT): Performs a bitwise NOT (one's complement) operation.

``` BEc
syntax
word result = ~operand;
```

- `<<` (Left Shift): Shifts the bits of the first operand to the left by the number of positions specified by the second operand.

``` BEc
syntax
word result = operand1 << operand2;
```

- `>>` (Right Shift): Shifts the bits of the first operand to the right by the number of positions specified by the second operand.

``` BEc
syntax
word result = operand1 >> operand2;
```

### Assignment Operators

Assignment operators assign a value to a variable or operand.

- `=` (Assignment): Assigns the value of the right operand to the left operand.

``` BEc
syntax
word variable = value;
```

- `+=` (Addition Assignment): Adds the value of the right operand to the left operand and assigns the result to the left operand.

``` BEc
syntax
word variable += value;
```

- `-=` (Subtraction Assignment): Subtracts the value of the right operand from the left operand and assigns the result to the left operand.

``` BEc
syntax
word variable -= value;
```

- `*=` (Multiplication Assignment): Multiplies the value of the right operand with the left operand and assigns the result to the left operand.

``` BEc
syntax
word variable *= value;
```

- `/=` (Division Assignment): Divides the value of the left operand by the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable /= value;
```

- `%=` (Modulus Assignment): Computes the modulus of the left operand with the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable %= value;
```

- `&=` (Bitwise AND Assignment): Computes the bitwise AND of the left operand with the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable &= value;
```

- `|=` (Bitwise OR Assignment): Computes the bitwise OR of the left operand with the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable |= value;
```

- `^=` (Bitwise XOR Assignment): Computes the bitwise XOR of the left operand with the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable ^= value;
```

- `<<=` (Left Shift Assignment): Shifts the bits of the left operand to the left by the number of positions specified by the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable <<= value;
```

- `>>=` (Right Shift Assignment): Shifts the bits of the left operand to the right by the number of positions specified by the right operand and assigns the result to the left operand.

``` BEc
syntax
word variable >>= value;
```

These operators enable you to perform various operations, comparisons, and assignments in your code, enhancing the flexibility and expressiveness of your programs.

## Keywords

Keywords are reserved words in the language that have special meanings and cannot be used as identifiers.

### Literals

#### word

A word is a literal representing a number with a size ranging from 0 to 16 bits that is signed.

``` BEc
syntax
word Number = -65534; // Declaring a word variable named Number with a value of -65534
```

#### Uword

A Uword is a literal representing a number with a size ranging from 0 to 16 bits that is unsigned.

``` BEc
syntax
Uword Number = 65535; // Declaring a Uword variable named Number with a value of 65535
```

#### byte

A byte is a literal representing a number with a size ranging from 0 to 8 bits.

``` BEc
syntax
byte ByteValue = 127; // Declaring a byte variable named ByteValue with a value of 127
```

#### Int

A int is a literal representing a number with a size ranging from 0 to 20 bits.

``` BEc
syntax
int IntNumber = 10000; // Declaring a long variable named IntNumber with a value of 10000
```

#### ptr

A ptr is a literal representing a pointer. It allows the user to set the pointer to a memory address within the specified range (0x9000 to 0x8FFF).

``` BEc
syntax
ptr MemoryPointer = 0x9000; // Initializing a pointer named MemoryPointer to 0x9000
MemoryPointer = 1234; // Storing the value 1234 at the memory address pointed by MemoryPointer
MemoryPointer = new ptr 0x9001; // Reinitiating the pointer to point at 0x9001
MemoryPointer = new ptr nullptr; // Freeing the pointer by assigning nullptr
```

#### char

A char is an 8-bit value that can hold an ASCII character.

``` BEc
syntax
char Character = 'A'; // Declaring a char variable named Character with the value 'A'
```

Certainly! Here's the additional content for the provided document:

## Statements

Statements are executable units of code that perform actions. They are the building blocks for constructing programs in this language.

### if, else if, else

Conditional statements that allow the execution of different blocks of code based on specified conditions.

``` BEc
syntax
int condition = /* some value */;

if (condition == 0) {
    // Code block executed if 'condition' is equal to 0
} else if (condition == 1) {
    // Code block executed if 'condition' is equal to 1
} else {
    // Code block executed if 'condition' is neither 0 nor 1
}
```

### free (ptr)

The `free` function deallocates memory associated with a pointer variable, preventing memory leaks and optimizing resource utilization.

#### Syntax

``` BEc
syntax
free(ptr variable);
```

#### Parameters

- `variable`: A pointer variable that points to a memory location.

#### Description

The `free` function releases the memory associated with the specified pointer variable. This ensures that the allocated memory is returned to the system for reuse, preventing potential memory leaks.

#### Example

``` BEc
ptr MemoryPointer = &someVariable; // Initializing a pointer to the address of someVariable
// Perform operations with MemoryPointer
free(MemoryPointer); // Freeing the associated memory
```

In this example, the `free` function is used to release the memory pointed to by `MemoryPointer`, which was initialized with the address of `someVariable`. After calling `free`, the pointer is set to `nullptr`, indicating that the memory is now available for reuse.

**Note:** It is crucial to use the `free` function responsibly and only on memory that has been dynamically allocated or obtained through the address-of operator (`&`). Improper use may lead to undefined behavior and memory corruption.

### exit (word)

The `exit` function is used to terminate the program with a specified exit code. This function provides a controlled and orderly way to end the program's execution.

``` BEc
syntax
exit(word exitCode);
```

#### Parameters

- `exitCode`: A word literal representing the exit code of the program.

#### Description

The `exit` function terminates the program and returns the specified exit code to the operating system. The exit code is a way for the program to communicate its termination status to the calling process or the environment.

#### Example

``` BEc
exit(0); // Terminate the program with an exit code of 0 (indicating success)
```

In this example, the `exit` function is called with an exit code of 0, indicating a successful program execution. The exit code can be checked by other programs or scripts that invoke the current program.

**Note:** It's common to use a non-zero exit code to indicate an error or abnormal termination of the program. Developers can choose meaningful exit codes based on their application's needs.

### return

A return statement is used to exit a function and return a value.

``` BEc
syntax
int add(int a, int b) {
    return a + b;
}
```

### continue

The `continue` statement is used in loops to skip the rest of the code inside the loop for the current iteration and move to the next iteration.

``` BEc
syntax
for (int i = 0; i < 10; ++i) {
    if (i % 2 == 0) {
        continue; // Skip even numbers
    }
    // Code here will be skipped for even values of i
}
```

### break

The `break` statement is used to exit a loop or switch statement prematurely.

``` BEc
syntax
while (true) {
    // Code here will be executed indefinitely
    if (/* some condition */) {
        break; // Exit the loop if the condition is met
    }
}
```

### for

The `for` statement is used for iterative control flow, often for looping through a range of values.

``` BEc
syntax
for (word i = 0; i < 5; ++i) {
    // Code block executed five times
}
```

### while

The `while` statement is used for repetitive execution of a block of code as long as a specified condition is true.

``` BEc
syntax
int i = 0;
while (i < 5) {
    // Code block executed as long as i is less than 5
    ++i;
}
```

## Librarys


### Standard Library

The Standard Library in this programming language includes a set of predefined functions, constants, and types that developers can leverage to simplify common tasks and enhance code readability.

#### bool

A bool is a boolean literal representing true or false.

``` BEc
syntax
bool Bool = false;
bool TrueBool = true;
```

#### Constants

##### nullptr

The nullptr constant is a special value initialized to zero and is used to represent a null pointer. When assigned to a pointer, it signifies that the pointer does not point to any valid memory location.

``` BEc
syntax
ptr Running = nullptr; // Initializing the pointer Running to a null value
```

##### true

``` BEc
syntax
const long trueValue = 1; // Declaring a constant named trueValue with a value of 1
```

##### false

``` BEc
syntax
const long falseValue = 0; // Declaring a constant named falseValue with a value of 0
```

### BIOS Library

the BIOS Library is the way to communicate with the BIOS

#### Port

### Other Standard Library Components

- [Include relevant functions, data structures, or utilities available in the standard library.]
