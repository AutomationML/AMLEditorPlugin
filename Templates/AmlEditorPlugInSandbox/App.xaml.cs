using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace AmEditorPlugInSandbox
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            try
            {
                string startUpPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(startUpPath, "Plugins");

                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);
            }
            catch
            {

            }
        }
    }
}
