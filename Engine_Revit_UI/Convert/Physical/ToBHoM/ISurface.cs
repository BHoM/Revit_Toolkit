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
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Physical.Elements.ISurface> ToBHoMISurfaces(this HostObject hostObject, PullSettings pullSettings = null)
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

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> aDictionary = Query.PlanarSurfaceDictionary(hostObject, true, pullSettings);
            if (aDictionary == null)
                return null;

            aISurfaceList = new List<oM.Physical.Elements.ISurface>();

            foreach (KeyValuePair<PlanarSurface, List<oM.Physical.Elements.IOpening>> aKeyValuePair in aDictionary)
            {
                PlanarSurface aPlanarSurface = aKeyValuePair.Key;

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
                    aISurface = BH.Engine.Physical.Create.Floor(aPlanarSurface, aConstruction);
                else if (hostObject is RoofBase)
                    aISurface = BH.Engine.Physical.Create.Roof(aConstruction, aPlanarSurface);                  

                if (aISurface == null)
                    continue;

                if (aKeyValuePair.Value != null)
                    aISurface.Openings = aKeyValuePair.Value;
         
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