/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public List<BuildingElement> HostedBuildingElements(HostObject hostObject, Face face, PullSettings pullSettings = null)
        {
            if (hostObject == null)
                return null;

            IList<ElementId> aElementIdList = hostObject.FindInserts(false, false, false, false);
            if (aElementIdList == null || aElementIdList.Count < 1)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            foreach (ElementId aElementId in aElementIdList)
            {
                Element aElement_Hosted = hostObject.Document.GetElement(aElementId);
                if ((BuiltInCategory)aElement_Hosted.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Windows || (BuiltInCategory)aElement_Hosted.Category.Id.IntegerValue == Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                {
                    IntersectionResult aIntersectionResult = null;

                    BoundingBoxXYZ aBoundingBoxXYZ = aElement_Hosted.get_BoundingBox(null);

                    aIntersectionResult = face.Project(aBoundingBoxXYZ.Max);
                    if (aIntersectionResult == null)
                        continue;

                    XYZ aXYZ_Max = aIntersectionResult.XYZPoint;
                    UV aUV_Max = aIntersectionResult.UVPoint;

                    aIntersectionResult = face.Project(aBoundingBoxXYZ.Min);
                    if (aIntersectionResult == null)
                        continue;

                    XYZ aXYZ_Min = aIntersectionResult.XYZPoint;
                    UV aUV_Min = aIntersectionResult.UVPoint;

                    double aU = aUV_Max.U - aUV_Min.U;
                    double aV = aUV_Max.V - aUV_Min.V;

                    XYZ aXYZ_V = face.Evaluate(new UV(aUV_Max.U, aUV_Max.V - aV));
                    if (aXYZ_V == null)
                        continue;

                    XYZ aXYZ_U = face.Evaluate(new UV(aUV_Max.U - aU, aUV_Max.V));
                    if (aXYZ_U == null)
                        continue;

                    List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
                    aPointList.Add(aXYZ_Max.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_U.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_Min.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_V.ToBHoM(pullSettings));
                    aPointList.Add(aXYZ_Max.ToBHoM(pullSettings));

                    BuildingElement aBuildingElement = Convert.ToBHoMBuildingElement(aElement_Hosted, BH.Engine.Geometry.Create.Polyline(aPointList), pullSettings);
                    if (aBuildingElement != null)
                        aBuildingElmementList.Add(aBuildingElement);

                }
            }
            return aBuildingElmementList;
        }

        /***************************************************/
    }
}