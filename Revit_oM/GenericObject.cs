using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit
{
    public class GenericObject : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public IGeometry Location { get; set; } = null;

        /***************************************************/
    }
}

