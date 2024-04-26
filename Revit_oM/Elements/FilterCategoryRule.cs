using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Elements
{
    public class FilterCategoryRule : FilterRule
    {
        public virtual List<string> CategoryNames { get; set; }
    }
}
