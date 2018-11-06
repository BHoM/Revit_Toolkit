using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Sheet object by given name and number.")]
        [Input("name", "Sheet Name")]
        [Input("number", "Sheet Number")]
        [Output("Sheet")]
        public static Sheet Sheet(string name, string number)
        {
            Sheet aSheet = new Sheet()
            {
                Name = name
            };

            aSheet.CustomData.Add("Sheet Name", name);
            aSheet.CustomData.Add("Sheet Number", number);

            aSheet.CustomData.Add(Convert.FamilyName, "Sheet");
            aSheet.CustomData.Add(Convert.FamilyTypeName, "Sheet");
            aSheet.CustomData.Add(Convert.CategoryName, "Sheets");

            return aSheet;
        }

        /***************************************************/
    }
}

