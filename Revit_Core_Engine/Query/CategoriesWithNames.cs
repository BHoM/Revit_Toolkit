using Autodesk.Revit.DB;
using BH.Engine.Base;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        public static Dictionary<string, BuiltInCategory> CategoriesWithNames()
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
            m_CategoriesWithNames = new Dictionary<string, BuiltInCategory>();
            foreach (BH.oM.Revit.Enums.Category category in Enum.GetValues(typeof(BH.oM.Revit.Enums.Category)))
            {
                m_CategoriesWithNames.Add(category.ToText(), (BuiltInCategory)((int)category));
            }
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<string, BuiltInCategory> m_CategoriesWithNames = null;

        /***************************************************/
    }
}
