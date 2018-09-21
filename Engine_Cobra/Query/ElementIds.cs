using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.DataManipulation.Queries;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<ElementId> ElementIds(this Document document, IEnumerable<string> uniqueIds, bool removeNulls)
        {
            if (document == null || uniqueIds == null)
                return null;


            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (string aUniqueId in uniqueIds)
            {
                if (!string.IsNullOrEmpty(aUniqueId))
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

        public static IEnumerable<ElementId> ElementIds(this Dictionary<FilterQuery, List<Element>> filterQueryDictionary)
        {
            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
            {
                foreach (Element aElement in aKeyValuePair.Value)
                    if (aElementIdList.Find(x => x == aElement.Id) == null)
                        aElementIdList.Add(aElement.Id);
            }

            return aElementIdList;
        }

        /***************************************************/
    }
}