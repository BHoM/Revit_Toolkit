using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns all Paths in FamilyLibrary of given Category, Family and Family Type")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Output("Paths")]
        public static List<string> Paths(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null, string typeName = null)
        {
            if (familyLibrary == null || familyLibrary.Dictionary == null || familyLibrary.Dictionary.Keys.Count == 0)
                return null;

            List<string> aPathList = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> aDictionaryList_Category = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(categoryName))
                aDictionaryList_Category = familyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Category = null;
                if (familyLibrary.Dictionary.TryGetValue(categoryName, out aDictionary_Category))
                    aDictionaryList_Category.Add(aDictionary_Category);
            }

            List<Dictionary<string, string>> aDictionaryList_Type = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(typeName))
            {
                foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
                    aDictionaryList_Type.AddRange(aDictionary_Category.Values);
            }
            else
            {
                foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
                {
                    Dictionary<string, string> aDictionary_Family = null;
                    if (aDictionary_Category.TryGetValue(typeName, out aDictionary_Family))
                        aDictionaryList_Type.Add(aDictionary_Family);
                }
            }

            if (string.IsNullOrEmpty(familyName))
            {
                aDictionaryList_Type.ForEach(x => aPathList.AddRange(x.Values));
            }
            else
            {
                foreach (Dictionary<string, string> aDictionary_Family in aDictionaryList_Type)
                {
                    string aPath = null;
                    if (aDictionary_Family.TryGetValue(familyName, out aPath))
                        aPathList.Add(aPath);
                }
            }

            return aPathList;
        }

        /***************************************************/
    }
}
