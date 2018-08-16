using Autodesk.Revit.DB;

using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<BuiltInCategory> Append(this IEnumerable<BuiltInCategory> builtInCategories_ToBeAppended, IEnumerable<BuiltInCategory>  builtInCategories, bool AllowDuplicates = false)
        {
            if (builtInCategories_ToBeAppended == null && builtInCategories == null)
                return null;

            if (builtInCategories_ToBeAppended == null)
                return new List<BuiltInCategory>(builtInCategories);

            if(builtInCategories == null)
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
    }
}