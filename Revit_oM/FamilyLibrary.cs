using System.Collections.Generic;
using BH.oM.Base;

namespace BH.oM.Revit
{
    public class FamilyLibrary : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Dictionary { get; set; } = null;

        /***************************************************/
    }
}
