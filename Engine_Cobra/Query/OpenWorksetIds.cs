using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<WorksetId> OpenWorksetIds(this Document document)
        {
            if (document == null)
                return null;

            FilteredWorksetCollector aFilteredWorksetCollector = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset);

            List<WorksetId> aResult = new List<WorksetId>();
            foreach (Workset aWorkset in aFilteredWorksetCollector)
                if (aWorkset.IsOpen)
                    aResult.Add(aWorkset.Id);

            return aResult;
        }

        /***************************************************/
    }
}
