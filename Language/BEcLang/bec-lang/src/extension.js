const vscode = require('vscode');

/**
 * @param {vscode.ExtensionContext} context
 */
function activate(context) {
    const disposables = [];

    let disposableHelloWorld = vscode.commands.registerCommand('BEC.helloWorld', function () {
        vscode.window.showInformationMessage('Hello World from Basm!');
    });

    disposables.push(disposableHelloWorld);
}

function deactivate() {

}

module.exports = {
    activate,
    deactivate
}