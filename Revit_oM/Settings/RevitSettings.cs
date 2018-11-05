﻿
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class RevitSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public ConnectionSettings ConnectionSettings { get; set; } = new ConnectionSettings();
        public FamilyLoadSettings FamilyLoadSettings { get; set; } = new FamilyLoadSettings();
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();

        /***************************************************/
    }
}
