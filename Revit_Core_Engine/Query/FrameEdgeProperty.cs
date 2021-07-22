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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Physical.FramingProperties;
using BH.oM.Facade.SectionProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Geometry;
using BH.Engine.Geometry;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FrameEdgeProperty FrameEdgeProperty(this FamilyInstance familyInstance, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null )
                return null;

            // Create FrameEdgeProperties, currently only using default
            BH.Engine.Reflection.Compute.RecordWarning(String.Format("Revit specific FrameEdgeProperty conversion for this element is not currently supported, and a default FrameEdgeProperty has been assigned. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
            List<oM.Physical.FramingProperties.ConstantFramingProperty> frameEdgeSectionProps = new List<oM.Physical.FramingProperties.ConstantFramingProperty>();
            oM.Physical.Materials.Material alumMullion = new oM.Physical.Materials.Material { Name = "Aluminum" };
            BH.oM.Spatial.ShapeProfiles.RectangleProfile rect = BH.Engine.Spatial.Create.RectangleProfile(0.1, 0.2);

            Vector offsetVector = new Vector { X = 0.1 };
            List<ICurve> mullionCrvs = new List<ICurve>();
            foreach (ICurve crv in rect.Edges)
            {
                mullionCrvs.Add(crv.ITranslate(offsetVector));
            }

            oM.Spatial.ShapeProfiles.FreeFormProfile edgeProf = BH.Engine.Spatial.Create.FreeFormProfile(mullionCrvs, false);
            oM.Physical.FramingProperties.ConstantFramingProperty frameEdgeProp = new oM.Physical.FramingProperties.ConstantFramingProperty { Name = "Default Frame Edge Section Prop", Material = alumMullion, Profile = edgeProf };
            frameEdgeSectionProps.Add(frameEdgeProp);
            FrameEdgeProperty defaultEdgeProp = new FrameEdgeProperty { Name = "Default Edge Property", SectionProperties = frameEdgeSectionProps };

            return defaultEdgeProp;
        }

        /***************************************************/
    }
}

