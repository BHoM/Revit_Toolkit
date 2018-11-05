using System.Collections.Generic;
using System.IO;

using BH.oM.Adapters.Revit.Elements;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

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

