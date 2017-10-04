using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BasePages;
using System.Web.UI;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd
{
    public static class UiHelper
    {
        public static void HandleSpeechFeedback(UmbracoEnsuredPage page)
        {
            if (String.IsNullOrEmpty(page.Request["feedback"]))
                return;

            switch (page.Request["feedback"])
            {
                case "saved":
                    page.speechBubble(BasePage.speechBubbleIcon.save, "Saved", "Changes has been saved successfully.");
                    break;
                case "deleted":
                    page.speechBubble(BasePage.speechBubbleIcon.success, "Deleted", "Item was deleted.");
                    break;
                default:
                    var type = (BasePage.speechBubbleIcon)Enum.Parse(typeof(BasePage.speechBubbleIcon), page.Request["feedback"]);
                    page.speechBubble(type, page.Request["feedbackTitle"] ?? "Info", page.Request["feedbackText"] ?? "");
                    break;
            }
        }

        public static void InjectHtml(Page page, XElement element)
        {
            foreach (XElement injectElement in element.Elements("htmlInject"))
            {
                var location = injectElement.GetAttribute("location", "head");
                Control container = page.Master.FindControl("head").FindControl(location);
                if (container == null)
                    container = page.Master.FindControl("body").FindControl(location);
                if (container == null)
                    throw new FormattedException("Element not found for html injection: {0}", injectElement);

                container.Controls.Add(new LiteralControl(injectElement.Value));
            }
        }
    }
}