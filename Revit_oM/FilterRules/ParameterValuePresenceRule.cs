using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.FilterRules
{
    public class ParameterValuePresenceRule : FilterValueRule
    {
        public virtual bool IsPresent { get; set; }
    }
}
