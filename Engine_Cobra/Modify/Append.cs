using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.DataManipulation.Queries;


namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> Append(this IEnumerable<BuiltInCategory> builtInCategories_ToBeAppended, IEnumerable<BuiltInCategory> builtInCategories, bool AllowDuplicates = false)
        {
            if (builtInCategories_ToBeAppended == null && builtInCategories == null)
                return null;

            if (builtInCategories_ToBeAppended == null)
                return new List<BuiltInCategory>(builtInCategories);

            if (builtInCategories == null || builtInCategories.Count() == 0)
                return new List<BuiltInCategory>(builtInCategories_ToBeAppended);

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>(builtInCategories_ToBeAppended);

            foreach (BuiltInCategory aBuiltInCategory in builtInCategories)
            {
                if (AllowDuplicates || !aBuiltInCategoryList.Contains(aBuiltInCategory))
                    aBuiltInCategoryList.Add(aBuiltInCategory);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static Dictionary<FilterQuery, List<Element>> Append(this Dictionary<FilterQuery, List<Element>> filterQueryDictionary_ToBeAppended, Dictionary<FilterQuery, List<Element>> filterQueryDictionary)
        {
            if (filterQueryDictionary_ToBeAppended == null && filterQueryDictionary == null)
                return null;

            if (filterQueryDictionary_ToBeAppended == null)
                return new Dictionary<FilterQuery, List<Element>>(filterQueryDictionary);

            if (filterQueryDictionary == null || filterQueryDictionary.Count == 0)
                return new Dictionary<FilterQuery, List<Element>>(filterQueryDictionary_ToBeAppended);

            Dictionary<FilterQuery, List<Element>> aResult = new Dictionary<FilterQuery, List<Element>>(filterQueryDictionary_ToBeAppended);
            foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
            {
                if (aKeyValuePair.Value == null)
                    continue;

                List<Element> aElementList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aElementList))
                {
                    aResult.Add(aKeyValuePair.Key, aKeyValuePair.Value);
                }
                else
                {
                    foreach (Element aElement in aKeyValuePair.Value)
                        if (aElementList.Find(x => x.Id == aElement.Id) == null)
                            aElementList.Add(aElement);
                }
            }

            return aResult;
        }

    }
}