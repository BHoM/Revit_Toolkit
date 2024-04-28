using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public abstract class FilterValueRule : FilterRule
    {
        public virtual string ParameterName { get; set; }
        public virtual string Value { get; set; }

    }
}
