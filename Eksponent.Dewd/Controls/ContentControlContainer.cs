using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.uicontrols;
using umbraco;
using System.Collections.Generic;
using System;
using umbraco.cms.businesslogic.datatype;
using System.Reflection;

namespace Eksponent.Dewd.Controls
{
    /// <summary>
    /// ContentControlContainer emulates the behaviour in umbracos ContentControl (in a generic way).
    /// </summary>
    public class ContentControlContainer : TabView
    {
        //private ArrayList _dataFields = new ArrayList();
        private Dictionary<string, Field> fields = new Dictionary<string, Field>();
        public List<MenuButton> MenuButtons { get; private set; }

        public class SaveEventArgs : EventArgs
        {
            public Dictionary<string, object> Values { get; set; }
            public bool Return { get; set; }
            
            public SaveEventArgs(Dictionary<string, object> values)
            {
                Values = values;
            }
        }

        public event EventHandler<SaveEventArgs> Save;
        public event EventHandler Delete;
        public event EventHandler Cancel;
        public event EventHandler PreAddButtons;
        internal const string CONTROL_ID = "TabView1";

        public ContentControlContainer()
        {
            ID = CONTROL_ID; // note: important ID is set here, because TabView uses this for client-side stuff
            Width = 350;
            Height = 350;
            MenuButtons = new List<MenuButton>();
        }

        public void DisplayError(string header, string body)
        {
            foreach (TabPage tp in GetPanels())
            {
                tp.ErrorControl.Visible = true;
                tp.ErrorHeader = header;
                tp.ErrorControl.Controls.AddAt(3, new LiteralControl(body));
                tp.CloseCaption = ui.Text("close");
            }
        }

        public void AddControls(IEnumerable<ContentControlDefinition> definitions)
        {
            var tabDefs = new Dictionary<string, TabPage>();
            foreach (ContentControlDefinition def in definitions)
            {
                if (!tabDefs.ContainsKey(def.TabTitle))
                {
                    var newTab = NewTabPage(def.TabTitle);
                    tabDefs.Add(def.TabTitle, newTab);
                    MenuButton.AddButtons(newTab, MenuButtons);
                }

                var tab = tabDefs[def.TabTitle];
                tab.Style.Add("text-align", "center");
                AddControl(tab, def);
            }
        }

        private class Field
        {
            public ContentControlDefinition Definition { get; set; }
            public IDataType DataTypeInstance { get; set; }
        } 

        private void AddControl(TabPage tp, ContentControlDefinition definition)
        {
            IDataType dt = definition.DataTypeDefinition.DataType;

            if (Configuration.UseBaseDataReflectHack)
            {
                // hack: override DefaultData
                try
                {
                    var dd = new Eksponent.Dewd.Controls.EditorControls.NoSqlDefaultData((BaseDataType)dt);
                    var bdField = dt.GetType().GetField("_baseData", BindingFlags.NonPublic | BindingFlags.Instance);
                    bdField.SetValue(dt, dd);
                }
                catch (Exception ex)
                {
                    Trace.Warn("Unable to override _baseData value:\n" + ex.ToString());
                }
            }
            
            var internalFieldId = fields.Count;
            var fieldName = String.Format("field{0}", internalFieldId);
            dt.DataEditor.Editor.ID = fieldName;
            dt.Data.PropertyId = 0; // hack: because DefaultData updates sql from PropertyId, we set it to zero
            try
            {
                dt.Data.Value = definition.InitialValue;
            }
            catch (Exception ex)
            {
                // hack: may cause non-critical exception, due to changes in v4.5
                Trace.Warn("ContentControlContainer, DataType.Data.Value failed: {0}", ex);
            }
            if (definition.InitializeTypeInstanceCallBack != null)
                definition.InitializeTypeInstanceCallBack(dt);

            // hack: in order to get TinyMCE (Richtext editor) to work, the umbraco_toolbar_id must be set
            // to some unique number
            var mce = (dt.DataEditor as umbraco.editorControls.tinyMCE3.TinyMCE);
            if (mce != null) 
            {
                var toolbarId = mce.ElementIdPreFix + internalFieldId.ToString();
                // hack^2: For some reason another umbraco_toolbar_id is added later. 
                // Fortunately mce config settings aren't encoded, so we just "comment it out".
                mce.config.Add("umbraco_toolbar_id", toolbarId + "',//");
            }

            fields.Add(fieldName, new Field() { DataTypeInstance = dt, Definition = definition });

            // check for buttons
            IDataFieldWithButtons df1 = dt.DataEditor.Editor as IDataFieldWithButtons;
            if (df1 != null)
                AddTopButtons(tp, dt, fieldName, df1);

            // check for element additions
            IMenuElement menuElement = dt.DataEditor.Editor as IMenuElement;
            if (menuElement != null)
            {
                // add separator
                tp.Menu.InsertSplitter();

                // add the element
                tp.Menu.NewElement(menuElement.ElementName, menuElement.ElementIdPreFix + internalFieldId.ToString(),
                                   menuElement.ElementClass, menuElement.ExtraMenuWidth);
            }

            Pane pp = new Pane();
            Control holder = new Control();
            holder.Controls.Add(dt.DataEditor.Editor);
            
            string title = definition.Title;
            if(!String.IsNullOrEmpty(definition.Caption))
                title += String.Format("<br /><small>{0}</small>", definition.Caption);            
            pp.addProperty(title, holder);

            // This is once again a nasty nasty hack to fix gui when rendering wysiwygeditor
            if (dt.DataEditor.TreatAsRichTextEditor)
            {
                tp.Controls.Add(dt.DataEditor.Editor);
            }
            else
            {
                Panel ph = new Panel();
                ph.Attributes.Add("style", "padding: 0px 0px 0px 0px");
                ph.Controls.Add(pp);

                tp.Controls.Add(ph);
            }
        }

