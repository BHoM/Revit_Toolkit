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
using BH.oM.Structure.SurfaceProperties;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (wall as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }
        
        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (floor as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }
        
        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this RoofBase roofBase, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (roofBase as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this HostObject hostObject, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Structure.Elements.Panel> result = refObjects.GetValues<oM.Structure.Elements.Panel>(hostObject.Id);
            if (result != null && result.Count > 0)
                return result;

            //TODO: check if the attributes != null
            HostObjAttributes hostObjAttributes = hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
            string materialGrade = hostObject.MaterialGrade(settings);
            ISurfaceProperty property2D = hostObjAttributes.SurfacePropertyFromRevit(materialGrade, settings, refObjects);

            List<oM.Geometry.ICurve> outlines = hostObject.Outlines(settings);
            if (outlines != null && outlines.Count != 0)
            {
                hostObject.AnalyticalPullWarning();
                result = BHS.Create.Panel(outlines, property2D, hostObject.Name);
            }
            else
            {
                result = new List<oM.Structure.Elements.Panel>();
                Dictionary<BH.oM.Geometry.PlanarSurface, List<BH.oM.Physical.Elements.IOpening>> dictionary = hostObject.PlanarSurfaceDictionary(true, settings);
                if (dictionary != null)
                {
                    foreach (BH.oM.Geometry.PlanarSurface planarSurface in dictionary.Keys)
                    {
                        List<BH.oM.Geometry.ICurve> internalBoundaries = new List<oM.Geometry.ICurve>(planarSurface.InternalBoundaries);

                        if (dictionary[planarSurface] != null)
                        {
                            foreach (BH.oM.Physical.Elements.IOpening opening in dictionary[planarSurface])
                            {
                                BH.oM.Geometry.PlanarSurface openingSurface = opening.Location as BH.oM.Geometry.PlanarSurface;
                                if (openingSurface != null)
                                    internalBoundaries.Add(openingSurface.ExternalBoundary);
                                else
                                    BH.Engine.Reflection.Compute.RecordWarning("A nonplanar opening has been ignored. ElementId: " + Query.ElementId(opening));
                            }
                        }

                        result.Add(BHS.Create.Panel(planarSurface.ExternalBoundary, internalBoundaries, property2D, hostObject.Name));
                    }
                }
            }

            for (int i = 0; i < result.Count; i++)
            {
                oM.Structure.Elements.Panel panel = result[i] as oM.Structure.Elements.Panel;
                panel.Property = property2D;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(hostObject);
                panel.SetCustomData(hostObject, settings.ParameterSettings);
                panel.SetProperties(hostObject, settings.ParameterSettings);

                refObjects.AddOrReplace(hostObject.Id, panel);
                result[i] = panel;
            }

            return result;
        }

        /***************************************************/
    }
}
