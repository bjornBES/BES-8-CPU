const vscode = require('vscode');

/**
 * This method is called when your extension is activated.
 * Your extension is activated the very first time the command is executed.
 */
function activate(context) {
  console.log('Your extension is now active!');

  // Register a command to get token information
  let disposable = vscode.commands.registerCommand('extension.getTokenInfo', getTokens);

  context.subscriptions.push(disposable);
}

/**
 * This method is called when your extension is deactivated.
 */
function deactivate() {
  console.log('Your extension is now deactivated!');
}

/**
 * Get information about the tokens at the current cursor position.
 */
function getTokens() {
  const editor = vscode.window.activeTextEditor;

  if (editor) {
    const position = editor.selection.active;
    const tokens = vscode.languages.getTokenInformationAtPosition(editor.document, position);
    
    console.log(tokens);
    // Handle the tokens as needed
  }
}

module.exports = {
  activate,
  deactivate
};
