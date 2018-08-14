using BH.oM.Base;
using System.Collections.Generic;

namespace BH.oM.Revit
{
    public class SelectionSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public bool IncludeSelected { get; set; } = false;

        public IEnumerable<int> ElementIds { get; set; } = new List<int>();

        public IEnumerable<string> UniqueIds { get; set; } = new List<string>();

        public IEnumerable<string> CategoryNames { get; set; } = new List<string>();

        /***************************************************/
    }
}
