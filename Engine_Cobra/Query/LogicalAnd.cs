using System.Linq;
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

        public static Dictionary<FilterQuery, List<Element>> LogicalAnd(this Dictionary<FilterQuery, List<Element>> filterQueryDictionary)
        {
            if (filterQueryDictionary == null)
                return null;

            IEnumerable<ElementId> aElementIdList = Query.ElementIds(filterQueryDictionary);
            if (aElementIdList == null || aElementIdList.Count() == 0)
                return null;

            Dictionary<FilterQuery, List<Element>> aResult = new Dictionary<FilterQuery, List<Element>>();
            foreach (ElementId aElementId in aElementIdList)
            {
                Element aElement = null;
                foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
                {
                    aElement = aKeyValuePair.Value.Find(x => x.Id == aElementId);
                    if (aElement == null)
                        break;
                }

                if(aElement != null)
                {
                    foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
                    {
                        List<Element> aElementList = null;
                        if(!aResult.TryGetValue(aKeyValuePair.Key, out aElementList))
                        {
                            aElementList = new List<Element>();
                            aResult.Add(aKeyValuePair.Key, aElementList);
                        }
                        aElementList.Add(aElement);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/

    }
}