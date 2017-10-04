using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Configuration;
using Eksponent.Dewd.Extensions;
using System.IO;

namespace Eksponent.Dewd
{
    /// <summary>
    ///     Usage:    
    ///     <tweak do="remove" select="../field" />
    ///     <tweak do="include" select="../field" />
    ///     <tweak do="include" external="/config/dewd/*.config" select="../field" />
    /// </summary>
    public class ConfigurationLoader
    {
        private XDocument document;
        private const string APP_SETTING_KEY = "Eksponent.Dewd.ConfigurationPath";

        public ConfigurationLoader()
        {
        }

        private string GetAbsolutePath(string path)
        {
            if (path.StartsWith("/") || path.StartsWith("~"))
                path = HttpContext.Current.Server.MapPath(path);
            return path;
        }

        private string GetPath() {
            var path = ConfigurationManager.AppSettings[APP_SETTING_KEY];
            if(String.IsNullOrEmpty(path))
                path = "/config/Eksponent.Dewd.config";
            return GetAbsolutePath(path);
        }

        private List<XElement> GetExternalXDocuments(string wildcardPath)
        {
            var list = new List<XElement>();
            var path = wildcardPath;
            
            // check for absolute file system path
            var files = new List<string>();
            if (!path.Contains("://"))
            {
                if (path.Contains('*') || path.Contains('?'))
                {
                    // get files from search pattern
                    var dir = Path.GetDirectoryName(path);
                    var pattern = path.Substring(dir.Length+1);
                    dir = GetAbsolutePath(path.Substring(0,path.Length - pattern.Length));
                    Trace.Info("ConfigurationLoader. Pattern found, dir: {0} and pattern: {1}", dir, pattern);

                    if (Directory.Exists(dir))
                        files.AddRange(Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly));
                    else
                        Trace.Info("ConfigurationLoader. Ignoring non-existing directory in wildcard: {0}", path);
                }
                else
                {
                    // just add the specificed file
                    files.Add(GetAbsolutePath(path));
                    Trace.Info("ConfigurationLoader. Single file found: {0}", path);
                }
            }

            foreach (string filePath in files)
            {
                var doc = XDocument.Load(filePath);
                doc = RemoveRootNs(doc);
                list.Add(doc.Root);
                Trace.Info("Loaded external: {0}", filePath);
            }
            return list;
        }

        static XDocument RemoveRootNs(XDocument document)
        {
            if (Configuration.DisableNamespaceRemoval || document.Root.Name.Namespace == XNamespace.None)
                return document;
            foreach (XElement e in document.Root.DescendantsAndSelf())
            {
                if (e.Name.Namespace != XNamespace.None)
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);

                if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                    e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
            }
            return document;
        }

        public XDocument Load() {
            document = XDocument.Load(GetPath());
            document = RemoveRootNs(document);

            var stack = new Stack<XElement>();
            stack.Push(document.Root); // push root node

            do
            {
                // get node to check for includes
                var checkNode = stack.Pop();

                // search check-node for include-nodes
                foreach (XElement tweakNode in checkNode.Descendants("tweak"))
                {
                    switch (tweakNode.GetAttribute("do"))
                    {
                        case "clone":
                            DoClone(stack, tweakNode);
                            break;
                        case "remove":
                            GetSelection(tweakNode, tweakNode).Remove(); 
                            break;
                    }
                }
            } while (stack.Count > 0);

            // clean up tweaks
            document.Descendants("tweak").Remove();

            if (Configuration.Debug)
                document.Save(HttpContext.Current.Server.MapPath("/data/dewd-debug.config"));

            return document;
        }

        private IEnumerable<XElement> GetSelection(XElement root, XElement tweakNode)
        {
            string xpath = tweakNode.GetAttribute("select", "./*[name(.)='container' or name(.)='repository']");
            IEnumerable<XElement> selection;
            try
            {
                selection = root.XPathSelectElements(xpath);
            }
            catch
            {
                throw new FormattedException("Unable to make xpath selection in include: {0}", tweakNode);
            }
            return selection;
        }

        private void DoClone(Stack<XElement> stack, XElement tweakNode)
        {
            List<XElement> alienRoots;
            if (tweakNode.GetAttribute("external").Length != 0)
                alienRoots = GetExternalXDocuments(tweakNode.GetAttribute("external"));
            else
                alienRoots = new List<XElement>() { tweakNode };

            foreach (XElement alienRoot in alienRoots)
            {
                // get elements to import
                var selection = GetSelection(alienRoot, tweakNode);
                foreach (XElement element in selection.Reverse())
                {
                    tweakNode.AddAfterSelf(element);
                    stack.Push(element);
                }
            }
        }
    }
}

