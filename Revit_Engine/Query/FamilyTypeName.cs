using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Revit Family Type name for given FilterQuery (Example: FamilyFilterQuery).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("FamilyTypeName")]
        public static string FamilyTypeName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.FamilyTypeName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.FamilyTypeName] as string;
        }

        /***************************************************/

        [Description("Gets Revit Family Type name (stored in CustomData) for given BHoMObject.")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("FamilyTypeName")]
        public static string FamilyTypeName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.FamilyTypeName, out aValue))
            {
                if (aValue == null)
                    return null;

                return aValue.ToString();
            }

            return null;
        }

        /***************************************************/

        [Description("Gets Revit Family Type name from Family Type Full Name in format [Family Name] : [Family Type Name].")]
        [Input("familyTypeFullName", "BHoMObject")]
        [Output("FamilyTypeName")]
        public static string FamilyTypeName(this string familyTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(familyTypeFullName))
                return null;

            int aIndex = familyTypeFullName.IndexOf(":");
            if (aIndex <= 0)
                return null;

            string aResult = familyTypeFullName.Substring(aIndex+1);
            return aResult.Trim();
        }

        /***************************************************/
    }
}
