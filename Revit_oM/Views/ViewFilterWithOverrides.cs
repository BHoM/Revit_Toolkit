using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Revit.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit
{
    public class ViewFilterWithOverrides
    {
        public virtual ViewFilter Filter { get; set; }
        public virtual List<OverrideGraphicSettings> Overrides { get; set; }
    }
}
