using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;

namespace BH.oM.Adapters.Revit
{
    public class SelectionSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public bool IncludeSelected { get; set; } = false;

        public IEnumerable<int> ElementIds { get; set; } = new List<int>();

        public IEnumerable<string> UniqueIds { get; set; } = new List<string>();

        /***************************************************/
    }
}
