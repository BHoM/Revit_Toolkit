using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using System.Xml.Linq;

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
