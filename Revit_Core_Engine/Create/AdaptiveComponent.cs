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
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance AdaptiveComponent(Document document, FamilySymbol familySymbol, List<XYZ> points, RevitSettings settings = null)
        {
            if (document == null || familySymbol == null || points == null)
                return null;

            settings = settings.DefaultIfNull();

            FamilyInstance adaptiveComponent = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, familySymbol);
            IList<ElementId> pointIds = AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(adaptiveComponent);
            if (pointIds.Count != points.Count)
                pointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(adaptiveComponent);

            if (pointIds.Count != points.Count)
            {
                BH.Engine.Reflection.Compute.RecordError($"An adaptive component could not be created based on the given ModelInstance because its definition requires different number of points than provided.");
                document.Delete(adaptiveComponent.Id);
                return null;
            }

            for (int i = 0; i < pointIds.Count; i++)
            {
                ReferencePoint rp = (ReferencePoint)document.GetElement(pointIds[i]);
                Transform t = Transform.CreateTranslation(points[i]);
                rp.SetCoordinateSystem(t);
            }

            return adaptiveComponent;
        }

        /***************************************************/
    }
}


