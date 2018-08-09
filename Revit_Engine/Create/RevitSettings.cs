using BH.oM.Revit;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings RevitSettings(int pushPort = 14128, int pullPort = 14129, int maxMinutesToWait = 10, WorksetSettings worksetSettings = null, SelectionSettings selectionSettings = null)
        {
            RevitSettings aRevitSettings = new RevitSettings()
            {
                PushPort = pushPort,
                PullPort = pullPort,
                MaxMinutesToWait = maxMinutesToWait
            };

            if (worksetSettings != null)
                aRevitSettings.WorksetSettings = worksetSettings;

            if (selectionSettings != null)
                aRevitSettings.SelectionSettings = selectionSettings;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
