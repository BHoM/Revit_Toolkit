using System.Collections.Generic;
using System.IO;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Viewport Viewport(string sheetNumber, string viewName, Point location)
        {
            Viewport aViewport = new Viewport()
            {
                Location = location
            };

            aViewport.CustomData.Add("Sheet Number", sheetNumber);
            aViewport.CustomData.Add("View Name", viewName);

            aViewport.CustomData.Add(Convert.FamilyName, "Viewport");
            aViewport.CustomData.Add(Convert.CategoryName, "Viewports");

            return aViewport;
        }

        /***************************************************/
    }
}

