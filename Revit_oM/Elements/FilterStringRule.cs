using BH.oM.Adapters.Revit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public class FilterStringRule : FilterValueRule
    {
        public virtual TextComparisonType Evaluator { get; set; }
    }
}
