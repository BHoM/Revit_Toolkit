using System.ComponentModel;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Revit Family name (stored in CustomData) for given BHoMObject.")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("FamilyName")]
        public static string FamilyName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.FamilyName, out aValue))
            {
                if (aValue == null)
                    return null;

                return aValue.ToString();
            }

            return null;
        }

        /***************************************************/

        [Description("Gets Revit Family name for given FilterQuery (Example: FamilyFilterQuery).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("FamilyName")]
        public static string FamilyName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.FamilyName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.FamilyName] as string;
        }

        /***************************************************/

        [Description("Gets Revit Family name from Family Type Full Name in format [Family Name] : [Family Type Name].")]
        [Input("familyTypeFullName", "Family Type Full Name")]
        [Output("FamilyName")]
        public static string FamilyName(this string familyTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(familyTypeFullName))
                return null;

            int aIndex = familyTypeFullName.IndexOf(":");
            if (aIndex <= 0)
                return null;

            return familyTypeFullName.Substring(0, aIndex).Trim();
        }

        /***************************************************/
    }
}
