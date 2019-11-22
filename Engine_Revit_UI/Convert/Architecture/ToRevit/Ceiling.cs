/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Ceiling ToRevitCeiling(this oM.Architecture.Elements.Ceiling ceiling, Document document, PushSettings pushSettings = null)
        {
            BH.Engine.Reflection.Compute.RecordError("Revit API does not allow creation of Ceiling: This is a known limitation, currently, Revit API doesn't support the new ceiling creation.");
            return null;

            if (ceiling == null || ceiling.Construction == null || document == null)
                return null;

            PlanarSurface aPlanarSurface = ceiling.Surface as PlanarSurface;
            if (aPlanarSurface == null)
                return null;

            Ceiling aCeiling = pushSettings.FindRefObject<Ceiling>(document, ceiling.BHoM_Guid);
            if (aCeiling != null)
                return aCeiling;

            pushSettings.DefaultIfNull();

            CeilingType aCeilingType = null;

            if (ceiling.Construction != null)
                aCeilingType = ToRevitHostObjAttributes(ceiling.Construction, document, pushSettings) as CeilingType;

            if (aCeilingType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(ceiling);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<CeilingType> aCeilingTypeList = new FilteredElementCollector(document).OfClass(typeof(CeilingType)).Cast<CeilingType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aCeilingTypeList != null || aCeilingTypeList.Count() != 0)
                        aCeilingType = aCeilingTypeList.First();
                }
            }

            if (aCeilingType == null)
            {
                string aFamilyTypeName = ceiling.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<CeilingType> aCeilingTypeList = new FilteredElementCollector(document).OfClass(typeof(CeilingType)).Cast<CeilingType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aCeilingTypeList != null || aCeilingTypeList.Count() != 0)
                        aCeilingType = aCeilingTypeList.First();
                }
            }

            if (aCeilingType == null)
                return null;

            double aLowElevation = aPlanarSurface.LowElevation();

            Level aLevel = document.HighLevel(aLowElevation, true);

            double aElevation = aLevel.Elevation;
            if (pushSettings.ConvertUnits)
                aElevation = aElevation.ToSI(UnitType.UT_Length);

            oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aLowElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));
            ICurve aCurve = BH.Engine.Geometry.Modify.Project(aPlanarSurface.ExternalBoundary as dynamic, aPlane) as ICurve;

            CurveArray aCurveArray = null;
            if (aCurve is PolyCurve)
                aCurveArray = ((PolyCurve)aCurve).ToRevitCurveArray(pushSettings);
            else if (aCurve is Polyline)
                aCurveArray = ((Polyline)aCurve).ToRevitCurveArray(pushSettings);

            if (aCurveArray == null)
                return null;

            //aCeiling = Ceiling.Create(aCurveArray, aCeilingType, aLevel, false);

            aCeiling.CheckIfNullPush(ceiling);
            if (aCeiling == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aCeiling, ceiling, new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(ceiling, aCeiling);

            return aCeiling;
        }

        /***************************************************/
    }
}