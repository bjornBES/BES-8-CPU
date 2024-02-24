const vscode = require('vscode');
const fs = require('fs');
const filePath = 'C:/Users/bjorn/Desktop/CPUs/Language/Bassembly/beassembly/src/instr_Tooltips.txt';

function activate(context) {
    console.log('Congratulations, your extension "basm" is now active!');
    const LSPDisposable = vscode.languages.registerCompletionItemProvider(
        { scheme: 'file', language: 'basm' },
        {
            provideCompletionItems,
        },
        '.',
        ' ',
    )
    context.subscriptions.push(LSPDisposable);
    const hoverProvider = vscode.languages.registerHoverProvider('basm', {
        provideHover(document, position, token) {
            // Determine the range of the token at the current position
            const wordRange = document.getWordRangeAtPosition(position);
            if (!wordRange) {
                return undefined;
            }

            // Read file content asynchronously
            return new Promise((resolve, reject) => {
                fs.readFile(filePath, 'utf8', (err, data) => {
                    if (err) {
                        console.error('Error reading file:', err);
                        reject(err);
                        return;
                    }

                    const instrText = data.split(/\r?\n/); // Use a regular expression to split by newline
                    const word = document.getText(wordRange);

                    let tooltipContent = new vscode.MarkdownString("");

                    for (let i = 0; i < instrText.length; i++) {
                        const line = instrText[i].split(" ")[0];
                        if (line === word.toUpperCase()) {
                            // Use the rest of the line as tooltip content
                            tooltipContent = new vscode.MarkdownString(instrText[i].replace(line + " ", ""));
                            tooltipContent.supportHtml = true;
                            break;
                        }
                    }
                    // Return the hover information
                    resolve(new vscode.Hover(tooltipContent, wordRange));
                });
            });
        }
    });

    context.subscriptions.push(hoverProvider);

    // Register a command
    let disposable = vscode.commands.registerCommand('beassembly.Hello', () => {
        // The code you want to run when the command is executed
        vscode.window.showInformationMessage('Hello, World!');
    });
    context.subscriptions.push(disposable);
}

let Result = [];
function provideCompletionItems(document, position, token, context) {
    // Retrieve the current word that the user is typing
    const PLine = document.lineAt(position.line - 1).text;
    const Line = document.lineAt(position).text.substr(0, position.character);
    Result = [];

    // Check if the word matches a predefined list of suggestions
    if (Line[0] === '.') {
        if (position.line > 11) {
            NewItem('.bits', vscode.CompletionItemKind.Field, "", "bits");
        }
        NewItem('.global', vscode.CompletionItemKind.Field, "", "global");
        NewItem('.include', vscode.CompletionItemKind.Field, "", "include");
        NewItem('.extern', vscode.CompletionItemKind.Field, "", "extern");
        NewItem('.str', vscode.CompletionItemKind.Field, "", "str");
        NewItem('.strz', vscode.CompletionItemKind.Field, "", "strz");
        NewItem('.word', vscode.CompletionItemKind.Field, "", "word");
        NewItem('.byte', vscode.CompletionItemKind.Field, "", "byte");
        createSnippet("Str", "Making a string", "str \"${1:Message}\"", "Making a string");
        createSnippet("Str_Messge", "Making a string and a lable", "${1:Lable}:\r\n.strz \"${2:Message}\"", "Making a string");
    }
    else {

        switch (Line.toUpperCase()) {
            case "MOV ":
                NewItem("AX", vscode.CompletionItemKind.Value, "16 bit register", "AX");
                createSnippet("Address", "Makes an address", "[${1:Number}],{2:}", "Makes an address");
                break;

            default:
                break;
        }

        Snippet();

        Instrs(Line);

        Registers(Line);
        if (Line.at(0) === '$' && Line.at(Line.length - 1) === ' ') {
            Result.push(new vscode.CompletionItem('=', vscode.CompletionItemKind.Operator))
            Result.push(new vscode.CompletionItem('#=', vscode.CompletionItemKind.Operator))
            Result.push(new vscode.CompletionItem('*=', vscode.CompletionItemKind.Operator))
        }
    }
    return Result;
}

