AMLEditorPluginContract
=======================

Contract DLL for Plugin Development for the AutomationML Editor with example Implementations and some already published PlugIns.

The included contract DLL defines interface classes for the implementation of AutomationML Editor - PlugIns. 
The implementation is based on the Microsoft Extencibility Framework (MEF). A new PlugIn class must implement one 
of the interface classes defined in the contract and export it for use in the AutomationML Editor. The PlugIn Manager
of the AutomationML Editor allows the installation of the PlugIns that implement the contract correctly. 

The sample implementations can be used as templates for plugin development. Four templates are currently available.
The template named 'SimpleWPFUserControl' implements a WPF Control Plugin which is inserted directly into the user 
interface of the AutomationML Editor. The template named 'EditingCAEXApplication' is an example of a plugin that creates
its own UI thread. The implementation shows how thread synchronization between the AutomationML editor and a plug-in works. 
The 'PluginWithToolBar' template implements a plug-in that adds its own toolbar to the editor toolbar.

PlugIn developers can make a new PlugIn available for the Editor with the help of the PlugIn Manager of the
AutomationML Editor (menu item PlugIn). To do this, the directory in which the PlugIn DLL is contained must
be specified as the PlugIn source. If the PlugIn has implemented the PlugIn Contract correctly, the PlugIn 
Manager lists the new PlugIn which can then be installed.

An AutomationML Editor PlugIn can be published to make it available to the AutomationML community. 
The platform for publishing is NuGet. Prerequisite for the PlugIn to be listed is that the NuGet name 
starts with the namespace prefix 'Aml.Editor.Plugin'. For example, a valid name is 'Aml.Editor.Plugin.MyPlugin', 
where you should replace MyPlugin with a meaningful name.
