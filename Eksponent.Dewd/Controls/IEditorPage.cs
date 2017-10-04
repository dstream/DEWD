using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BasePages;

namespace Eksponent.Dewd.Controls
{
    public interface IEditorPage
    {
        event EventHandler<EventArgs> ContentControlContainerCreated;
        ContentControlContainer ContentControlContainer { get; }
        void RedirectToView(BasePage.speechBubbleIcon icon, string title, string text);
        void RedirectToView(string predefinedFeedback);
    }
}