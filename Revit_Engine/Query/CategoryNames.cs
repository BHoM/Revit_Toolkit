using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<string> CategoryNames(this FamilyLibrary familyLibrary, string familyName, string typeName = null)
        {
            if (familyLibrary == null)
                return null;

            List<string> aCategoryNameList = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> aKeyValuePair_Category in familyLibrary.Dictionary)
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Type = aKeyValuePair_Category.Value;

                List<Dictionary<string, string>> aDictionaryList_Family = new List<Dictionary<string, string>>();
                if(string.IsNullOrEmpty(typeName))
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> aKeyValuePair_Type in aDictionary_Type)
                        aDictionaryList_Family.Add(aKeyValuePair_Type.Value);
                }
                else if(aKeyValuePair_Category.Value.ContainsKey(typeName))
                {
                    aDictionaryList_Family.Add(aDictionary_Type[typeName]);
                }

                if(string.IsNullOrEmpty(familyName))
                {
                    if (aDictionaryList_Family.Count > 0)
                        aCategoryNameList.Add(aKeyValuePair_Category.Key);
                }
                else
                {
                    foreach(Dictionary<string, string> aDictionary_Family in aDictionaryList_Family)
                        if(aDictionary_Family.ContainsKey(familyName))
                        {
                            aCategoryNameList.Add(aKeyValuePair_Category.Key);
                            break;
                        }
                }
            }

            return aCategoryNameList;
        }

        /***************************************************/
    }
}

