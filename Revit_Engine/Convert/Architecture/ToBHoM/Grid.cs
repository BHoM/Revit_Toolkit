using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static BHoMObject ToBHoMGrid(this Grid grid, bool copyCustomData = true, bool convertUnits = true)
        {
            Curve gridLine = grid.Curve;
            oM.Architecture.Elements.Grid aGrid = Architecture.Elements.Create.Grid(gridLine.ToBHoM(convertUnits));
            aGrid.Name = grid.Name;
            return aGrid;
        }

        /***************************************************/
    }
}
