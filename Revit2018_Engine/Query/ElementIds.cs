
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
        /// Gets ElementId list from UniqueIds collection
        /// </summary>
        /// <param name="document">Revit Document</param>
        /// <param name="uniqueIds">Unique Ids collection</param>
        /// <param name="removeNulls">Remove nulls or empty values from result</param>
        /// <returns name="ElementIds">ElementId List</returns>
        /// <search>
        /// Utilis, GetElementIdList, Revit, Get ElementId List, Parameter, ElementIds
        /// </search>
        public static List<ElementId> ElementIds(this Document document, IEnumerable<string> uniqueIds, bool removeNulls)
        {
            if (document == null || uniqueIds == null)
                return null;


            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (string aUniqueId in uniqueIds)
            {
                if (string.IsNullOrEmpty(aUniqueId))
                {
                    Element aElement = document.GetElement(aUniqueId);
                    if (aElement != null)
                    {
                        aElementIdList.Add(aElement.Id);
                        continue;
                    }
                }

                if (!removeNulls)
                    aElementIdList.Add(null);
            }

            return aElementIdList;
        }

        /***************************************************/
    }
}

