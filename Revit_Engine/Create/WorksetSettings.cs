﻿using System.Collections.Generic;
using BH.oM.Revit;

namespace BH.Engine.Revit
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
