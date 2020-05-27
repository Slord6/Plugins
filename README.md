# Plugins
 C# Plugin System

## Write a plugin

- For a new project, add a reference to the `PluginBase` project
- Ensure project XML includes
```XML
<ItemGroup>
      <ProjectReference Include="path\to\PluginBase.csproj">
          <Private>false</Private>
          <ExcludeAssets>runtime</ExcludeAssets>
      </ProjectReference>
  </ItemGroup>
```
- Create a new class (or classes) which implements `IPlugin`

### Order of execution

- `OnLoad` is called as soon as the plugin is instantiated.
- `CreateTask` may be called at any time after `OnLoad` and before `OnUnload`. The returned task should implement the desired behaviour of the plugin. `CreateTask` may return a `PluginResultData` object or `null`.
- `OnUnload` may be called at any time after `OnLoad`. Note that it may be called before `CreateTask`, whilst a task is still running or after it has completed.

### PluginApp Arguments
`--help` for in-tool help

`--pluginLocations` - List of locations to load plugins from, can be a filepath or URI - see "Loading remote plugins" below

`--targetPlugins` - List of plugin names to run. If not provided all plugins are run.

`--pluginLocationsFile` - List of files from which to load plugin paths. May be local or remote files. Each path should be on a new line.

## Loading remote plugins

The PluginHandler will accept remote URIs for plugins. The URI must contain the name of the assembly as the last part of the path. eg. `https://example.com/PluginName.dll` or `https://example.com/SearchPlugin.dll?token=abc123`or `https://example.com/QuestionPlugin?token=abc123`.

## Sources
Loosely based on the .NET Core plugin system described [here](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)