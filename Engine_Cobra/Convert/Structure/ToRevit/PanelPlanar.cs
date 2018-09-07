using System;

using Autodesk.Revit.DB;

using BH.Engine.Geometry;
using BH.Engine.Structure;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;


namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static HostObject ToRevit(this PanelPlanar panelPlanar, Document document, PushSettings pushSettings = null)
        {
            if (panelPlanar == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            //TODO: the solution below should be replaced by a Property of a PanelPlanar that would define if it is a floor or wall.
            double dotProduct = Math.Abs(panelPlanar.Outline().FitPlane().Normal.DotProduct(Vector.ZAxis));

            if (dotProduct <= Tolerance.Angle)
                return panelPlanar.ToRevitWall(document, pushSettings);
            else if (1 - dotProduct <= Tolerance.Angle)
                return panelPlanar.ToRevitFloor(document, pushSettings);
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("The current implementation of BHoM push to Revit works only on horizontal slabs and vertical walls. The Revit element has not been created. BHoM_Guid: {0}", panelPlanar.BHoM_Guid));
                return null;
            }
        }

        /***************************************************/
    }
}
