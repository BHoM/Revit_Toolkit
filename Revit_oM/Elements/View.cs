using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Views
{
    public class View : BHoMObject
    {
        public virtual List<ViewFilterWithOverrides> FiltersWithOverrides { get; set; }
    }
}
