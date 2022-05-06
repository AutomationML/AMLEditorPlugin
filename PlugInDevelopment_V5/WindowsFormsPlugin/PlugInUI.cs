using Aml.Engine.CAEX;
using System.Windows.Forms;

namespace Aml.Editor.Plugin
{
    /// <summary>
    /// This is a window forms UI control, containing a tree view. The Tree view is updated, ever when an InternalElement is selected
    /// in the editor which has an Instance Class relation to a SystemUnitClass. The Tree view is populated with the ExternalInterface
    /// objects and InternalElement objects of the referenced SystemUnitClass.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    public partial class PlugInUI : UserControl
    {
        #region Public Constructors

        public PlugInUI()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Internal Methods

        /// <summary>
        /// Shows the SystemUnitClass in the tree view.
        /// </summary>
        /// <param name="systemUnitClass">The system unit class.</param>
        internal void ShowClass(SystemUnitFamilyType systemUnitClass)
        {
            treeView1.Nodes.Clear();
            var tn = treeView1.Nodes.Add(systemUnitClass.Node.Name.LocalName + ": " + systemUnitClass.Name);
            AddEIChilds(systemUnitClass, tn);
            AddIEChilds(systemUnitClass, tn);

            treeView1.ExpandAll();
        }

        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Recursively adds The ExternalInterface  children.
        /// </summary>
        /// <param name="caexObject">The CAEX object.</param>
        /// <param name="treeNode">The tree node.</param>
        private void AddEIChilds(IObjectWithExternalInterface caexObject, TreeNode treeNode)
        {
            foreach (var element in caexObject.ExternalInterface)
            {
                var childNode = treeNode.Nodes.Add(element.Node.Name.LocalName + ": " + element.Name);
                AddEIChilds(element, childNode);
            }
        }

        /// <summary>
        /// Recursively adds The InternalElement children.
        /// </summary>
        /// <param name="caexObject">The CAEX object.</param>
        /// <param name="treeNode">The tree node.</param>
        private void AddIEChilds(IInternalElementContainer caexObject, TreeNode treeNode)
        {
            foreach (var element in caexObject)
            {
                var childNode = treeNode.Nodes.Add(element.Node.Name.LocalName + ": " + element.Name);
                AddIEChilds(element, childNode);
            }
        }

        #endregion Private Methods
    }
}