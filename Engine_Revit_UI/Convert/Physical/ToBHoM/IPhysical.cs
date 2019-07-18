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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Physical.IPhysical> ToBHoMIPhysicals(this HostObject hostObject, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Physical.IPhysical> aPhysicalList = pullSettings.FindRefObjects<oM.Physical.IPhysical>(hostObject.Id.IntegerValue);
            if (aPhysicalList != null && aPhysicalList.Count > 0)
                return aPhysicalList;

            oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes, pullSettings);

            List<PolyCurve> aPolyCurveList = Query.Profiles(hostObject, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            aPhysicalList = new List<oM.Physical.IPhysical>();

            foreach(PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                List<PolyCurve> aPolyCurveList_Inner = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(aPolyCurve, aPolyCurveList);

                oM.Physical.IPhysical aPhysical = null;

                if (hostObject is Wall)
                    aPhysical = BH.Engine.Physical.Create.Wall(BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)), aConstruction);
                else if(hostObject is Floor)
                    aPhysical = BH.Engine.Physical.Create.Floor(BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)), aConstruction);
                else if (hostObject is RoofBase)
                    aPhysical = BH.Engine.Physical.Create.Roof(BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)), aConstruction);

                if (aPhysical == null)
                    continue;

                aPhysical.Name = Query.FamilyTypeFullName(hostObject);

                aPhysical = Modify.SetIdentifiers(aPhysical, hostObject) as oM.Physical.IPhysical;
                if (pullSettings.CopyCustomData)
                    aPhysical = Modify.SetCustomData(aPhysical, hostObject, pullSettings.ConvertUnits) as oM.Physical.IPhysical;

                aPhysical = aPhysical.UpdateValues(pullSettings, hostObject) as oM.Physical.IPhysical;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPhysical);

                aPhysicalList.Add(aPhysical);
            }

            return aPhysicalList;
        }

        /***************************************************/
    }
}