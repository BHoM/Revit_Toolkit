using BH.oM.Adapters.Revit;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static SelectionSettings ElementIds(this SelectionSettings selectionSettings, IEnumerable<int> elementIds)
        {
            if (selectionSettings == null)
                return null;

            SelectionSettings aSelectionSettings = selectionSettings.GetShallowClone() as SelectionSettings;
            aSelectionSettings.ElementIds = elementIds;

            return aSelectionSettings;
        }

        /***************************************************/
    }
}