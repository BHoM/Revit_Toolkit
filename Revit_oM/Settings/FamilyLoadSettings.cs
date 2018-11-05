
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class FamilyLoadSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Generic.FamilyLibrary FamilyLibrary { get; set; } = new Generic.FamilyLibrary();
        public bool OverwriteFamily { get; set; } = true;
        public bool OverwriteParameterValues { get; set; } = true;

        /***************************************************/
    }
}
