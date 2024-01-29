using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Elements
{
    public class ViewFilter : BHoMObject
    {
        public virtual List<string> Categories {  get; set; }

        //public List<FilterRule> Rules { get; set; }
    }
}
