using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Eksponent.Dewd.Controls;
using System.Xml.Linq;
using umbraco.DataLayer;
using umbraco;
using Eksponent.Dewd.Extensions;
using Eksponent.Dewd.Fields.ValueGetters;

namespace Eksponent.Dewd.Repositories.Table
{
    public class TableButton : CustomButton
    {
        public XElement ButtonElement { get; set; }

        public TableButton(XElement buttonElement) : base(buttonElement)
        {
            ButtonElement = buttonElement;
        }

        public override void OnClick(object sender, EventArgs e)
        {
            var dsn = TableRepository.GetNearestConnectionString(ButtonElement);
            var sql = ButtonElement.Element("sql").Value;

            using (ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(dsn))
            {
                var parameters = ButtonElement.Elements("parameter")
                    .Select(x => sqlHelper.CreateParameter(x.GetAttribute("name"), ValueGetter.GetFromValueGetter(x)))
                    .ToArray();

                sqlHelper.ExecuteNonQuery(sql, parameters);
            }

            base.OnClick(sender, e);
        }
    }
}