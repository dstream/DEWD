using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Editors;
using Eksponent.Dewd.Fields;
using Eksponent.Dewd.Repositories;
using Eksponent.Dewd.Views;
using System.Configuration;
using Eksponent.Dewd.Repositories.Table;

namespace Eksponent.Dewd
{
    public class Configuration
    {
        public const string X_CONTAINER_ELEMENT = "container";
        public const string X_REPOSITORY_ELEMENT = "repository";
        private const string BASE_URL = "/umbraco/plugins/Dewd/";
        private const string APP_SETTING_BASE_KEY = "Eksponent.Dewd.BaseUrl";
        private XDocument document;
        private static readonly string[] repositoryTypeLocations =
            new string[] {
                "Eksponent.Dewd.Repositories.Table.{0},Eksponent.Dewd",
                "Eksponent.Dewd.Repositories.Object.{0},Eksponent.Dewd",
                "Eksponent.Dewd.Repositories.Examine.{0},Eksponent.Dewd",
                "Eksponent.Dewd.Repositories.Umbraco.{0},Eksponent.Dewd",
                "Eksponent.Dewd.Repositories.{0},Eksponent.Dewd"
            };

        private Configuration(XDocument document)
        {
            this.document = document;
        }

        public static string[] DefaultTypeLocations
        {
            get
            {
                return repositoryTypeLocations;
            }
        }

        public static string BaseUrl
        {
            get
            {
                var path = ConfigurationManager.AppSettings[APP_SETTING_BASE_KEY];
                if (String.IsNullOrEmpty(path))
                    path = BASE_URL;
                return path;
            }
        }

        public static bool Debug
        {
            get { return !String.IsNullOrEmpty(ConfigurationManager.AppSettings["Eksponent.Dewd.Debug"]); }
        }

        public static bool UseBaseDataReflectHack
        {
            get { return !String.IsNullOrEmpty(ConfigurationManager.AppSettings["Eksponent.Dewd.UseBaseDataReflectHack"]); }
        }

        public static bool DisableNamespaceRemoval {
            get { return !String.IsNullOrEmpty(ConfigurationManager.AppSettings["Eksponent.Dewd.DisableNamespaceRemoval"]); }
        }

        public XDocument XDocument
        {
            get { return document; }
        }

        internal static bool IsNavElement(XElement element)
        {
            if (element.GetAttribute<bool>("hidden"))
                return false;

            return element.Name.LocalName == X_REPOSITORY_ELEMENT || element.Name.LocalName == X_CONTAINER_ELEMENT;
        }

        public string GetElementID(XElement element)
        {
            return "/" +
                String.Join("/", element.AncestorsAndSelf().
                    Where(IsNamedElement).
                    Reverse().
                    Select(e => e.GetAttribute("name")).ToArray());
        }

        private static bool IsNamedElement(XElement element)
        {
            return element.GetAttribute("name").Length != 0;
        }

        public XElement GetElementFromID(string path)
        {
            var root = Current.XDocument.Root;
            if (String.IsNullOrEmpty(path))
                path = "";

            //Trace.Info("Umbraco verson {0}", umbraco.GlobalSettings.VersionMinor);
            if (Util.UmbracoVersion >= new Version("4.1.0"))
                path = HttpContext.Current.Server.UrlDecode(path);

            string[] steps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (steps.Length == 0)
                return root;

            var node = root;
            int level = 0;
            foreach (string step in steps)
            {
                node = node.Elements()
                    .Where(IsNamedElement)
                    .FirstOrDefault(e => e.Attribute("name").Value == step || step=="*");
                if (node == null)
                    Trace.Warn("Named element '{0}' was not found at level {1}.", step, level);
                level++;
            }
            return node;
        }

        private string GetNearestDefaultRespositoryType(XElement element)
        {
            var typeStr = element.AncestorsAndSelf()
                .Select(e => e.GetAttribute("drt"))
                .FirstOrDefault(s => s.Length != 0);
            return typeStr ?? "";
        }

        public IRepository GetRepository(string path)
        {
            var repositoryElement = GetElementFromID(path);
            string repoType = repositoryElement.GetAttribute("type");
            if(repoType.Length!=0) {
                // custom type
                var type = TypeInstantiater.SearchType(repoType, null, repositoryTypeLocations);
                if (type == null)
                    throw new FormattedException("Custom repository type not found: {0}", repoType);
                return (IRepository)Activator.CreateInstance(type, repositoryElement);
            }
            
            // look up default repository type
            var typeStr = GetNearestDefaultRespositoryType(repositoryElement);
            return TypeInstantiater.GetInstance<IRepository>(
                typeStr, 
                typeof(TableRepository),
                repositoryTypeLocations, 
                repositoryElement);
        }

        public static Configuration Current
        {
            get
            {
                // note: consider some kind of production-mode which stores the loaded config in static field
                var config = RequestTemp.Get<Configuration>("Dewd.Config");
                if(config==null)
                    RequestTemp.Put("Dewd.Config", config = new Configuration((new ConfigurationLoader()).Load()));
                return config;
            }
        }
    }
}