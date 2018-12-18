using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static oM.Geometry.Polyline VerticalBounds(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            //TODO: Get more accurate shape. Currently, Windows and doors goes as rectangular panel
            BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

            XYZ aVector = aBoundingBoxXYZ.Max - aBoundingBoxXYZ.Min;

            double aWidth = Math.Abs(aVector.Y);
            double aHeight = Math.Abs(aVector.Z);

            XYZ aVector_Y = (aBoundingBoxXYZ.Transform.BasisY * aWidth) / 2;
            XYZ aVector_Z = (aBoundingBoxXYZ.Transform.BasisZ * aHeight) / 2;

            XYZ aMiddle = (aBoundingBoxXYZ.Max + aBoundingBoxXYZ.Min) / 2;

            XYZ aXYZ_1 = aMiddle + aVector_Z - aVector_Y;
            XYZ aXYZ_2 = aMiddle + aVector_Z + aVector_Y;
            XYZ aXYZ_3 = aMiddle - aVector_Z + aVector_Y;
            XYZ aXYZ_4 = aMiddle - aVector_Z - aVector_Y;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            aPointList.Add(aXYZ_1.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_2.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_3.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_4.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_1.ToBHoM(pullSettings));

            return BH.Engine.Geometry.Create.Polyline(aPointList);
        }

        /***************************************************/
    }
}