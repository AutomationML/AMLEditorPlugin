AMLEditorPluginContract
=======================

Contract DLL for Plugin Development for the AutomationML Editor with example Implementations.

This Contract DLL defines Interface Classes which have to be implemented by AutomationML Editor - Plugin Developers. 
When implemented, the Plugin Classes should EXPORT its Interface with Microsoft Extencibility Framework (MEF).
The Plugin DLL has to be located under the Plugins Directory in the Installation Folder of the AutomationML Editor.

The Example Implementations can be used as Templates for Plugin Development. Currently four Templates are available.
The Template named 'SimpleWPFUserControl' implements a WPF Control Plugin, which is directly integrated 
in the UI of the AutomationML Editor. The Template, named 'EditingCAEXApplication' is an example for a Plugin, 
which has its own UI-Thread. The Implementation shows, how the Thread Synchronisation between the AMLEditor 
and the plugin can be implemented. The Template 'PluginWithToolBar' shows, how a PlugIn can add its own Toolbar to the Editors Toolbar.

PlugIn developers can make a new PlugIn available for the Editor with the help of the PlugIn Manager of the
AutomationML Editor (menu item PlugIn). To do this, the directory in which the PlugIn DLL is contained must
be specified as the PlugIn source. If the PlugIn has implemented the PlugIn Contract correctly, the PlugIn 
Manager lists the new PlugIn which can then be installed.

An AutomationML Editor PlugIn can be published to make it available to the AutomationML community. 
The platform for publishing is NuGet. Prerequisite for the PlugIn to be listed is that the NuGet name 
starts with the namespace prefix 'Aml.Editor.Plugin'. For example, a valid name is 'Aml.Editor.Plugin.MyPlugin', 
where you should replace MyPlugin with a meaningful name.
