AMLEditorPluginContract
=======================

Contract DLL for Plugin Development for the AutomationML Editor with example Implementations.

This Contract DLL defines Interface Classes which have to be implemented by AutomationML Editor - Plugin Developers. 
When implemented, the Plugin Classes should EXPORT its Interface with Microsoft Extencibility Framework (MEF).
The Plugin DLL has to be located under the Plugins Directory in the Installation Folder of the AutomationML Editor.

The Example Implementations can be used as Templates for Plugin Development. Currently two Templates are available. The Template named 'SimpleWPFUserControl' implements a WPF Control Plugin, which is directly integrated in the UI of the AutomationML Editor. The Template, named 'EditingCAEXApplication' is an example for a Plugin, which has its own UI-Thread. The Implementation shows, how the Thread Synchronisation between the AMLEditor and the plugin can be implemented.
