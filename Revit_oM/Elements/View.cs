using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Revit.Views;

namespace BH.oM.Adapters.Revit.Elements
{
    public class View : BHoMObject
    {
        public virtual List<ViewFilterWithOverrides> FiltersWithOverrides { get; set; }
    }
}
