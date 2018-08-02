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

        public static SelectionSettings SelectionSettings(IEnumerable<int> elementIds = null, IEnumerable<string> uniqueIds = null)
        {
            SelectionSettings aSelectionSettings = new SelectionSettings()
            {
                ElementIds = elementIds,
                UniqueIds = uniqueIds,
            };

            return aSelectionSettings;
        }

        /***************************************************/
    }
}
