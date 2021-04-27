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
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Facade.Elements.FrameEdge FrameEdgeFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            Mullion mullion = familyInstance as Mullion;            
            if (mullion == null)
                return null;

            settings = settings.DefaultIfNull();

            FrameEdge bHoMFrameEdge = refObjects.GetValue<FrameEdge>(mullion.Id);
            if (bHoMFrameEdge != null)
                return bHoMFrameEdge;

            BH.oM.Geometry.ICurve location = mullion.LocationCurve.IFromRevit();
            if (location == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Location of the frame edge could not be retrieved from the model. A frame edge without location has been returned. Revit ElementId: {0}", mullion.Id.IntegerValue));
            }

            FrameEdgeProperty prop = new FrameEdgeProperty { Name = mullion.FamilyTypeFullName() };

            bHoMFrameEdge = new FrameEdge { Curve = location, FrameEdgeProperty = prop, Name = mullion.FamilyTypeFullName() };

            //Set identifiers, parameters & custom data
            bHoMFrameEdge.SetIdentifiers(mullion);
            bHoMFrameEdge.CopyParameters(mullion, settings.ParameterSettings);
            bHoMFrameEdge.SetProperties(mullion, settings.ParameterSettings);

            refObjects.AddOrReplace(mullion.Id, bHoMFrameEdge);
            return bHoMFrameEdge;
        }

        /***************************************************/
    }
}

