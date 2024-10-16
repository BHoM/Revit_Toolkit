using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public class ViewFilterWithOverrides: BHoMObject
    {
        public virtual ViewFilter Filter { get; set; }
        public virtual OverrideGraphicSettings Overrides { get; set; }
    }
}