function Registers(line)
{
    if (line.toUpperCase().includes(' A')) {
        NewItem("AX", vscode.CompletionItemKind.Value, "16 bit register", "AX");
        NewItem("AL", vscode.CompletionItemKind.Value, "the low 8 bits part of AX", "AL");
        NewItem("AH", vscode.CompletionItemKind.Value, "the high 8 bits part of AX", "AH");
    }
    if (line.toUpperCase().includes(' B')) {
        NewItem("BX", vscode.CompletionItemKind.Value, "16 bit register", "BX");
        NewItem("BL", vscode.CompletionItemKind.Value, "the low 8 bits part of BX", "BL");
        NewItem("BH", vscode.CompletionItemKind.Value, "the high 8 bits part of BX", "BH");
    }
    if (line.toUpperCase().includes(' C')) {
        NewItem("CX", vscode.CompletionItemKind.Value, "16 bit register", "CX");
        NewItem("CL", vscode.CompletionItemKind.Value, "the low 8 bits part of CX", "CL");
        NewItem("CH", vscode.CompletionItemKind.Value, "the high 8 bits part of CX", "CH");
    }
    if (line.toUpperCase().includes(' D')) {
        NewItem("DX", vscode.CompletionItemKind.Value, "16 bit register", "DX");
        NewItem("DL", vscode.CompletionItemKind.Value, "the low 8 bits part of DX", "DL");
        NewItem("DH", vscode.CompletionItemKind.Value, "the high 8 bits part of DX", "DH");
    }
    if (line.toUpperCase().includes(' Z')) {
        NewItem("ZX", vscode.CompletionItemKind.Value, "16 bit register", "ZX");
        NewItem("ZL", vscode.CompletionItemKind.Value, "the low 8 bits part of ZX", "ZL");
        NewItem("ZH", vscode.CompletionItemKind.Value, "the high 8 bits part of ZX", "ZH");
    }
    if (line.toUpperCase().includes(' X')) {
        NewItem("X", vscode.CompletionItemKind.Value, "16 bit register", "X");
        NewItem("XL", vscode.CompletionItemKind.Value, "the low 8 bits part of X", "XL");
        NewItem("XH", vscode.CompletionItemKind.Value, "the high 8 bits part of X", "XH");
    }
    if (line.toUpperCase().includes(' Y')) {
        NewItem("Y", vscode.CompletionItemKind.Value, "16 bit register", "Y");
        NewItem("YL", vscode.CompletionItemKind.Value, "the low 8 bits part of Y", "YL");
        NewItem("YH", vscode.CompletionItemKind.Value, "the high 8 bits part of Y", "YH");
    }
    if (line.toUpperCase().includes(' R')) {
        NewItem("R1", vscode.CompletionItemKind.Value, "16 bit register", "R1");
        NewItem("R2", vscode.CompletionItemKind.Value, "16 bit register", "R2");
        NewItem("R3", vscode.CompletionItemKind.Value, "16 bit register", "R3");
        NewItem("R4", vscode.CompletionItemKind.Value, "16 bit register", "R4");
    }
    if (line.toUpperCase().includes(' P')) {
        NewItem("PC", vscode.CompletionItemKind.Value, "program Counter", "PC");
    }
    if (line.toUpperCase().includes(' S')) {
        NewItem("SP", vscode.CompletionItemKind.Value, "stack pointer", "SP");
    }
    if (line.toUpperCase().includes(' M')) {
        NewItem("MB", vscode.CompletionItemKind.Value, "memory bank", "MB");
    }
    if (line.toUpperCase().includes(' F')) {
        NewItem("F", vscode.CompletionItemKind.Value, "flags", "F");
    }
}

