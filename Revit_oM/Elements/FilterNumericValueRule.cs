using BH.oM.Adapters.Revit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public class FilterNumericValueRule : FilterValueRule
    {
        public virtual NumberComparisonType Evaluator { get; set; }


    }
}
