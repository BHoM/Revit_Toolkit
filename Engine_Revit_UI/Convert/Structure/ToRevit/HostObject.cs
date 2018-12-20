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

using System;

using Autodesk.Revit.DB;

using BH.Engine.Geometry;
using BH.Engine.Structure;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static HostObject ToRevitHostObject(this PanelPlanar panelPlanar, Document document, PushSettings pushSettings = null)
        {
            if (panelPlanar == null || document == null)
                return null;

            HostObject aHostObject = pushSettings.FindRefObject<HostObject>(document, panelPlanar.BHoM_Guid);
            if (aHostObject != null)
                return aHostObject;

            pushSettings.DefaultIfNull();

            //TODO: the solution below should be replaced by a Property of a PanelPlanar that would define if it is a floor or wall.
            double dotProduct = Math.Abs(panelPlanar.Outline().FitPlane().Normal.DotProduct(Vector.ZAxis));

            if (dotProduct <= Tolerance.Angle)
                aHostObject = panelPlanar.ToRevitWall(document, pushSettings);
            else if (1 - dotProduct <= Tolerance.Angle)
                aHostObject = panelPlanar.ToRevitFloor(document, pushSettings);
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("The current implementation of BHoM push to Revit works only on horizontal slabs and vertical walls. The Revit element has not been created. BHoM_Guid: {0}", panelPlanar.BHoM_Guid));
                return null;
            }

            aHostObject.CheckIfNullPush(panelPlanar);
            if (aHostObject == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aHostObject, panelPlanar, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(panelPlanar, aHostObject);

            return aHostObject;
        }

        /***************************************************/
    }
}
