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

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
            {
                foreach (Element aElement in aKeyValuePair.Value)
                    if (aElementIdList.Find(x => x == aElement.Id) == null)
                        aElementIdList.Add(aElement.Id);
            }

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