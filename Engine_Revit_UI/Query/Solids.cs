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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<Solid> Solids(this GeometryElement geometryElement, Transform transform = null, PullSettings pullSettings = null)
        {
            if (geometryElement == null)
                return null;

            List<Solid> aResult = new List<Solid>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                if (aGeometryObject is GeometryInstance)
                {
                    GeometryInstance aGeometryInstance = (GeometryInstance)aGeometryObject;

                    Transform aTransform = aGeometryInstance.Transform;
                    if (transform != null)
                        aTransform = aTransform.Multiply(transform.Inverse);

                    GeometryElement aGeometryElement = aGeometryInstance.GetInstanceGeometry(aTransform);
                    if (aGeometryElement == null)
                        continue;

                    List<Solid> aSolidList = Solids(aGeometryElement);
                    if (aSolidList != null && aSolidList.Count != 0)
                        aResult.AddRange(aSolidList);
            }

                else if (aGeometryObject is Solid)
                {
                    aResult.Add((Solid)aGeometryObject);                                              
                }

            }
            return aResult;
        }

        /***************************************************/

        public static List<Solid> Solids(this Element element, Options options, PullSettings pullSettings = null)
        {
            GeometryElement aGeometryElement = element.get_Geometry(options);

            Transform aTransform = null;
            if (element is FamilyInstance)
                aTransform = ((FamilyInstance)element).GetTotalTransform();

            return Solids(aGeometryElement, aTransform, pullSettings);
        }

        /***************************************************/
    }
}
