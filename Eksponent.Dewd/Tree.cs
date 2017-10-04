using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using umbraco.interfaces;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using System.Text;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using System.Reflection;

namespace Eksponent.Dewd
{
    /// <summary>
    /// Summary description for Tree
    /// </summary>
    public class Tree : BaseTree, ITree
    {
        private MethodInfo treeServiceUrlMethod;

        public Tree(string application) : base(application) {
            treeServiceUrlMethod = this.GetType().GetMethods()
                .First(mi => mi.Name == "GetTreeServiceUrl" &&
                    mi.GetParameters().Length == 1 &&
                    (mi.GetParameters()[0].ParameterType == typeof(string) ||
                    mi.GetParameters()[0].ParameterType == typeof(object)));
            Trace.Info("Tree.ctor, url-method: {0}", treeServiceUrlMethod);
            Trace.Info("TreeAlias: {0}", this.TreeAlias);
            Trace.Info("App: {0}", application);
        }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Icon = FolderIcon;
            rootNode.OpenIcon = FolderIconOpen;
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionRefresh.Instance);
        }

        public override void Render(ref XmlTree tree)
        {
            // renders current level of the tree
            Trace.Info("Rendering tree from: {0}", this.NodeKey);
            try
            {
                var nodes = GetCurrentXNode().Elements();
                foreach (XElement node in nodes.Where(Configuration.IsNavElement))
                {
                    XmlTreeNode xNode = XmlTreeNode.Create(this);
                    string elementId = Configuration.Current.GetElementID(node);
                    xNode.NodeID = HttpContext.Current.Server.UrlPathEncode(elementId);
                    xNode.Text = node.Attribute("name").Value;
                    xNode.Action = GetNavAction(node);
                    xNode.Icon = GetNavIcon(node);

                    if (node.Name.LocalName == Configuration.X_REPOSITORY_ELEMENT)
                    {
                        // todo: find some way of optimizing this performance wise
                        var repos = Configuration.Current.GetRepository(elementId);
                        if (repos.Editor != null) // hitting the editor will do a lot of type instantiation 
                            xNode.Menu.Add(DewdActionCreate.Instance);
                    }

                    // enable child nodes for containers
                    if (node.Elements().Any(Configuration.IsNavElement))
                    {
                        xNode.HasChildren = true;                        
                        //xNode.Source = this.GetTreeServiceUrl((object)xNode.NodeID);
                        xNode.Source = (string)treeServiceUrlMethod.Invoke((object)this, new object[] { xNode.NodeID });
                        Trace.Info("Tree.Render, source {0}", xNode.Source);
                    }
                    tree.Add(xNode);
                }
            }
            catch (Exception ex) {
                Trace.Error(ex);
                throw;
            }
        }

        private string GetNavAction(XElement element)
        {
            string url = element.GetAttribute("url");
            if (url.Length != 0)
                return String.Format("javascript:dewdOpenUrl('{0}')", url);

            if (element.Name == Configuration.X_CONTAINER_ELEMENT)
                return "";
           
            return String.Format("javascript:dewdOpen('{0}');", 
                HttpContext.Current.Server.UrlPathEncode(Configuration.Current.GetElementID(element)));
        }

        private string GetNavIcon(XElement element)
        {            
            var customIcon = element.GetAttribute("icon");
            if (customIcon.Length != 0)
                return customIcon;

            return element.Name.LocalName == Configuration.X_REPOSITORY_ELEMENT ? "doc.gif" : "folder.gif";
        }

        private XElement GetCurrentXNode()
        {
            string path = NodeKey;
            return Configuration.Current.GetElementFromID(path);
        }

        public override void RenderJS(ref StringBuilder js)
        {

            js.Append(
               @"
                    function dewdCreate(txe) {
	                    parent.right.document.location.href = '" + Configuration.BaseUrl + @"Edit.aspx?txe=' + txe;
                    }

                    function dewdOpen(txe) {
	                    parent.right.document.location.href = '" + Configuration.BaseUrl + @"View.aspx?txe=' + txe;
                    }

                    function dewdOpenUrl(url) {
                        parent.right.document.location.href = url;
                    }
                ");
        }

        /// <summary>
        /// This action is invoked upon creation of a document
        /// </summary>
        public class DewdActionCreate : IAction
        {
            //create singleton
            private static readonly DewdActionCreate m_instance = new DewdActionCreate();

            public static DewdActionCreate Instance
            {
                get { return m_instance; }
            }

            #region IAction Members

            public char Letter
            {
                get
                {
                    return 'W';
                }
            }

            public delegate string JsDelegate();
            public JsDelegate JsCallback;

            public string JsFunctionName
            {
                get
                {
                    if (Util.UmbracoVersion >= new Version("4.1.0"))
                        return "dewdCreate(Umbraco.Controls.UmbracoTree.inst.JTree.getActionNode().nodeId);";
                    else
                        return "parent.tree.dewdCreate(parent.nodeID)";
                }
            }

            public string JsSource
            {
                get
                {
                    return null;
                }
            }

            public string Alias
            {
                get
                {
                    return "create";
                }
            }

            public string Icon
            {
                get
                {
                    return ".sprNew";
                }
            }

            public bool ShowInNotifier
            {
                get
                {
                    return false;
                }
            }
            public bool CanBePermissionAssigned
            {
                get
                {
                    return false;
                }
            }

            #endregion
        }
    }
}