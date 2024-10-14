using BH.oM.Adapters.Revit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.FilterRules
{
    public class FilterStringRule : FilterValueRule
    {
        public virtual TextComparisonType ComparisonType { get; set; }
    }
}
