/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Facade.Elements;
using BH.oM.Facade.SectionProperties;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Facade.Elements.FrameEdge.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("frameEdge", "BH.oM.Facade.Elements.FrameEdge resulting from converting the input Revit FamilyInstance.")]
        public static FrameEdge FrameEdgeFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            Mullion mullion = familyInstance as Mullion;            
            if (mullion == null)
                return null;

            settings = settings.DefaultIfNull();

            FrameEdge bHoMFrameEdge = refObjects.GetValue<FrameEdge>(mullion.Id);
            if (bHoMFrameEdge != null)
                return bHoMFrameEdge;

            BH.oM.Geometry.ICurve location = mullion.LocationCurve?.IFromRevit();
            if (location == null)
            {
                BH.Engine.Base.Compute.RecordWarning(String.Format("Location of the frame edge could not be retrieved from the model. A frame edge without location has been returned. Revit ElementId: {0}", mullion.Id.IntegerValue));
            }

            FrameEdgeProperty prop = mullion.MullionElementProperty(settings, refObjects);

            bHoMFrameEdge = new FrameEdge { Curve = location, FrameEdgeProperty = prop, Name = mullion.FamilyTypeFullName() };

            //Set identifiers, parameters & custom data
            bHoMFrameEdge.SetIdentifiers(mullion);
            bHoMFrameEdge.CopyParameters(mullion, settings.MappingSettings);
            bHoMFrameEdge.SetProperties(mullion, settings.MappingSettings);

            refObjects.AddOrReplace(mullion.Id, bHoMFrameEdge);
            return bHoMFrameEdge;
        }

        /***************************************************/
    }
}





