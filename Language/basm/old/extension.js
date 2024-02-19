// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require('vscode');

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed

/**
 * @param {vscode.ExtensionContext} context
 */
function activate(context) {

	// Use the console to output diagnostic information (console.log) and errors (console.error)
	// This line of code will only be executed once when your extension is activated
	console.log('Congratulations, your extension "basm" is now active!');

	// The command has been defined in the package.json file
	// Now provide the implementation of the command with  registerCommand
	// The commandId parameter must match the command field in package.json
	let disposable = vscode.commands.registerCommand('basm.helloWorld', function () {
		// The code you place here will be executed every time your command is executed

		// Display a message box to the user
		vscode.window.showInformationMessage('Hello World from Basm!');
	});

	disposable = vscode.commands.registerCommand('basm.testTokens', function () {
		const document = vscode.window.activeTextEditor.document;
		const languageId = document.languageId;
	
		// Tokenize the document
		const tokens = vscode.languages.tokenize(document.uri, document.getText(), languageId);
	
		// Convert token information to a readable format
		const tokenInfo = tokens.map(token => ({
		  startIndex: token.startIndex,
		  endIndex: token.endIndex,
		  scopes: token.scopes.join(', ')
		}));
	
		// Log the tokens to the Output panel
		console.log('Tokens:', tokenInfo);
	});

	context.subscriptions.push(disposable);
}

// This method is called when your extension is deactivated
function deactivate() {}

module.exports = {
	activate,
	deactivate
}
