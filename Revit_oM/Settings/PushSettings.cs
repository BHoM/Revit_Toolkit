using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

namespace BH.oM.Adapters.Revit
{
    public class PushSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/
        public bool CopyCustomData { get; set; } = true;

        public bool ConvertUnits { get; set; } = true;

        public bool Replace { get; set; } = true;

        /***************************************************/

        public static PushSettings Default = new PushSettings();
    }
}
