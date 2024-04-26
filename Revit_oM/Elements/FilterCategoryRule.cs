using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public class FilterCategoryRule : FilterRule
    {
        public virtual List<string> categoryNames { get; set; }
    }
}
