using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit
{
    public class FilterWithOverrides
    {
        public virtual ViewFilter Filter { get; set; }

        //public virtual List<Override> Overrides { get; set; }
    }
}
