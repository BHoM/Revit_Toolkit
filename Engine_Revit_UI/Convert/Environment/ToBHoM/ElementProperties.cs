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

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this ElementType elementType, PullSettings pullSettings = null)
        {
            //TODO: dynamic does not work. ToBHoM for WallType not recognized
            //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

            ElementProperties aBuildingElementProperties = null;

            if (elementType is WallType)
                aBuildingElementProperties = (elementType as WallType).ToBHoMElementProperties(pullSettings);
            else if (elementType is FloorType)
                aBuildingElementProperties = (elementType as FloorType).ToBHoMElementProperties(pullSettings);
            else if (elementType is CeilingType)
                aBuildingElementProperties = (elementType as CeilingType).ToBHoMElementProperties(pullSettings);
            else if (elementType is RoofType)
                aBuildingElementProperties = (elementType as RoofType).ToBHoMElementProperties(pullSettings);
            else if (elementType is FamilySymbol)
                aBuildingElementProperties = (elementType as FamilySymbol).ToBHoMElementProperties(pullSettings);

            if (aBuildingElementProperties == null)
                aBuildingElementProperties = new ElementProperties();

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this WallType wallType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ElementProperties aElementProperties = Create.ElementProperties(BuildingElementType.Wall, Query.Construction(wallType, pullSettings));
            aElementProperties = aElementProperties.UpdateValues(pullSettings, wallType) as ElementProperties;
            return aElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this FloorType floorType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ElementProperties aElementProperties = Create.ElementProperties(BuildingElementType.Floor, Query.Construction(floorType, pullSettings));
            aElementProperties = aElementProperties.UpdateValues(pullSettings, floorType) as ElementProperties;
            return aElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ElementProperties aElementProperties = Create.ElementProperties(BuildingElementType.Ceiling, Query.Construction(ceilingType, pullSettings));
            aElementProperties = aElementProperties.UpdateValues(pullSettings, ceilingType) as ElementProperties;
            return aElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this RoofType roofType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ElementProperties aElementProperties = Create.ElementProperties(BuildingElementType.Roof, Query.Construction(roofType, pullSettings));
            aElementProperties = aElementProperties.UpdateValues(pullSettings, roofType) as ElementProperties;
            return aElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMElementProperties(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            ElementProperties aElementProperties = Create.ElementProperties(aBuildingElementType.Value);
            aElementProperties = aElementProperties.UpdateValues(pullSettings, familySymbol) as ElementProperties;
            return aElementProperties;
        }

        /***************************************************/
    }
}