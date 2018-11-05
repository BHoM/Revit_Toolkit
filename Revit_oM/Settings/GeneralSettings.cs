
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class GeneralSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public Enums.Discipline DefaultDiscipline { get; set; } = Enums.Discipline.Structural;
        public bool Replace { get; set; } = true;
        public string TagsParameterName { get; set; } = "BHE_Tags";

        /***************************************************/
    }
}
