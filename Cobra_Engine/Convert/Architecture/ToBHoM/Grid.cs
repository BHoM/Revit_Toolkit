using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMGrid(this Grid grid, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            Curve gridLine = grid.Curve;
            oM.Architecture.Elements.Grid aGrid = Architecture.Elements.Create.Grid(gridLine.ToBHoM(pullSettings));
            aGrid.Name = grid.Name;
            return aGrid;
        }

        /***************************************************/
    }
}
