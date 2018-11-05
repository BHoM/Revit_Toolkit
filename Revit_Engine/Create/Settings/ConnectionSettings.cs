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

        public static ConnectionSettings ConnectionSettings(int pushPort = 14128, int pullPort = 14129, int maxMinutesToWait = 10)
        {
            return new ConnectionSettings()
            {
                PushPort = pushPort,
                PullPort = pullPort,
                MaxMinutesToWait = maxMinutesToWait

            };
        }

        /***************************************************/
    }
}