function Instrs(line) {
    let FirstChar = line[0];
    switch (FirstChar.toUpperCase()) {
        case 'A':
            NewItem("ADD", vscode.CompletionItemKind.Module, "ADD destination, source.<br>Adds the source and destination and puts it in the destination", "add");
            NewItem("AND", vscode.CompletionItemKind.Module, "AND destination, source.<br>Bitwise AND on the source and destination and puts it in the destination", "and");
            break;
        case 'C':
            NewItem("CMP", vscode.CompletionItemKind.Keyword, "CMP operand1, operand2<br>Compares the source and destination and updates the flags", "cmp");
            NewItem("CALL", vscode.CompletionItemKind.Keyword, "CALL address<br>Call a function at the specified address", "call");
            NewItem("CLF", vscode.CompletionItemKind.Keyword, "CLF flag<br>Clear the specified flag", "clf");
            break;
        case 'D':
            NewItem("DIV", vscode.CompletionItemKind.Keyword, "DIV destination, source.<br>Divides the source and destination and puts it in the destination", "div");
            NewItem("DEC", vscode.CompletionItemKind.Keyword, "DEC destination.<br>Decrement the value in destination", "dec");
            break;
        case 'E':
            NewItem("EOR", vscode.CompletionItemKind.Keyword, "XOR destination, source.<br>Bitwise XOR on the source and destination and puts it in the destination", "xor");
            break;
        case 'H':
            NewItem("HALT", vscode.CompletionItemKind.Keyword, "HALT<br>Sets the HALT flag", "halt");
            break;
        case 'I':
            NewItem("INT", vscode.CompletionItemKind.Keyword, "INT interrupt_number<br>Generate a software interrupt with the specified interrupt number", "int");
            NewItem("INC", vscode.CompletionItemKind.Keyword, "INC destination.<br>Increment the value in destination", "inc");
            NewItem("IN", vscode.CompletionItemKind.Keyword, "IN port, destination<br>Input the value from the specified port into the destination", "in");
            break;
        case 'J':
            NewItem("JMP", vscode.CompletionItemKind.Keyword, "JMP address<br>Jumps to the specified Addess", "jmp");
            NewItem("JLE", vscode.CompletionItemKind.Keyword, "JLE address<br>Jumps to the specified Addess if less or equal to is set", "jle");
            NewItem("JE", vscode.CompletionItemKind.Keyword, "JE address<br>Jumps to the specified Addess if equal to is set", "je");
            NewItem("JGE", vscode.CompletionItemKind.Keyword, "JGE address<br>Jumps to the specified Addess if less is not set or equal to is set", "jge");
            NewItem("JG", vscode.CompletionItemKind.Keyword, "JG address<br>Jumps to the specified Addess if less is not set", "jg");
            NewItem("JNE", vscode.CompletionItemKind.Keyword, "JNE address<br>Jumps to the specified Addess if equal to is not set", "jne");
            NewItem("JL", vscode.CompletionItemKind.Keyword, "JL address<br>Jumps to the specified Addess if less is set", "jl");
            NewItem("JER", vscode.CompletionItemKind.Keyword, "JER address<br>Jumps to the specified Addess if error is set", "jer");
            NewItem("JMC", vscode.CompletionItemKind.Keyword, "JMC address<br>Jumps to the specified Addess if carray is set", "jmc");
            NewItem("JMZ", vscode.CompletionItemKind.Keyword, "JMZ address<br>Jumps to the specified Addess if zero is set", "jmz");
            NewItem("JNC", vscode.CompletionItemKind.Keyword, "JNC address<br>Jumps to the specified Addess if carry is not set", "jnc");
            NewItem("JMS", vscode.CompletionItemKind.Keyword, "JMS address<br>Jumps to the specified Addess if sign is set", "jms");
            NewItem("JNS", vscode.CompletionItemKind.Keyword, "JNS address<br>Jumps to the specified Addess if sign is not set", "jns");
            break;
        case 'M':
            NewItem("MOV", vscode.CompletionItemKind.Keyword, "MOV destination, source.<br>Moves the source into the destination", "mov");
            NewItem("MUL", vscode.CompletionItemKind.Keyword, "MUL destination, source.<br>Multiplys the source and destination and puts it in the destination", "mul");
            break;
        case 'N':
            NewItem("NOP", vscode.CompletionItemKind.Keyword, "NOP<br>dose not thing", "nop");
            NewItem("NOT", vscode.CompletionItemKind.Keyword, "NOT destination.<br>Bitwise NOT on the destination and puts it in the destination", "not");
            NewItem("NOR", vscode.CompletionItemKind.Keyword, "NOR destination, source.<br>Bitwise XOR the the source and destination and puts it in the destination", "nor");
            break;
        case 'O':
            NewItem("OR", vscode.CompletionItemKind.Keyword, "OR destination, source.<br>Bitwise OR on the source and destination and puts it in the destination", "or");
            NewItem("OUT", vscode.CompletionItemKind.Keyword, "OUT port, operand<br>Output the value in the source to the specified port", "out");
            break;
        case 'P':
            NewItem("PUSH", vscode.CompletionItemKind.Keyword, "PUSH source<br>pushs the source on the stack and decrements the SP register", "push");
            NewItem("POP", vscode.CompletionItemKind.Keyword, "POP destination<br>Increments the SP register and pops the a value off the stack", "pop");
            NewItem("PUSHR", vscode.CompletionItemKind.Keyword, "PUSHR<br>Push all registers (AX, BX, CX, DX, ZX, X, Y) onto the stack", "pushr");
            NewItem("POPR", vscode.CompletionItemKind.Keyword, "POPR<br>Pop all registers (Y, X, DX, CX, BX, AX) from the stack", "popr");
            break;
        case 'R':
            NewItem("ROL", vscode.CompletionItemKind.Keyword, "ROL destination, operand1<br>Rotates the destination left by the number of positions specified by operand1", "rol");
            NewItem("ROR", vscode.CompletionItemKind.Keyword, "ROR destination, operand1<br>Rotates the destination right by the number of positions specified by operand1", "ror");
            NewItem("RTS", vscode.CompletionItemKind.Keyword, "RTS<br>Return from a function without an offset", "rts");
            NewItem("RET", vscode.CompletionItemKind.Keyword, "RET operand1<br>Return from a function with the specified operand1", "ret");
            break;
        case 'S':
            NewItem("SUB", vscode.CompletionItemKind.Keyword, "SUB destination, source.<br>Subtracts the source and destination and puts it in the destination", "sub");
            NewItem("SEF", vscode.CompletionItemKind.Keyword, "SEF flag<br>Sets the specified flag", "sef");
            NewItem("SHL", vscode.CompletionItemKind.Keyword, "SHL destination, operand1<br>Shifts the destination left by the number of positions specified by operand1", "shl");
            NewItem("SHR", vscode.CompletionItemKind.Keyword, "SHR destination, operand1<br>Shifts the destination right by the number of positions specified by operand1", "shr");
            break;
        case 'X':
            NewItem("XOR", vscode.CompletionItemKind.Keyword, "XOR destination, source.<br>Bitwise XOR on the source and destination and puts it in the destination", "xor");
            break;

        default:
            break;
    }
}

