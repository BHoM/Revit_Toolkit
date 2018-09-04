using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string CategoryName(this BuiltInCategory builtInCategory, Document document)
        {
            if (document == null || document.Settings == null || document.Settings.Categories == null)
                return null;

            foreach (Category aCategory in document.Settings.Categories)
                if (aCategory.Id.IntegerValue == (int)builtInCategory)
                    return aCategory.Name;

            return null;
        }

        /***************************************************/
    }
}
