using System.Collections.Generic;
using BH.oM.Revit;
using System.Linq;

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
                CategoryNames = categoryNames
            };

            return aSelectionSettings;
        }

        public static SelectionSettings SelectionSettings(IEnumerable<string> categoryNames)
        {
            SelectionSettings aSelectionSettings = new SelectionSettings()
            {
                CategoryNames = categoryNames
            };

            return aSelectionSettings;
        }

        public static SelectionSettings SelectionSettings(bool includeSelected = false)
        {
            SelectionSettings aSelectionSettings = new SelectionSettings()
            {
                IncludeSelected = includeSelected
            };

            return aSelectionSettings;
        }

        /***************************************************/
    }
}