function Snippet() {
    createSnippet("Address", "Makes an address", "[${1:Number}],{2:}", "Makes an address");
    createSnippet("Switch_Bank", "Bank",
        "push MB\r\n" +
        "mov MB #${1:New_Bank}\r\n" +
        "; Code\r\n" +
        "pop MB\r\n",
        "Makes a way to move something else in MB");

    createSnippet("Stack_fream", "Start Stack fream",
        "pushr\r\n" +
        "mov ZX, SP\r\n" +
        "add SP, #8\r\n" +
        "${1:; pop values out of the stack}\r\n" +
        "mov SP, ZX\r\n",
        "Sets up a stack fream"
    )

    createSnippet("function", "make a new function",
        "; Register Type\r\n" +
        "; Register Type\r\n" +
        "; return in register \r\n" +
        "; the number of arguments is ${1:Number of arguments}\r\n" +
        ".global ${2:Func_Name}:\r\n" +
        "pushr\r\n" +
        "mov ZX, SP\r\n" +
        "add SP, #8\r\n" +
        "\r\n" +
        "; here we will pop all the values of the stack\r\n" +
        "; remeber the WIP you push the values in is not the\r\n" +
        "; when you need to pop them out like this\r\n" +
        "; pop value2r\n" +
        "; pop value1r\n" +
        "\r\n" +
        "mov SP, ZX\r\n" +
        "${3:; Code}\r\n" +
        "popr\r\n" +
        "ret #${1:return}\r\n" +
        "\r\n" +
        "; the way to call this function is to push the ${1:Number of arguments}\r\n" +
        "; variables that is\r\n" +
        "; push value1\r\n" +
        "; push value2\r\n" +
        "\r\n" +
        "; after that you can now call the func from any file \r\n" +
        "; that has included this file or is in the same project \r\n" +
        "; call [${2:Func_Name}]\r\n" +
        "",
        "make a function with a stack fream"
    )
}

function createSnippet(name, label, body, description) {
    const snippet = new vscode.CompletionItem(name, vscode.CompletionItemKind.Snippet);
    snippet.insertText = new vscode.SnippetString(body);
    snippet.documentation = new vscode.MarkdownString(description);
    snippet.detail = label;
    Result.push(snippet);
}
function NewItem(triggerCharacter, triggerKind, detail = "", insertText = "") {
    let BufferItem = new vscode.CompletionItem(triggerCharacter, triggerKind);
    let MarkDownText = new vscode.MarkdownString(detail);
    MarkDownText.supportHtml = true;
    BufferItem.documentation = MarkDownText;
    BufferItem.insertText = insertText;
    Result.push(BufferItem);
}

function deactivate() {
    // Cleanup logic when the extension is deactivated
}

module.exports = {
    activate,
    deactivate
}