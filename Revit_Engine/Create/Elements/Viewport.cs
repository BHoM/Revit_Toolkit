using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Viewport on specific location point and which is assigned to the view by given name and to the sheet by given sheet Number.")]
        [Input("sheetNumber", "Sheet Number linked with this Viewport")]
        [Input("viewName", "View Name linked with this Viewport")]
        [Input("location", "Location of the view port on sheet")]
        [Output("Viewport")]
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

