using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Controls
{
    public class CustomButton
    {
        public string ID { get; set; }
        public string Icon { get; set; }
        public string AltText { get; set; }
        public string ClientClick { get; set; }
        public string ServerMethodName { get; set; }
        const string UB_KEY = "Eksponent.Dewd.Controls.CustomButton";

        string GetUniqueButtonID()
        {
            if (HttpContext.Current.Items[UB_KEY] == null)
                HttpContext.Current.Items[UB_KEY] = 0;
            var current = (int)HttpContext.Current.Items[UB_KEY];
            current++;
            HttpContext.Current.Items[UB_KEY] = current;
            return String.Format("customButton{0}", current);
        }

        public CustomButton(XElement buttonElement)
        {
            string method = buttonElement.GetAttribute("handler");
            ID = buttonElement.GetAttribute("id", GetUniqueButtonID());
            Icon = buttonElement.GetAttribute("icon", "/umbraco/images/about.png");
            AltText = buttonElement.GetAttribute("text", "My Custom Button");
            ClientClick = buttonElement.GetAttribute("onClientClick");
            ServerMethodName = method;
        }

        public static List<CustomButton> GetCustomButtons(XElement containerElement)
        {
            var list = new List<CustomButton>();
            var buttonTypePaths = new List<string>(Configuration.DefaultTypeLocations);
            buttonTypePaths.Add("Eksponent.Dewd.Views.Buttons.{0},Eksponent.Dewd");
            foreach (XElement buttonElement in containerElement.Elements("button"))
            {
                var button = TypeInstantiater.GetInstance<CustomButton>(
                    buttonElement.GetAttribute("type"),
                    typeof(CustomButton),
                    buttonTypePaths.ToArray(),
                    buttonElement);
                list.Add(button);
            }
            return list;
        }

        public virtual void OnClick(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ServerMethodName))
                MethodCaller.Invoke(ServerMethodName, sender, e);
        }

        public MenuButton GetMenuButton()
        {
            var button = new MenuButton()
            {
                ID = ID,
                Icon = Icon,
                AltText = AltText,
                ClientClick = ClientClick
            };
            button.Click += (sender, e) => { OnClick(sender, e); };

            return button;
        }
    }
}