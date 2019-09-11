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
using System;
using System.Collections.Generic;
using BH.oM.Environment.Fragments;
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

            List<PolyCurve> aPolyCurveList_Outer = null;
            try
            {
                aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            }
            catch(Exception aException)
            {
                aPolyCurveList_Outer = aPolyCurveList;
            }

            if (aPolyCurveList_Outer == null)
                return null;
            
            aPhysicalList = new List<oM.Physical.IPhysical>();

            foreach(PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                List<PolyCurve> aPolyCurveList_Inner = null;
                try
                {
                    aPolyCurveList_Inner = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(aPolyCurve, aPolyCurveList);
                }
                catch(Exception aException)
                {

                }

                List<ICurve> aICurveList = new List<ICurve>();
                if (aPolyCurveList_Inner != null && aPolyCurveList_Inner.Count > 0)
                    aICurveList = aPolyCurveList_Inner.ConvertAll(x => (ICurve)x);

                oM.Physical.IPhysical aPhysical = null;

                //TODO: Create method in Geometry Engine shall be used however IsClosed method returns false for some of the PolyCurves pulled from Revit
                //PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)
                PlanarSurface aPlanarSurface = new PlanarSurface()
                {
                    ExternalBoundary = aPolyCurve,
                    InternalBoundaries = aICurveList
                };

                if (hostObject is Wall)
                {
                    aPhysical = BH.Engine.Physical.Create.Wall(aPlanarSurface, aConstruction);

                    Wall aWall = (Wall)hostObject;
                    CurtainGrid aCurtainGrid = aWall.CurtainGrid;
                    if(aCurtainGrid != null)
                    {
                        foreach(ElementId aElementId in aCurtainGrid.GetPanelIds())
                        {
                            Panel aPanel = aWall.Document.GetElement(aElementId) as Panel;
                            if (aPanel == null)
                                continue;
                        }
                    }
                    
                }   
                else if(hostObject is Floor)
                    aPhysical = BH.Engine.Physical.Create.Floor(aPlanarSurface, aConstruction);
                else if (hostObject is RoofBase)
                    aPhysical = BH.Engine.Physical.Create.Roof(aPlanarSurface, aConstruction);

                if (aPhysical == null)
                    continue;

                aPhysical.Name = Query.FamilyTypeFullName(hostObject);

                ElementType aElementType = hostObject.Document.GetElement(hostObject.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = hostObject.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(hostObject);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, hostObject) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
                aPhysical.Fragments.Add(aOriginContextFragment);

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