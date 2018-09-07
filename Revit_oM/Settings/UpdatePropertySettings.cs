using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class UpdatePropertySettings : BHoMObject
    {
        /***************************************************/
        /****            Public Properties              ****/
        /***************************************************/

        public string ParameterName { get; set; } = null;

        public object Value { get; set; } = null;

        public bool ConvertUnits { get; set; } = true;

        /***************************************************/

        public static UpdatePropertySettings Default = new UpdatePropertySettings();

        /***************************************************/
    }
}
