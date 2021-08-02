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
using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Window WindowFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return familyInstance.WindowFromRevit(null, settings, refObjects);
        }

        public static Window WindowFromRevit(this FamilyInstance familyInstance, HostObject host = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null)
                return null;

            settings = settings.DefaultIfNull();

            string refId = familyInstance.Id.ReferenceIdentifier(host);
            Window window = refObjects.GetValue<Window>(refId);
            if (window != null)
                return window;

            BH.oM.Geometry.ISurface location = familyInstance.OpeningSurface(host, settings);
            if (location == null)
            {
                if (host == null)
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("Location of the window could not be retrieved from the model (possibly it has zero area or lies on a non-planar face). A window object without location has been returned. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("Location of the window could not be retrieved from the model (possibly it has zero area or lies on a non-planar face), the opening has been skipped. Revit ElementId: {0}", familyInstance.Id.IntegerValue));
                    return null;
                }
            }

            window = new Window { Location = location, Name = familyInstance.FamilyTypeFullName() };
            
            //Set identifiers, parameters & custom data
            window.SetIdentifiers(familyInstance);
            window.CopyParameters(familyInstance, settings.ParameterSettings);
            window.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(refId, window);
            return window;
        }

        /***************************************************/
    }
}

