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
using BH.oM.Physical.FramingProperties;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Beam BeamFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Beam beam = refObjects.GetValue<Beam>(familyInstance.Id);
            if (beam != null)
                return beam;

            oM.Geometry.ICurve locationCurve = familyInstance.LocationCurveFraming(settings);
            IFramingElementProperty property = familyInstance.FramingElementProperty(settings, refObjects);
            beam = BH.Engine.Physical.Create.Beam(locationCurve, property, familyInstance.Name);

            //Set identifiers, parameters & custom data
            beam.SetIdentifiers(familyInstance);
            beam.CopyParameters(familyInstance, settings.ParameterSettings);
            beam.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(familyInstance.Id, beam);
            return beam;
        }

        /***************************************************/
    }
}

