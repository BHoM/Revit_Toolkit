using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings RevitSettings(int pushPort = 14128, int pullPort = 14129, int maxMinutesToWait = 10, Discipline defaultDiscipline = Discipline.Structural, string tagsParameterName = "BHE_Tags", FamilyLibrary familyLibrary = null)
        {
            RevitSettings aRevitSettings = new RevitSettings()
            {
                PushPort = pushPort,
                PullPort = pullPort,
                MaxMinutesToWait = maxMinutesToWait,
                DefaultDiscipline = defaultDiscipline,
                TagsParameterName = tagsParameterName
            };

            if (familyLibrary != null)
                aRevitSettings.FamilyLibrary = familyLibrary;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
