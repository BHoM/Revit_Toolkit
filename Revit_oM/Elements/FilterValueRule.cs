using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public abstract class FilterValueRule : FilterRule
    {
        public virtual String parameterName { get; set; }
        public virtual String value { get; set; }

    }
}
