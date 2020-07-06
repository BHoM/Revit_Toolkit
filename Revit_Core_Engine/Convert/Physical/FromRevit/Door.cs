/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Door DoorFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Door door = refObjects.GetValue<Door>(familyInstance.Id);
            if (door != null)
                return door;

            PlanarSurface location = familyInstance.OpeningSurface(settings);
            if (location == null)
            {
                //TODO: add to refobjects, throw warning, maybe return without geometry?
                return null;
            }

            door = new Door { Location = location, Name = familyInstance.FamilyTypeFullName() };
            ElementType elementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = familyInstance.Id.IntegerValue.ToString();
            originContext.TypeName = familyInstance.FamilyTypeFullName();
            originContext.SetProperties(familyInstance, settings.ParameterSettings);
            originContext.SetProperties(elementType, settings.ParameterSettings);
            door.Fragments.Add(originContext);

            //Set identifiers, parameters & custom data
            door.SetIdentifiers(familyInstance);
            door.CopyParameters(familyInstance, settings.ParameterSettings);
            door.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(familyInstance.Id, door);
            return door;
        }

        /***************************************************/
    }
}
