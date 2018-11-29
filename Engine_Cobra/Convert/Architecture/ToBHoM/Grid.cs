using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMGrid(this Grid grid, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Architecture.Elements.Grid aGrid = pullSettings.FindRefObject<oM.Architecture.Elements.Grid>(grid.Id.IntegerValue);
            if (aGrid != null)
                return aGrid;

            aGrid = BH.Engine.Architecture.Elements.Create.Grid(grid.Curve.ToBHoM(pullSettings));
            aGrid.Name = grid.Name;

            aGrid = Modify.SetIdentifiers(aGrid, grid) as oM.Architecture.Elements.Grid;
            if (pullSettings.CopyCustomData)
                aGrid = Modify.SetCustomData(aGrid, grid, pullSettings.ConvertUnits) as oM.Architecture.Elements.Grid;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aGrid);

            return aGrid;
        }

        /***************************************************/
    }
}