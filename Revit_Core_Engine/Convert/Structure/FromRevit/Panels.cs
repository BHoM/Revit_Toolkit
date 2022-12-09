/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Structure.SurfaceProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BHS = BH.Engine.Structure;
using BH.Revit.Engine.Core.Objects;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Wall to a collection of BH.oM.Structure.Elements.Panels.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Collection of BH.oM.Structure.Elements.Panels resulting from converting the input Revit Wall.")]
        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (wall as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit Floor to a collection of BH.oM.Structure.Elements.Panels.")]
        [Input("floor", "Revit Floor to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Collection of BH.oM.Structure.Elements.Panels resulting from converting the input Revit Floor.")]
        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (floor as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit RoofBase to a collection of BH.oM.Structure.Elements.Panels.")]
        [Input("roofBase", "Revit RoofBase to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Collection of BH.oM.Structure.Elements.Panels resulting from converting the input Revit RoofBase.")]
        public static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this RoofBase roofBase, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return (roofBase as HostObject).StructuralPanelsFromRevit(settings, refObjects);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Converts a Revit HostObject to a collection of BH.oM.Structure.Elements.Panels.")]
        [Input("hostObject", "Revit HostObject to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Collection of BH.oM.Structure.Elements.Panels resulting from converting the input Revit HostObject.")]
        private static List<oM.Structure.Elements.Panel> StructuralPanelsFromRevit(this HostObject hostObject, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Structure.Elements.Panel> result = refObjects.GetValues<oM.Structure.Elements.Panel>(hostObject.Id);
            if (result != null && result.Count > 0)
                return result;
            
            HostObjAttributes hostObjAttributes = hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
            string materialGrade = hostObject.MaterialGrade(settings);
            ISurfaceProperty property2D = hostObjAttributes?.SurfacePropertyFromRevit(materialGrade, settings, refObjects);

            if (property2D == null)
                BH.Engine.Base.Compute.RecordError(String.Format("Conversion of Revit panel's construction to BHoM ISurfaceProperty failed. A panel without property is returned. Revit ElementId : {0}", hostObjAttributes.Id));

            List<ICurve> outlines;
            OutlineCache outlineCache = refObjects.GetValue<OutlineCache>(hostObject.Id.SurfaceCacheKey());
            if (outlineCache != null)
                outlines = outlineCache.Outlines;
            else
                outlines = hostObject.AnalyticalOutlines(settings);

            if (outlines != null && outlines.Count != 0)
            {
                hostObject.AnalyticalPullWarning();
                result = BHS.Create.Panel(outlines, property2D, null, hostObject.Name);
            }
            else
            {
                Dictionary<PlanarSurface, List<PlanarSurface>> surfaces;
                SurfaceCache surfaceCache = refObjects.GetValue<SurfaceCache>(hostObject.Id.SurfaceCacheKey());
                if (surfaceCache != null)
                    surfaces = surfaceCache.Surfaces;
                else
                    surfaces = hostObject.PanelSurfaces(null, settings);

                result = new List<oM.Structure.Elements.Panel>();
                if (surfaces != null)
                {

                    Vector translation = new Vector();
                    if (property2D is ConstantThickness && (hostObject is Floor || hostObject is RoofBase))
                        translation = -((ConstantThickness)property2D).Thickness * 0.5 * Vector.ZAxis;

                    foreach (PlanarSurface planarSurface in surfaces.Keys)
                    {
                        if (planarSurface.ExternalBoundary == null)
                            continue;

                        List<ICurve> internalBoundaries = new List<ICurve>();
                        if (surfaces[planarSurface] != null)
                            internalBoundaries.AddRange(surfaces[planarSurface].Select(x => x.ExternalBoundary.ITranslate(translation)));

                        result.Add(BHS.Create.Panel(planarSurface.ExternalBoundary.ITranslate(translation), internalBoundaries, property2D, null, hostObject.Name));
                    }
                }
            }

            if (result.Count == 0)
            {
                result.Add(new oM.Structure.Elements.Panel { Name = hostObject.Name, Property = property2D });
                BH.Engine.Base.Compute.RecordError(String.Format("Conversion of Revit panel's location to BHoM failed. A panel without location is returned. Revit ElementId : {0}", hostObject.Id));
            }

            //Set identifiers, parameters & custom data
            foreach (oM.Structure.Elements.Panel panel in result)
            {
                panel.SetIdentifiers(hostObject);
                panel.CopyParameters(hostObject, settings.MappingSettings);
                panel.SetProperties(hostObject, settings.MappingSettings);
            }
            
            refObjects.AddOrReplace(hostObject.Id, result);
            return result;
        }

        /***************************************************/
    }
}


