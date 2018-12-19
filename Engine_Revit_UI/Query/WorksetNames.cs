using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<string> WorksetNames(this Document document)
        {
            if (document == null)
                return null;

            FilteredWorksetCollector aFilteredWorksetCollector = new FilteredWorksetCollector(document);

            List<string> aResult = new List<string>();
            foreach (Workset aWorkset in aFilteredWorksetCollector)
            {
                if (aWorkset.Kind == WorksetKind.UserWorkset)
                    aResult.Add(aWorkset.Name);
            }

            return aResult;
        }

        /***************************************************/
    }
}
