using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        public static PullSettings DefaultIfNull(this PullSettings pullSettings)
        {
            if (pullSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordNote("Pull settings are not set. Default settings are used.");
                return PullSettings.Default;
            }

            return pullSettings;
        }

        /***************************************************/

        public static PushSettings DefaultIfNull(this PushSettings pushSettings)
        {
            if (pushSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordNote("Push settings are not set. Default settings are used.");
                return PushSettings.Default;
            }

            return pushSettings;
        }


        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        /***************************************************/


    }
}