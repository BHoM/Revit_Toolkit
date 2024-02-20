using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public abstract class FilterRule : BHoMObject
    {
        public virtual FilterRuleType RuleType { get; set; }
        public virtual string ParameterName { get; set; }
        public virtual object Value { get; set; }

    }

}
