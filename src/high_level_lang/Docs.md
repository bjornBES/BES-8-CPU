# Introduction

This language is a faster way to write code to the BES-8 then assembly.  

## contains

### nullptr

the nullptr is a constant that has been initiated to zero and will delete a pointer.

``` BEc
const ptr int nullptr = new ptr 0 
```

## data types

### int

number from 0 to 16 bits

``` BEc
// syntax
int Name = value
```

### long int

number from 0 to 32 bits

``` BEc
// syntax
long int Name = value
```

### ptr

this is a pointer, the user can set the point from 0x9000 to 0x8FFF

#### syntax

``` BEc
// initiate 

// a pointer dont need a type

ptr int Name = 9000h        // here we initiate an int pointer to 9000 hex

ptr Name = 9000h            // here we initiate a pointer to 9000 hex casted as an int
```

Here we are initiating a new pointer called ``` Name ``` that we are setting to point at ``` 9000h ``` hex.

the first pointer that is been initated have a type that is ``` int ``` this is not needed,
but if it have an ``` int ``` type,
that means it can hold up to 16 bits of data form hex ``` 0000 ``` to hex ``` FFFF ```

``` BEc
// setting a value to the pointed address
ptr int Running = 9000h     // here we initiate the Running pointer to point at 9000 hex

Running = 1234              // here we store 1234 to the pointed address the Running pointer
```

Here we are initiating pointer called ``` Running ``` .

``` BEc
// reinitiate
Running = new ptr 9001h     // here we reinitiate the Running pointer to point at 9001 hex
```

``` BEc
// freeing a pointer
Running = new ptr nullptr
```

### char

a char is an 8 bit value, it can be hold an ascii char

``` BEc
syntax
```
