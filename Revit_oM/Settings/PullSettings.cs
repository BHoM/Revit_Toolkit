using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

namespace BH.oM.Adapters.Revit
{
    public class PullSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Discipline Discipline { get; set; } = Discipline.Environmental;

        public bool CopyCustomData { get; set; } = true;

        public bool ConvertUnits { get; set; } = true;

        public Dictionary<int, List<IBHoMObject>> RefObjects = null;

        /***************************************************/

        public static PullSettings Default = new PullSettings();
    }

    
}
