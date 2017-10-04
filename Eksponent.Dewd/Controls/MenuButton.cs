using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using umbraco.uicontrols;
using umbraco;
using System.Web.UI.WebControls;

namespace Eksponent.Dewd.Controls
{
    public class MenuButton
    {
        public string ID { get; set; }
        public string Icon { get; set; }
        public string AltText { get; set; }
        public string ClientClick { get; set; }
        public event EventHandler<ImageClickEventArgs> Click;
        public string Tab { get; set; }

        public MenuButton()
        {
            ID = GetUID();
        }

        private static string GetUID() {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        private void TriggerClick(object sender, ImageClickEventArgs args)
        {
            if (Click != null)
                Click(sender, args);
        }

        public static void AddButtons(WebControl menuContainer, List<MenuButton> buttons)
        {
            if (buttons.Count() == 0)
                return;

            ScrollingMenu menu;
            var buttonPrefix = menuContainer.ID;
            if (menuContainer is TabPage)
            {
                menu = ((TabPage)menuContainer).Menu;
            }
            else if (menuContainer is UmbracoPanel)
            {
                ((UmbracoPanel)menuContainer).hasMenu = true;
                menu = ((UmbracoPanel)menuContainer).Menu;
            }
            else if (menuContainer is ScrollingMenu)
                menu = ((ScrollingMenu)menuContainer);
            else
                throw new FormattedException("Unrecognized menu container {0}", menuContainer.GetType());

            Trace.Info("Adding buttons");
            foreach (MenuButton button in buttons)
            {
                if (button.Tab == null)
                {
                    var innerButton = button; // aha .. needed to get closure+Click-eventhandler working
                    var umbButton = menu.NewImageButton();
                    umbButton.ID = buttonPrefix + "_" + button.ID;
                    umbButton.AltText = ui.Text("buttons", button.AltText, null);
                    umbButton.ImageUrl = button.Icon;
                    if (!String.IsNullOrEmpty(button.ClientClick))
                        umbButton.OnClickCommand = button.ClientClick;

                    umbButton.Click += (obj, args) =>
                    {
                        Trace.Info("Button cliekd");
                        innerButton.TriggerClick(obj, args);
                    };
                }
            }            
        }
    }
}