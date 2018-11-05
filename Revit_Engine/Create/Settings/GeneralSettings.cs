using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GeneralSettings GeneralSettings(Discipline defaultDiscipline = Discipline.Structural, bool replace = true, string tagsParameterName = "BHE_Tags")
        {
            return new GeneralSettings()
            {
                DefaultDiscipline = defaultDiscipline,
                Replace = replace,
                TagsParameterName = tagsParameterName

            };
        }

        /***************************************************/
    }
}
