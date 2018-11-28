using BH.oM.Base;
using System;
using System.Collections.Generic;

namespace BH.oM.Adapters.Revit.Settings
{
    public class PushSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/

        public bool CopyCustomData { get; set; } = true;
        public bool ConvertUnits { get; set; } = true;
        public bool Replace { get; set; } = true;
        public FamilyLoadSettings FamilyLoadSettings { get; set; } = null;
        public Dictionary<Guid, List<int>> RefObjects = null;

        /***************************************************/

        public static PushSettings Default = new PushSettings();

        /***************************************************/
    }
}
