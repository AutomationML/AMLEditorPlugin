// Copyright (c) 2017 AutomationML e.V.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Aml.Editor.PlugInManager.AssemblyLoader;

internal class PluginLoadContext: AssemblyLoadContext
{
    public PluginLoadContext(string pluginPath): base(pluginPath)
    {
        _resolver = new(pluginPath);
        AssemblyDirectory = Path.GetDirectoryName(pluginPath);
    }

    string AssemblyDirectory { get; set; }


    internal AssemblyDependencyResolver _resolver;

    public bool CanResolve (AssemblyName assemblyName)
    {
        return !string.IsNullOrEmpty (_resolver.ResolveAssemblyToPath(assemblyName));
    }

    

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return nint.Zero;
    }

    internal Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            //Make an array for the list of assemblies.
            Assembly[] assems = currentDomain.GetAssemblies();

            var ass = assems.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
            if (ass != null)
            {
                return ass;
            }

            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
        //return Assembly.LoadFrom (Path.Combine (AssemblyDirectory, assemblyName.Name));
    }
}