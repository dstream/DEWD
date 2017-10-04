using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Eksponent.Dewd.Extensions;

namespace Eksponent.Dewd.Fields.Criteria
{
    public abstract class Criterion : ICriterion
    {
        public XElement CriterionElement { get; private set; }

        public Criterion(XElement element)
        {
            CriterionElement = element;
        }

        public abstract bool IsTrue(object value);

        public static bool IsTrue(XElement criterionElement, object value, Type defaultCriterion)
        {
            bool negate = criterionElement.GetAttribute<bool>("negate");

            /*var type = Configuration.GetCustomType(
                criterionElement.GetAttribute("type"),
                defaultCriterion,
                new string[] { "Eksponent.Dewd.Fields.Criteria.{0},Eksponent.Dewd" });
            var criterion = (ICriterion)Activator.CreateInstance(type, criterionElement);*/

            var criterion = TypeInstantiater.GetInstance<ICriterion>(
                criterionElement.GetAttribute("type"),
                defaultCriterion,
                new string[] { "Eksponent.Dewd.Fields.Criteria.{0},Eksponent.Dewd" },
                criterionElement);

            if (criterion.IsTrue(value))
                return !negate;
            return negate;
        }

        public static bool IsAnyTrue(IEnumerable<XElement> criterionElements, object value, Type defaultCriterion)
        {
            return criterionElements.Any(c => IsTrue(c, value, defaultCriterion));
        }
    }
}