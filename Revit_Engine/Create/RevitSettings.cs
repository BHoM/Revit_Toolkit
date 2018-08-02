using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings RevitSettings(int pushPort = 14128, int pullPort = 14129, int maxMinutesToWait = 10)
        {
            RevitSettings aRevitSettings = new RevitSettings()
            {
                PushPort = pushPort,
                PullPort = pullPort,
                MaxMinutesToWait = maxMinutesToWait
            };

            return aRevitSettings;
        }

        /***************************************************/
    }
}
