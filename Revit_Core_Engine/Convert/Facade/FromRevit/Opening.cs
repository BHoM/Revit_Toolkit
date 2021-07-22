/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.Engine.Facade;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Physical.Constructions;
using BH.oM.Facade.Elements;
using BH.oM.Facade.SectionProperties;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Facade.Elements.Opening FacadeOpeningFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return familyInstance.FacadeOpeningFromRevit(null, settings, refObjects);
        }

        public static oM.Facade.Elements.Opening FacadeOpeningFromRevit(this FamilyInstance familyInstance, HostObject host = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null)
                return null;

            settings = settings.DefaultIfNull();

            string refId = familyInstance.Id.ReferenceIdentifier(host);
            oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
            if (opening != null)
                return opening;

            FrameEdgeProperty frameEdgeProperty = familyInstance.FrameEdgeProperty(settings, refObjects);
            BH.oM.Geometry.ISurface location = familyInstance.OpeningSurface(host, settings);

            List<FrameEdge> edges = new List<FrameEdge>();
            if (location == null)
            {
                if (host == null)
                {
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("Location of the opening could not be retrieved from the model (possibly it has zero area or lies on a non-planar face). An opening object without location has been returned. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                }
                else
                {
                    BH.Engine.Reflection.Compute.RecordError(String.Format("Location of the opening could not be retrieved from the model (possibly it has zero area or lies on a non-planar face), the opening has been skipped. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
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
            opening.CopyParameters(familyInstance, settings.ParameterSettings);
            opening.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(refId, opening);
            return opening;
        }

        /***************************************************/

    }
}

