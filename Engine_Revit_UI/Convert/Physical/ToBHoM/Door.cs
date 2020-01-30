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

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Door ToBHoMDoor(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Door door = pullSettings.FindRefObject<Door>(familyInstance.Id.IntegerValue);
            if (door != null)
                return door;

            PolyCurve polycurve = Query.PolyCurve(familyInstance, pullSettings);
            if (polycurve == null)
                return null;

            door = new Door()
            {
                Name = Query.FamilyTypeFullName(familyInstance),
                Location = BH.Engine.Geometry.Create.PlanarSurface(polycurve)
            };

            ElementType elementType = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as ElementType;
            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = familyInstance.Id.IntegerValue.ToString();
            originContext.TypeName = Query.FamilyTypeFullName(familyInstance);
            originContext = originContext.UpdateValues(pullSettings, familyInstance) as OriginContextFragment;
            originContext = originContext.UpdateValues(pullSettings, elementType) as OriginContextFragment;
            door.Fragments.Add(originContext);

            door = Modify.SetIdentifiers(door, familyInstance) as Door;
            if (pullSettings.CopyCustomData)
                door = Modify.SetCustomData(door, familyInstance) as Door;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(door);

            return door;
        }

        /***************************************************/
    }
}
