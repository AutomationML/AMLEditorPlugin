## Development and deployment of plug-ins for the AutomationML Editor as of version 6.0

------

### Prerequisites

- You need AutomationML Editor version 6.0 or higher to use the plug-ins with the editor. For development and testing the sandbox project is sufficient.
- You need at least Visual Studio 2019 as your development platform. The projects in this repository were created using Visual Studio 2022. 
- Install the .NET 5 SDK

### What's new for Version 6.0

- The example projects are switched to the SDK based **.csproj* format for Visual Studio projects.  
- The Target Framework has changed to .NET 5. 
- New Resources, provided for AutomationML Editor Plug-In Development are used. This includes, for example, the possibility of selecting the light or dark display mode.

### Examples

The examples show different aspects of plug-in development. The plug-in contract provides specific interface classes for individual aspects. These can be implemented in a plug-in individually or in combination. The examples use different docking modes for the embedding of the views in the editor's UI layout.

- `Aml.Editor.Plugin.Theming` 
  This example shows how a plugin can use the light or dark display mode currently selected in the editor. The Theme selection aspect is included in all other plugin examples, too.  The implemented contract is:

  - `ISupportsThemes`

    

- `Aml.Editor.Plugin.WithToolbar`
  This example shows how a plugin can add a toolbar defining Plugin commands to the Editor's main toolbar. The implemented contracts are:

  - `ISupportsThemes`

  - `IToolBarIntegration`

    

- `Aml.Editor.Plugin.MultiView`
  In this example, a plugin is developed that defines multiple views that are all embedded in the current layout by the editor's docking system. The implemented contracts are:

  - `ISupportsThemes`

  - `IAMLEditorViewCollection`,

  - `INotifyViewActivation`

    

- `Aml.Editor.Plugin.CallCommand`
  This example shows how a plugin can activate user methods of the editor like opening an AutomationML file. The implemented contracts are:

  - `ISupportsThemes`

  - `IEditorCommanding`

    

- `Aml.Editor.Plugin.Window`
  This plugin creates a separate main window that is not integrated into the UI layout of the editor. This window reacts to the UI scaling of the editor. The implemented contracts are:

  - `ISupportsThemes`

  - `ISupportsUIZoom`

    

- `Aml.Editor.Plugin.Collada`
  This example shows how a plugin processes the selection of an object that references an external COLLADA file. This plugin das dependencies to other packages which are effected by the `<EnableDynamicLoading>true</EnableDynamicLoading>` element. The implemented contracts are:

  - `ISupportsThemes`

  - `ISupportsUIZoom`

  - `IAMLEditorExternalsPlugin`

  - [Aml.Editor.API](https://www.nuget.org/packages/Aml.Editor.API/) is used

    <img src="img\Collada.png" alt="Collada" style="zoom:80%;" />
    

The 3D Viewer is created using the [HelixToolkit.SharpDX](https://github.com/helix-toolkit/helix-toolkit). The used 3D COLLADA Robot models are provided by [COLLADA 1.5 robot repository](https://github.com/rdiankov/collada_robots).

### Publishing

If you want to share the created plug-in for third party use you need to create a package. In between the `<PropertyGroup>` tags of the project file, the `<EnableDynamicLoading>true</EnableDynamicLoading>` element has to be added. It prepares the project so that it can be used as a plugin. Among other  things, this will copy all of its dependencies to the output of the  project.

Packages referenced by the AutomationML Editor should have the  `<ExcludeAssets>runtime</ExcludeAssets>` element added to the Package reference tag. This will not include the shared packages into the package. 

The Tag **AMLEditorPlugin.Core** has to be added to the package tag names. This ensures, that the Package Manager can find the plugin in a provided package repository and can offer it to other users.

The *PackageName* of the Plugin shall be unique.








