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
        /// Reads Workset Names from Revit Document
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <returns name="Names">Revit Workset names</returns>
        /// <search>
        /// Query, BHoM, WorksetNames, Document
        /// </search>
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
    }
}
