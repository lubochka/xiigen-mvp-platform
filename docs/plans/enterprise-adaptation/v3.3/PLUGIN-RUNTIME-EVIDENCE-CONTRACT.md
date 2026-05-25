# Plugin Runtime Evidence Contract — Enterprise v3.2

Two evidence levels exist:

```text
contract_harness
interactive_ide_plugin_scenario
```

`contract_harness` can prove only source wiring and shaped adapter output. It cannot close PLUGIN-VS or PLUGIN-VSCODE interactive rows.

Visual Studio evidence must show:

- VSIX or Visual Studio extension path;
- command or tool-window/view-model entrypoint;
- Visual Studio host or Experimental Instance invocation;
- real project context;
- prompt submitted through host UI/command;
- result returned through host path;
- screenshot/log/trace evidence;
- unit locator/import proof.

VS Code evidence must show:

- extension path;
- Extension Host invocation;
- command/panel/chat entrypoint;
- real workspace context;
- prompt submitted through host path;
- result returned through host path;
- screenshot/log/trace evidence;
- unit locator/import proof.
