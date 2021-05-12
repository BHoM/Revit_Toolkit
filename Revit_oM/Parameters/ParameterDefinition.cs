using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Parameters
{
    public class ParameterDefinition : BHoMObject
    {
        public virtual string GroupName { get; set; } = ""; //TODO: only relevant to shared - split into 2 different? or merge this with paramGroup better!
        public virtual string ParameterGroup { get; set; } = "";
        public virtual string ParameterType { get; set; } = "";
        public virtual bool Instance { get; set; } = true;
        public virtual List<string> Categories { get; set; } = null;
        public virtual bool Shared { get; set; } = false;
        //TODO: add discipline!
    }
}
