using System.Collections.Generic;
using BH.oM.Revit;

namespace BH.Engine.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static SelectionSettings SelectionSettings(bool includeSelected = false, IEnumerable<int> elementIds = null, IEnumerable<string> uniqueIds = null, IEnumerable<string> categoryNames = null)
        {
            SelectionSettings aSelectionSettings = new SelectionSettings()
            {
                IncludeSelected = includeSelected,
                ElementIds = elementIds,
                UniqueIds = uniqueIds,
                CategoryNames = categoryNames,
            };

            return aSelectionSettings;
        }

        /***************************************************/
    }
}
