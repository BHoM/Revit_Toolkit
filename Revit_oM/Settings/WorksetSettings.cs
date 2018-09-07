using BH.oM.Base;
using System.Collections.Generic;

namespace BH.oM.Adapters.Revit.Settings
{
    public class WorksetSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public bool OpenWorksetsOnly { get; set; } = false;

        public IEnumerable<int> WorksetIds { get; set; } = new List<int>();

        public IEnumerable<string> WorksetNames { get; set; } = new List<string>();

        /***************************************************/
    }
}
