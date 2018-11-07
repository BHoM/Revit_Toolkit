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

        [Description("Creates Connection Settings Class which stores basic information about Adapter Connection settings such as Push Port, Pull Port and Maximum Time to Wait")]
        [Input("pushPort", "Push Port for Revit Adapter")]
        [Input("pullPort", "Pull Port for Revit Adapter")]
        [Input("maxMinutesToWait", "Max Time to wait for response [min]")]
        [Output("ConnectionSettings")]
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
