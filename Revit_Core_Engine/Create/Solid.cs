using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Create solid object from filled region limited by bottom and top elevation.")]
        [Input("filledRegion", "Filled region that is extruded to the solid.")]
        [Input("bottomElevation", "Bottom elevation of the solid.")]
        [Input("topElevation", "Top elevation of the solid.")]
        [Output("solid", "Solid representation of extruded filled region.")]
        public static Solid Solid(this FilledRegion filledRegion, double bottomElevation, double topElevation)
        {
            if (filledRegion == null)
            {
                BH.Engine.Reflection.Compute.RecordError($"Filled region cannot be null.");
                return null;
            }
            else if (topElevation - bottomElevation < Tolerance.Distance)
            {
                BH.Engine.Reflection.Compute.RecordError($"Top elevation value must be greater than bottom elevation.");
                return null;
            }

            Document doc = filledRegion.Document;
            IList<CurveLoop> boundaries = filledRegion.GetBoundaries();

            return boundaries.Solid(bottomElevation, topElevation);
        }

        /***************************************************/

        [Description("Create solid object from boundaries limited by bottom and top elevation.")]
        [Input("boundaries", "Boundaries list that is extruded to the solid.")]
        [Input("bottomElevation", "Bottom elevation of the solid.")]
        [Input("topElevation", "Top elevation of the solid.")]
        [Output("solid", "Solid representation of extruded boundaries.")]
        public static Solid Solid(this IList<CurveLoop> boundaries, double bottomElevation, double topElevation)
        {
            if (boundaries == null || boundaries.Count == 0)
            {
                BH.Engine.Reflection.Compute.RecordError($"Boundaries cannot be null or empty.");
                return null;
            }
            else if (topElevation - bottomElevation < Tolerance.Distance)
            {
                BH.Engine.Reflection.Compute.RecordError($"Top elevation value must be greater than bottom elevation.");
                return null;
            }

            double elev = boundaries[0].GetPlane().Origin.Z;
            XYZ dir = XYZ.BasisZ;
            double height = topElevation - bottomElevation;
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(boundaries, dir, height);

            if (Math.Abs(elev - bottomElevation) > Tolerance.Distance)
            {
                XYZ translation = new XYZ(0, 0, bottomElevation - elev);
                Transform transform = Transform.CreateTranslation(translation);
                Solid solid2 = SolidUtils.CreateTransformed(solid, transform);
                solid = solid2;
            }

            return solid;
        }

        /***************************************************/
    }
}