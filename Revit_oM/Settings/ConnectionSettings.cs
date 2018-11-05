
using BH.oM.Base;

namespace BH.oM.Adapters.Revit.Settings
{
    public class ConnectionSettings : BHoMObject
    {
        /***************************************************/
        /**** Public Properties                        ****/
        /***************************************************/

        public int PushPort { get; set; } = 14128;
        public int PullPort { get; set; } = 14129;
        public int MaxMinutesToWait { get; set; } = 10;

        /***************************************************/
    }
}