        private static void AddTopButtons(TabPage tp, IDataType dt, string fieldName, IDataFieldWithButtons df1)
        {
            ((Control)df1).ID = fieldName;

            if (df1.MenuIcons.Length > 0)
                tp.Menu.InsertSplitter();

            // Add buttons
            int c = 0;
            bool atEditHtml = false;
            bool atSplitter = false;
            foreach (object o in df1.MenuIcons)
            {
                try
                {
                    MenuIconI m = (MenuIconI)o;
                    MenuIconI mi = tp.Menu.NewIcon();
                    mi.ImageURL = m.ImageURL;
                    mi.OnClickCommand = m.OnClickCommand;
                    mi.AltText = m.AltText;
                    mi.ID = tp.ID + "_" + m.ID;

                    if (m.ID == "html")
                        atEditHtml = true;
                    else
                        atEditHtml = false;

                    atSplitter = false;
                }
                catch(Exception ex)
                {
                    tp.Menu.InsertSplitter();
                    atSplitter = true;
                    Trace.Error(ex);
                }

                // Testing custom styles in editor
                if (atSplitter && atEditHtml && dt.DataEditor.TreatAsRichTextEditor)
                {
                    DropDownList ddl = tp.Menu.NewDropDownList();

                    ddl.Style.Add("margin-bottom", "5px");
                    ddl.Items.Add(ui.Text("buttons", "styleChoose", null));
                    ddl.ID = tp.ID + "_editorStyle";
                    if (StyleSheet.GetAll().Length > 0)
                    {
                        foreach (StyleSheet s in StyleSheet.GetAll())
                        {
                            foreach (StylesheetProperty sp in s.Properties)
                            {
                                ddl.Items.Add(new ListItem(sp.Text, sp.Alias));
                            }
                        }
                    }
                    ddl.Attributes.Add("onChange", "addStyle(this, '" + fieldName + "');");
                    atEditHtml = false;
                }
                c++;
            }
        }

        private void SaveHandler(object sender, ImageClickEventArgs e)
        {
            var saveValues = new Dictionary<string,object>();
            foreach (Field field in fields.Values)
            {
                var dataType = field.DataTypeInstance;
                ((IDataEditor)dataType.DataEditor.Editor).Save();
                saveValues.Add(field.Definition.ID, dataType.Data.Value);
                //Trace.Info("{0}: {1}", field.Definition.ID, dataType.Data.Value);
            }
            if(Save!=null)
                Save(this, new SaveEventArgs(saveValues)
                {
                    Return = ((Control)sender).ID.Contains("back")
                });
        }

        public class SaveButton : MenuButton
        {
            public SaveButton(ContentControlContainer container, string id, string icon)
            {
                ID = id;
                Icon = icon;
                AltText = umbraco.ui.Text("buttons", "save", null);
                Click += (obj, args) => { container.SaveHandler(obj, args); };
            }
        }

        public class DeleteButton : MenuButton
        {
            public DeleteButton(ContentControlContainer container)
            {
                ID = "delete";
                AltText = ui.Text("buttons", "remove", null);
                Icon = Configuration.BaseUrl + "cross.png";
                ClientClick = "return confirm('Sure?')";
                Click += (obj, args) => {
                    if (container.Delete != null)
                        container.Delete(this, new EventArgs());
                };
            }
        }

        public class CancelButton : MenuButton
        {
            public CancelButton(ContentControlContainer container)
            {
                ID = "cancel";
                Icon = Configuration.BaseUrl + "back.png";
                AltText = ui.Text("buttons", "cancel", null);
                Click += (obj, args) =>
                {
                    if (container.Cancel != null)
                        container.Cancel(this, new EventArgs());
                };
            }
        }
    }
}
