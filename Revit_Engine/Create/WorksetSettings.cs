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

        public static WorksetSettings WorksetSettings(bool openWorksetsOnly = false, IEnumerable<int> worksetIds = null)
        {
            WorksetSettings aWorksetSettings = new WorksetSettings()
            {
                OpenWorksetsOnly = openWorksetsOnly,
                WorksetIds = worksetIds
            };

            return aWorksetSettings;
        }

        /***************************************************/
    }
}
