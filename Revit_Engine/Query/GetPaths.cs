using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<string> GetPaths(this FamilyLibrary FamilyLibrary, string CategoryName = null, string FamilyName = null, string TypeName = null)
        {
            if (FamilyLibrary == null)
                return null;

            List<string> aPathList = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> aDictionaryList_Category = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(CategoryName))
                aDictionaryList_Category = FamilyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Category = null;
                if (FamilyLibrary.Dictionary.TryGetValue(CategoryName, out aDictionary_Category))
                    aDictionaryList_Category.Add(aDictionary_Category);
            }

            List<Dictionary<string, string>> aDictionaryList_Type = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(TypeName))
            {
                foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
                    aDictionaryList_Type.AddRange(aDictionary_Category.Values);
            }
            else
            {
                foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
                {
                    Dictionary<string, string> aDictionary_Family = null;
                    if (aDictionary_Category.TryGetValue(TypeName, out aDictionary_Family))
                        aDictionaryList_Type.Add(aDictionary_Family);
                }
            }

            if (string.IsNullOrEmpty(FamilyName))
            {
                aDictionaryList_Type.ForEach(x => aPathList.AddRange(x.Values));
            }
            else
            {
                foreach (Dictionary<string, string> aDictionary_Family in aDictionaryList_Type)
                {
                    string aPath = null;
                    if (aDictionary_Family.TryGetValue(FamilyName, out aPath))
                        aPathList.Add(aPath);
                }
            }

            return aPathList;
        }

        /***************************************************/
    }
}
