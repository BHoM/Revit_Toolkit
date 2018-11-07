using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates General Settings class which contols general behaviour of Adapter")]
        [Input("defaultDiscipline", "Default disciplne for pull method")]
        [Input("replace", "Replace existing elements in the model for push method. Update parameters (CustomData) only if set to false.")]
        [Input("tagsParameterName", "Name of the parameter which stores Tags assigned to BHoM object")]
        [Output("GeneralSettings")]
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
