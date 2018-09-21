using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings RevitSettings(int pushPort = 14128, int pullPort = 14129, int maxMinutesToWait = 10, Discipline defaultDiscipline = Discipline.Environmental)
        {
            RevitSettings aRevitSettings = new RevitSettings()
            {
                PushPort = pushPort,
                PullPort = pullPort,
                MaxMinutesToWait = maxMinutesToWait,
                DefaultDiscipline = defaultDiscipline
            };

            return aRevitSettings;
        }

        /***************************************************/
    }
}
