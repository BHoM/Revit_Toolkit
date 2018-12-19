using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<WorksetId> WorksetIds(this Document document)
        {
            if (document == null)
                return null;

            FilteredWorksetCollector aFilteredWorksetCollector = new FilteredWorksetCollector(document);

            List<WorksetId> aResult = new List<WorksetId>();
            foreach (Workset aWorkset in aFilteredWorksetCollector)
            {
                if (aWorkset.Kind == WorksetKind.UserWorkset)
                    aResult.Add(aWorkset.Id);
            }

            return aResult;
        }

        /***************************************************/
    }
}
