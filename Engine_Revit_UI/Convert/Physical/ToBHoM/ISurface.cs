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

using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Physical.Elements.ISurface> ToBHoMISurfaces(this HostObject hostObject, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Physical.Elements.ISurface> aISurfaceList = pullSettings.FindRefObjects<oM.Physical.Elements.ISurface>(hostObject.Id.IntegerValue);
            if (aISurfaceList != null && aISurfaceList.Count > 0)
                return aISurfaceList;

            //TODO: check if the attributes != null
            HostObjAttributes hostObjAttributes = hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(hostObjAttributes, pullSettings);
            string materialGrade = hostObject.MaterialGrade();
            aConstruction = aConstruction.UpdateMaterialProperties(hostObjAttributes, materialGrade, pullSettings);

            IEnumerable<PlanarSurface> aPlanarSurfaces = Query.PlanarSurfaces(hostObject, pullSettings);
            if (aPlanarSurfaces == null)
                return null;

            aISurfaceList = new List<oM.Physical.Elements.ISurface>();

            foreach (PlanarSurface aPlanarSurface in aPlanarSurfaces)
            {
                oM.Physical.Elements.ISurface aISurface = null;

                if (hostObject is Wall)
                {
                    aISurface = BH.Engine.Physical.Create.Wall(aPlanarSurface, aConstruction);

                    Wall aWall = (Wall)hostObject;
                    CurtainGrid aCurtainGrid = aWall.CurtainGrid;
                    if (aCurtainGrid != null)
                    {
                        foreach (ElementId aElementId in aCurtainGrid.GetPanelIds())
                        {
                            Panel aPanel = aWall.Document.GetElement(aElementId) as Panel;
                            if (aPanel == null)
                                continue;
                        }
                    }

                }
                else if (hostObject is Floor)
                {
                    aISurface = BH.Engine.Physical.Create.Floor(aPlanarSurface, aConstruction);
                } 
                else if (hostObject is RoofBase)
                {
                    aISurface = BH.Engine.Physical.Create.Roof(aPlanarSurface, aConstruction);
                }
                    

                if (aISurface == null)
                    continue;

                if(!BH.Engine.Geometry.Query.IIsPlanar(aPlanarSurface.ExternalBoundary))
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Invalid Geometry of ISurface. External Boundary of ISurface is not planar and Openings cannot be pulled.");
                }
                else
                {
                    IEnumerable<ElementId> aElementIds_Hosted = hostObject.FindInserts(false, false, false, false);
                    if (aElementIds_Hosted != null && aElementIds_Hosted.Count() > 0)
                    {
                        List<oM.Physical.Elements.IOpening> aOpeningList = new List<oM.Physical.Elements.IOpening>();
                        foreach (ElementId aElementId in aElementIds_Hosted)
                        {
                            FamilyInstance aFamilyInstance = hostObject.Document.GetElement(aElementId) as FamilyInstance;
                            if (aFamilyInstance == null)
                                continue;

                            if (aFamilyInstance.Category == null)
                                continue;

                            switch ((BuiltInCategory)(aFamilyInstance.Category.Id.IntegerValue))
                            {
                                case BuiltInCategory.OST_Windows:
                                    aOpeningList.Add(aFamilyInstance.ToBHoMWindow(pullSettings));
                                    break;
                                case BuiltInCategory.OST_Doors:
                                    aOpeningList.Add(aFamilyInstance.ToBHoMDoor(pullSettings));
                                    break;
                            }
                        }

                        if (aOpeningList != null && aOpeningList.Count > 0)
                        {
                            aOpeningList.RemoveAll(x => x == null);
                            aISurface.Openings = aOpeningList;
                        }
                    }
                }

                aISurface.Name = Query.FamilyTypeFullName(hostObject);

                ElementType aElementType = hostObject.Document.GetElement(hostObject.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = hostObject.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(hostObject);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, hostObject) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
                aISurface.Fragments.Add(aOriginContextFragment);

                aISurface = Modify.SetIdentifiers(aISurface, hostObject) as oM.Physical.Elements.ISurface;
                if (pullSettings.CopyCustomData)
                    aISurface = Modify.SetCustomData(aISurface, hostObject, pullSettings.ConvertUnits) as oM.Physical.Elements.ISurface;

                aISurface = aISurface.UpdateValues(pullSettings, hostObject) as oM.Physical.Elements.ISurface;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aISurface);

                aISurfaceList.Add(aISurface);
            }

            return aISurfaceList;
        }

        /***************************************************/
    }
}