using Autodesk.Revit.DB;
using BH.Engine.Base;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        public static Dictionary<BuiltInCategory, string> CategoriesWithNames()
        {
            if (m_CategoriesWithNames == null)
                CollectCategories();

            return m_CategoriesWithNames;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void CollectCategories()
        {
            m_CategoriesWithNames = new Dictionary<BuiltInCategory, string>();
            foreach (BH.oM.Revit.Enums.Category category in Enum.GetValues(typeof(BH.oM.Revit.Enums.Category)))
            {
                m_CategoriesWithNames.Add((BuiltInCategory)((int)category), category.ToText());
            }
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<BuiltInCategory, string> m_CategoriesWithNames = null;

        /***************************************************/
    }
}
