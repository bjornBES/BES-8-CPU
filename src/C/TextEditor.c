typedef unsigned short ushort;

ushort Running = 0xd000;
ushort CursorX = 0;
ushort CursorY = 0;
const ushort UserPort = 0x002; 
const ushort Screen = 0x001; 
int main()
{
    OutByte(0b01011000, 0);

    Start();

    OutByte(0b01010000, 0);

    if (Running != 1)
    {
        return 1;
    }

    return 0;
}

void Start()
{
    Running = 0; 

    OutByte(0x0101, UserPort);
    ushort Value = 0;
    InByte(UserPort, &Value);
    if (Value != 1)
    {
        OutByte(0b1000, 0);
    }
    OutByte(Screen, 0x01FF);
    while (Running == 1)
    {
        InByte(UserPort, Value);
        if (Value == 0)
        {
            Enter();
            Value = 0;
            continue;
        }
        if (Value == 0)
        {
            Escape();
            Value = 0;
            continue;
        }
        if (Value == 0)
        {
            BackSpace();
            Value = 0;
            continue;
        }
        OutByte(&Value, Screen);
        CursorX++;
    }
    Running = 0xFF;
}

void Enter()
{
    CursorY++;
    CursorX = 0;
    MoveCursor();
}
void Escape()
{
    Running = 0;
}
void BackSpace()
{
    CursorX--;
    MoveCursor();
    OutByte(0x20, Screen);
    MoveCursor();
}

void MoveCursor()
{
    ushort Instr = CursorY;

    Instr |= 0b00000011;
    OutByte(Instr, Screen);

    Instr = CursorX;
    Instr |= 0b00000101;
    OutByte(Instr, Screen);
}

/// @brief Outputs some Data to a port
/// @param Data the data beening outputed
/// @param port the port beening ref
void OutByte(ushort Data, ushort port)
{
    ushort* addr = (ushort*)0x8000 + port;
    *addr = Data;
}
/// @brief Inputs some Data from a port
void InByte(ushort port, ushort* Ret)
{
    ushort* addr = (ushort*)0x8000 + port;
    *Ret = (ushort)addr;
}