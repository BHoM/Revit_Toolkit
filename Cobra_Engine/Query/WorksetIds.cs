using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Reads Workset Ids from Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <returns name="Ids">Revit WorksetIds</returns>
        /// <search>
        /// Query, BHoM, WorksetIds, Document
        /// </search>
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
    }
}
