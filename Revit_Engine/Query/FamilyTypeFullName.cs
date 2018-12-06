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

        [Description("Gets Full Revit Family Type name for given Family and Type Name. Example: [FamilyName] : [FamilyTypeName]")]
        [Input("familyName", "Family Name")]
        [Input("familyTypeName", "Family Type Name")]
        [Output("FamilyTypeFullName")]
        public static string FamilyTypeFullName(string familyName, string familyTypeName)
        {
            if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(familyTypeName))
                return null;

            return string.Format("{0} : {1}", familyName, familyTypeName);
        }

        /***************************************************/
    }
}
