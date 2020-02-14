/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Physical.Elements.ISurface> ToBHoMISurfaces(this HostObject hostObject, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Physical.Elements.ISurface> surfaces = refObjects.GetValues<oM.Physical.Elements.ISurface>(hostObject.Id);
            if (surfaces != null && surfaces.Count > 0)
                return surfaces;

            //TODO: check if the attributes != null
            HostObjAttributes hostObjAttributes = hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ToBHoMConstruction(settings, refObjects);
            string materialGrade = hostObject.MaterialGrade();
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> dictionary = hostObject.PlanarSurfaceDictionary(true, settings);
            if (dictionary == null)
                return null;

            surfaces = new List<oM.Physical.Elements.ISurface>();

            foreach (KeyValuePair<PlanarSurface, List<oM.Physical.Elements.IOpening>> kvp in dictionary)
            {
                PlanarSurface planarSurface = kvp.Key;

                oM.Physical.Elements.ISurface iSurface = null;

                if (hostObject is Wall)
                {
                    iSurface = BH.Engine.Physical.Create.Wall(planarSurface, construction);

                    Wall wall = (Wall)hostObject;
                    CurtainGrid curtainGrid = wall.CurtainGrid;
                    if (curtainGrid != null)
                    {
                        foreach (ElementId elementID in curtainGrid.GetPanelIds())
                        {
                            Panel panel = wall.Document.GetElement(elementID) as Panel;
                            if (panel == null)
                                continue;
                        }
                    }

                }
                else if (hostObject is Floor)
                    iSurface = BH.Engine.Physical.Create.Floor(planarSurface, construction);
                else if (hostObject is RoofBase)
                    iSurface = BH.Engine.Physical.Create.Roof(construction, planarSurface);                  

                if (iSurface == null)
                    continue;

                if (kvp.Value != null)
                    iSurface.Openings = kvp.Value;
         
                iSurface.Name = hostObject.FamilyTypeFullName();

                ElementType elementType = hostObject.Document.GetElement(hostObject.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = hostObject.Id.IntegerValue.ToString();
                originContext.TypeName = hostObject.FamilyTypeFullName();
                originContext = originContext.UpdateValues(settings, hostObject) as OriginContextFragment;
                originContext = originContext.UpdateValues(settings, elementType) as OriginContextFragment;
                iSurface.Fragments.Add(originContext);

                //Set identifiers & custom data
                iSurface = iSurface.SetIdentifiers(hostObject) as oM.Physical.Elements.ISurface;
                iSurface = iSurface.SetCustomData(hostObject) as oM.Physical.Elements.ISurface;

                iSurface = iSurface.UpdateValues(settings, hostObject) as oM.Physical.Elements.ISurface;

                refObjects.AddOrReplace(hostObject.Id, iSurface);
                surfaces.Add(iSurface);
            }

            return surfaces;
        }

        /***************************************************/
    }
}
