/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Facade.Elements;
using BH.oM.Facade.SectionProperties;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Facade.Elements.Opening.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("opening", "BH.oM.Facade.Elements.Opening resulting from converting the input Revit FamilyInstance.")]
        public static oM.Facade.Elements.Opening FacadeOpeningFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return familyInstance.FacadeOpeningFromRevit(null, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Facade.Elements.Opening.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("host", "Revit Element hosting the FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("opening", "BH.oM.Facade.Elements.Opening resulting from converting the input Revit FamilyInstance.")]
        public static oM.Facade.Elements.Opening FacadeOpeningFromRevit(this FamilyInstance familyInstance, HostObject host = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null)
                return null;

            settings = settings.DefaultIfNull();

            string refId = familyInstance.Id.ReferenceIdentifier(host);
            oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
            if (opening != null)
                return opening;

            // Extraction of frame edge property from Revit FamilyInstance is not implemented yet
            BH.Engine.Base.Compute.RecordWarning($"Extraction of frame edge property from a Revit opening is currently not supported, property set to null. ElementId: {familyInstance.Id.IntegerValue}");
            FrameEdgeProperty frameEdgeProperty = null;
            
            BH.oM.Geometry.ISurface location = familyInstance.OpeningSurface(host, settings);

            List<FrameEdge> edges = new List<FrameEdge>();
            if (location == null)
            {
                if (host == null)
                {
                    BH.Engine.Base.Compute.RecordWarning(String.Format("Location of the opening could not be retrieved from the model (possibly it has zero area or lies on a non-planar face). An opening object without location has been returned. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                }
                else
                {
                    BH.Engine.Base.Compute.RecordError(String.Format("Location of the opening could not be retrieved from the model (possibly it has zero area or lies on a non-planar face), the opening has been skipped. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                    return null;
                }
            }
            else
            {
                List<ICurve> extCrvs = location.IExternalEdges().SelectMany(x => x.ISubParts()).ToList();
                edges = extCrvs.Select( x => new FrameEdge { Curve = x, FrameEdgeProperty = frameEdgeProperty }).ToList();
            }

            int category = familyInstance.Category.Id.IntegerValue;
            oM.Physical.Constructions.Construction glazingConstruction = null;
            if (category == (int)BuiltInCategory.OST_Walls)
            {
                HostObjAttributes hostObjAttributes = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as HostObjAttributes;
                string materialGrade = familyInstance.MaterialGrade(settings);
                glazingConstruction = hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            }
            else
            {
                glazingConstruction = familyInstance.GlazingConstruction();
            }

            opening = new BH.oM.Facade.Elements.Opening { Name = familyInstance.FamilyTypeFullName(), Edges = edges, OpeningConstruction = glazingConstruction };

            if (category == (int)BuiltInCategory.OST_Windows || category == (int)BuiltInCategory.OST_CurtainWallPanels)
                opening.Type = OpeningType.Window;
            else if (category == (int)BuiltInCategory.OST_Doors)
                opening.Type = OpeningType.Door;
            else if (category == (int)BuiltInCategory.OST_Walls)
                opening.Type = OpeningType.Undefined;

            //Set identifiers, parameters & custom data
            opening.SetIdentifiers(familyInstance);
            opening.CopyParameters(familyInstance, settings.MappingSettings);
            opening.SetProperties(familyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(refId, opening);
            return opening;
        }

        /***************************************************/
    }
}


