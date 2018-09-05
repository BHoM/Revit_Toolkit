using BH.oM.Base;
using System.Collections.Generic;

namespace BH.oM.Adapters.Revit
{
    public class PullSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Discipline Discipline { get; set; } = Discipline.Structural;

        public bool CopyCustomData { get; set; } = true;

        public bool ConvertUnits { get; set; } = true;

        public Dictionary<int, List<IBHoMObject>> RefObjects = null;

        /***************************************************/

        public static PullSettings Default = new PullSettings();

        /***************************************************/
    }
}
