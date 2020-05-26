# Plugins
 C# Plugin System

## Write a plugin

- For a new project, add a reference to the `PluginBase` project
- Create a new class (or classes) which implements `IPlugin`

### Order of execution

- `OnLoad` is called as soon as the plugin is instantiated.
- `CreateTask` may be called at any time after `OnLoad` and before `OnUnload`. The returned task should implement the desired behaviour of the plugin. `CreateTask` may return a `PluginResultData` object or `null`.
- `OnUnload` may be called at any time after `OnLoad`. Note that it may be called before `CreateTask`, whilst a task is still running or after it has completed.

## Loading remote plugins

The PluginHandler will accept remote URIs for plugins. The URI must contain the name of the assembly as the last part of the path. eg. `https://example.com/PluginName.dll` or `https://example.com/SearchPlugin.dll?token=abc123`or `https://example.com/QuestionPlugin?token=abc123`.

Loosely based on the .NET Core plugin system described [here](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)