using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Grid ToRevitGrid(this oM.Architecture.Elements.Grid grid, Document document, PushSettings pushSettings = null)
        {
            Grid aGrid = pushSettings.FindRefObject<Grid>(document, grid.BHoM_Guid);
            if (aGrid != null)
                return aGrid;

            pushSettings.DefaultIfNull();

            Curve aCurve = Convert.ToRevitCurve(grid.Curve, pushSettings);

            if (aCurve is Line)
                aGrid = Grid.Create(document, (Line)aCurve);
            if (aCurve is Arc)
                aGrid = Grid.Create(document, (Arc)aCurve);

            aGrid.CheckIfNullPush(grid);
            if (aGrid == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aGrid, grid, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(grid, aGrid);

            return aGrid;
        }

        /***************************************************/
    }
}