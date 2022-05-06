// Copyright (c) 2022 AutomationML and Contributors. All rights reserved.
// Licensed to the AutomationML association under one or more agreements.
// The AutomationML association licenses this file to you under the MIT license.

using Aml.Editor.Plugin.Contracts;
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Aml.Editor.PlugInManager.Loader
{
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        #region Public Constructors

        public SafeDirectoryCatalog(string directory)
        {
            _catalog = new AggregateCatalog();

            foreach (string file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    AssemblyCatalog asmCat = new(file);

                    // Force MEF to load the plugIn and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                    {
                        _catalog.Catalogs.Add(asmCat);
                    }
                }

                catch (BadImageFormatException)
                {
                    continue;
                }
                catch (Exception exc)
                {
                    _ = MessageBox.Show(
                       "Could not load " + Path.GetFileNameWithoutExtension(file) + exc.Message, "PlugIn Loader", MessageBoxButtons.OK,
                       MessageBoxIcon.Information);
                }
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public override IQueryable<ComposablePartDefinition> Parts => _catalog.Parts;

        #endregion Public Properties

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveCustomFolder;
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private Assembly AssemblyResolveCustomFolder(object sender, ResolveEventArgs args)
        {
            return AssemblyResolve(sender, args, _pluginPath);
        }

        #endregion Private Methods

        #region Private Fields

        private static readonly Assembly[] AMLAssemblies =
        {
            typeof(Toolkit.ViewModel.AMLLayout).Assembly,
            typeof(Engine.CAEX.CAEXDocument).Assembly,
            typeof(Engine.Services.LookupService).Assembly,
            typeof(Skins.AMLApp).Assembly,
            typeof(IPC.CommunicationChannel).Assembly,
            typeof(API.AMLEditor).Assembly,
            typeof(Engine.Resources.AMLLibraries).Assembly,
            typeof(PluginCommand).Assembly
        };

        private readonly AggregateCatalog _catalog;
        private readonly string _pluginPath;

        #endregion Private Fields

        #region Public Methods

        public static Assembly AssemblyResolve(object sender, ResolveEventArgs args, string probingPath)
        {
            AssemblyName assyName = new(args.Name);

            string assemblyPath = null;

            string[] fields = args.Name.Split(',');
            string assemblyName = fields[0];
            string assemblyCulture = fields.Length < 2 ? null : fields[2].Substring(fields[2].IndexOf('=') + 1);

            Assembly amlAssembly = AMLAssemblies.FirstOrDefault(ass => ass.GetName().Name == assyName.Name);
            if (amlAssembly != null)
            {
                Version targetVersion = amlAssembly.GetName().Version;
                byte[] publicKeyToken = amlAssembly.GetName().GetPublicKeyToken();
                AssemblyName requestedAssembly = new(args.Name)
                {
                    CultureInfo = CultureInfo.InvariantCulture,
                    Version = targetVersion
                };
                requestedAssembly.SetPublicKeyToken(publicKeyToken);

                return Assembly.Load(requestedAssembly);
            }

            DirectoryInfo di = new(probingPath);
            FileInfo[] assemblyFiles = di.GetFiles(assyName.Name + "*.dll", SearchOption.AllDirectories);
            if (!assemblyFiles.Any())
            {
                return string.IsNullOrEmpty(assemblyPath) ? null : Assembly.LoadFrom(assemblyPath);
            }

            if (assemblyName.EndsWith(".resources"))
            {
                FileInfo languageDll = assemblyFiles.FirstOrDefault
                    (f => f.FullName.Split('\\').Contains(assemblyCulture));

                assemblyPath = languageDll?.FullName;
            }
            else
            {
                assemblyPath = assemblyFiles.FirstOrDefault()?.FullName;
            }

            return string.IsNullOrEmpty(assemblyPath) ? null : Assembly.LoadFrom(assemblyPath);
        }

        #endregion Public Methods
    }
}