using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Fields;
using Eksponent.Dewd.Editors;
using System.Xml.Linq;

namespace Eksponent.Dewd.Extensions
{
    public static class EditorExtensions
    {
        public static List<IField> GetFields(this IEditor editor, XElement editorElement, Type defaultField, string[] additionalSearchPaths)
        {
            var paths = new List<string>();
            paths.Add("Eksponent.Dewd.Fields.{0},Eksponent.Dewd");
            paths.AddRange(additionalSearchPaths);

            var fields = new List<IField>();
            foreach (XElement fieldElement in editorElement.Elements("field"))
            {
                var field = TypeInstantiater.GetInstance<IField>(
                    fieldElement.GetAttribute("type"),
                    defaultField,
                    paths.ToArray(),
                    fieldElement,
                    editor);
                fields.Add(field);
            }
            return fields;
        }
    }
}