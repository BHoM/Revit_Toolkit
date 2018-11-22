using BH.oM.Base;

using System.Collections.Generic;

namespace BH.oM.Adapters.Revit.Settings
{
    public class PullSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Enums.Discipline Discipline { get; set; } = Enums.Discipline.Structural;
        public bool CopyCustomData { get; set; } = true;
        public bool ConvertUnits { get; set; } = true;
        public Dictionary<int, List<IBHoMObject>> RefObjects = null;

        /***************************************************/

        public static PullSettings Default = new PullSettings();

        /***************************************************/
    }
}
