using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aml.Editor.Plugin.Sandbox
{
  
    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
        #region Private Fields

        private readonly AggregateCatalog _catalog;

        #endregion Private Fields

        #region Public Constructors

        public SafeDirectoryCatalog(string directory)
        {
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

            _catalog = new AggregateCatalog();

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);

                    // Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(asmCat);
                }
                catch (ReflectionTypeLoadException loaderException)
                {
                    StringBuilder sb = new StringBuilder();

                        if (loaderException.LoaderExceptions != null && loaderException.LoaderExceptions.Length > 0)
                            sb.AppendLine(loaderException.LoaderExceptions[0].ToString());
                        else
                            sb.AppendLine(loaderException.ToString());

                    MessageBox.Show("Could not load " + Path.GetFileNameWithoutExtension(file) + sb.ToString(), 
                        "PlugIn Loader", MessageBoxButton.OK, MessageBoxImage.Information);


                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return _catalog.Parts; }
        }

        #endregion Public Properties
    }
}
