using BH.oM.Adapters.Revit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.FilterRules
{
    public abstract class FilterNumericValueRule : FilterValueRule
    {
        public virtual NumberComparisonType ComparisonType { get; set; }


    }
}
