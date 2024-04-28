using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Revit.Enums;

namespace BH.oM.Adapters.Revit.Elements
{
    public class FilterLevelRule : FilterRule
    {
        public virtual string LevelName { get; set; }
        public virtual LevelComparisonType Evaluator { get; set; }

    }
}
