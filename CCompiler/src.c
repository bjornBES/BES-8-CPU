// Function prototypes
void exampleFunction(int);

int globalVariabel = 10;

int main(char disk, char sector)
{
    // Variable declarations
    int test1 = sector;
    int test2 = disk;
    int num1 = 10;
    int num2 = 20;
    char letter = 'A';
    //float floatValue = 3.14;
    
    // Arithmetic operations
    int sum = num1 + num2;
    int difference = num1 - num2;
    int product = num1 * num2;
    //float division = (float)num1 / num2; // Type casting
    
    // Conditional statements
    // if ( Expr [Operater] Expr )
    // if ( Expr (== 1) )
    if (sum == num2) 
    {
        int test = 0;
    } 
    else if (num1 != num2) 
    {
        int test2 = 0;
    } 
    else 
    {
        char test3 = 'A';
    }
    /*
    // Loops
    for (int i = 0; i < 5; i++) {
    }
    
    */
    int count = 0;
    while (count < 5) {
        count++;
    }
    
    // Function call
    exampleFunction(num2, '0');
    
    return 0;
}

// Function definition
void exampleFunction(int value, char test) {
    
}
