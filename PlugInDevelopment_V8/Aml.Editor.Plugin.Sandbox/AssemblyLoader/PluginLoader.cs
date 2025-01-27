using Aml.Editor.Plugin.Contracts;
using Aml.Editor.Plugin.Sandbox.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;

namespace Aml.Editor.PlugInManager.AssemblyLoader;

public class PluginLoader
{
    private static MainViewModel _mainModel;

    internal static void LoadPlugin (PluginViewModel plugin, MainViewModel mainModel)
    {
        _mainModel = mainModel;
        var pluginLocation = plugin.FilePath;
        Console.WriteLine($"Loading commands from: {pluginLocation}");
        var loadContext = new PluginLoadContext(pluginLocation);
        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));

        CreatePlugin(plugin, assembly, loadContext);
    }



    private static bool _tryResolve = false;


    // needed to load assemblies requested by Xaml parser which are not found in the default load context but can be provided by plugin load context
    internal static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {        
        if (_tryResolve)
        {
            return null;
        }
        foreach (var plugin in _mainModel.Plugins)
        {
            if (AssemblyLoadContext.GetLoadContext(plugin.Plugin?.GetType().Assembly) is PluginLoadContext loadContext)
            {
                var assemblyName = new AssemblyName(args.Name);
                if (loadContext.CanResolve(assemblyName))
                {
                    _tryResolve = true;
                    var ass = loadContext.LoadFromAssemblyName(assemblyName);
                    _tryResolve = false;
                    return ass;
                }
            }
        }
        return null;
    }

    private static void ActivatePlugin (PluginViewModel plugin, Type pluginType)
    {
        if (Activator.CreateInstance(pluginType) is IAMLEditorPlugin result)
        {
            plugin.DisplayName = result.DisplayName;
            plugin.Plugin = result;            
        }
    }

    private static void CreatePlugin(PluginViewModel plugin, Assembly assembly, PluginLoadContext loadContext)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(IAMLEditorPlugin).IsAssignableFrom(type))
            {
                if (typeof(IAMLEditorSubView).IsAssignableFrom(type))
                {
                    continue;
                }
                
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        ActivatePlugin(plugin, type);
                    }
                    catch 
                    {
                        AppDomain.CurrentDomain.AssemblyResolve += loadContext.ResolveAssembly;
                        try
                        {
                            ActivatePlugin(plugin, type);
                        }
                        catch (Exception ex2)
                        {
                            MessageBox.Show($"Error loading plugin {plugin.Name}: {ex2.Message}");
                        }
                        finally
                        {
                            AppDomain.CurrentDomain.AssemblyResolve -= loadContext.ResolveAssembly;
                        }
                    }
                }));
            }
        }
    }
}
