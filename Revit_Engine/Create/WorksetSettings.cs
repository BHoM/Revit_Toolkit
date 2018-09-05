using System.Collections.Generic;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static WorksetSettings WorksetSettings(bool openWorksetsOnly = false, IEnumerable<int> worksetIds = null, IEnumerable<string> worksetNames = null)
        {
            WorksetSettings aWorksetSettings = new WorksetSettings()
            {
                OpenWorksetsOnly = openWorksetsOnly,
                WorksetIds = worksetIds,
                WorksetNames = worksetNames
            };

            return aWorksetSettings;
        }

        /***************************************************/

        public static WorksetSettings WorksetSettings(IEnumerable<string> worksetNames)
        {
            WorksetSettings aWorksetSettings = new WorksetSettings()
            {
                WorksetNames = worksetNames
            };

            return aWorksetSettings;
        }
    }
}
