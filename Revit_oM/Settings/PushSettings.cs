using BH.oM.Base;

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

        public FamilyLibrary FamilyLibrary { get; set; } = null;

        /***************************************************/

        public static PushSettings Default = new PushSettings();

        /***************************************************/
    }
}
