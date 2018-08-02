using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

namespace BH.oM.Adapters.Revit
{
    public class WorksetSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public bool OpenWorksetsOnly { get; set; } = false;

        public IEnumerable<int> WorksetIds { get; set; } = new List<int>();

        /***************************************************/
    }
}
