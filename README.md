Aml.Editor.Plugin
=================

---

![][1]

---

#### Overview
This repository provides an implementation guide and sources which explain, how to develop PlugIns for the [AutomationML Editor](https://github.com/AutomationML/AutomationMLEditor/blob/main/README.md).

The AutomationML Editor provides a contract DLL for PlugIn Development, published as a [NUGET package](https://www.nuget.org/packages/Aml.Editor.Plugin.Contract).  The contract DLL contains interface classes, defining the API to access data, edited by AutomationML Editor and to call commands to execute editor actions. 

The API is based on the [Microsoft Extensibility Framework (MEF)](https://docs.microsoft.com/en-us/dotnet/framework/mef/). A new PlugIn class must implement one of the interface classes defined in the contract and export it for use in the AutomationML Editor. The PlugIn Manager of the AutomationML Editor allows the installation of the PlugIns that implement the contract correctly. 

#### Example PlugIns
The sample implementations can be used as templates for PlugIn development. Five templates are currently available. The template named **SimpleWPFUserControl** implements a WPF Control PlugIn which is inserted directly into the user interface of the AutomationML Editor. The template named **EditingCAEXApplication** is an example of a PlugIn that creates its own UI thread. The implementation shows how thread synchronization between the AutomationML editor and a plug-in works. The **PluginWithToolBar** template implements a PlugIn that adds its own toolbar to the editors toolbar. The PlugIn **PlugInCallingCommands** explains, how a PlugIn can invoke editor actions. The AutomationML editor's UI is based on the [Windows Presentation Foundation (WPF)](https://visualstudio.microsoft.com/de/vs/features/wpf/). Most example PlugIns are also based on WPF, but the use of WPF is not mandatory as shown with the **WindowsFormsPlugin**.

To help to develop and test a new PlugIn before deployment, the **AmlEditorPlugInSandbox** sandbox project is provided. Here the interaction between the PlugIn and the editor can be tested using a small editor dummy. 

#### Note
The AutomationML editor is currently based on .NET Framework 4.7. PlugIns, based on a higher version, cannot be included. Check your installed editor version for the supported Framework version.

#### PlugIn Deployment
Since version 5.1.3. of the AutomationML editor, PlugIn developers can make a new PlugIn available for the Editor with the help of the editors PlugIn Manager (menu item PlugIn). To do this, the directory in which the PlugIn DLL is contained must be specified as the PlugIn source. If the PlugIn has implemented the PlugIn Contract correctly, the PlugIn Manager lists the new PlugIn which can then be installed.  The PlugIn Manager can install PlugIns, deployed as a ZIP package, containing all DLLs and additional files, needed by the PlugIn or alternatively also PlugIns, deployed as a NUGET package. The ZIP package needs an additional Metadata.xml file, containing information about the PlugIn and the PlugIn provider. The Deployment of NUGET packages is explained in the provided sources (see the _NugetDeployment_ build configuration).

An AutomationML Editor PlugIn can be published to make it available to the AutomationML community. The platform for publishing is NuGet. Prerequisite for the PlugIn to be listed is that the NuGet name starts with the namespace prefix **Aml.Editor.Plugin**. For example, a valid name is _Aml.Editor.Plugin.MyPlugin_, where you should replace _MyPlugin_ with a meaningful name, not already used.

To create a NUGET package using the _NugetDeployment_ build configuration, a NUGET CLI has to be installed. You can download the CLI from [here](https://www.nuget.org/downloads). Ensure that the command is found by the build system.

#### PlugIn Installation

The example plugins can be installed to your installed AutomationML editor. Deploy the PlugIns as NUGET packages as explained. Run the AutomationML editor and go to the PlugIn Manager. From the PlugIn Manager open the Settings dialog and add the PlugIn folder of the Sandbox project as an additional PlugIn source as shown in the image below.

![](img/PMan.png)

Click the _Update_ Button and the PlugIns will be displayed as new available PlugIns and can be selected for installation.

![](img/PInstall.png)

#### Troubleshooting

- If you encounter problems integrating the plugin into the AutomationML editor, please make sure that you are not using a newer version of the **Aml.Engine** than the AutomationML editor itself. 
- Ensure, that all referenced DLLs, needed by your PlugIn, are contained in the deployed package, but don't include the contract DLL,  the Aml.Engine DLLs or System DLLs, which are part of the windows OS in the package. 
- For support you can also contact the editor support. 



[1]: https://raw.githubusercontent.com/AutomationML/AMLEngine2.1/master/img/AutomationML-Logo.png
