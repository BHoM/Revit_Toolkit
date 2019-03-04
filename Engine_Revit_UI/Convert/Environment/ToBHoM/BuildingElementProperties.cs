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

        internal static ElementProperties ToBHoMBuildingElementProperties(this ElementType elementType, PullSettings pullSettings = null)
        {
            //TODO: dynamic does not work. ToBHoM for WallType not recognized
            //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

            ElementProperties aBuildingElementProperties = null;

            if (elementType is WallType)
                aBuildingElementProperties = (elementType as WallType).ToBHoMBuildingElementProperties(pullSettings);
            else if (elementType is FloorType)
                aBuildingElementProperties = (elementType as FloorType).ToBHoMBuildingElementProperties(pullSettings);
            else if (elementType is CeilingType)
                aBuildingElementProperties = (elementType as CeilingType).ToBHoMBuildingElementProperties(pullSettings);
            else if (elementType is RoofType)
                aBuildingElementProperties = (elementType as RoofType).ToBHoMBuildingElementProperties(pullSettings);
            else if (elementType is FamilySymbol)
                aBuildingElementProperties = (elementType as FamilySymbol).ToBHoMBuildingElementProperties(pullSettings);

            if (aBuildingElementProperties == null)
                aBuildingElementProperties = new ElementProperties();

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static ElementProperties ToBHoMBuildingElementProperties(this WallType wallType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return Create.ElementProperties(BuildingElementType.Wall, Query.Construction(wallType, pullSettings));
        }

        /***************************************************/

        internal static ElementProperties ToBHoMBuildingElementProperties(this FloorType floorType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return Create.ElementProperties(BuildingElementType.Floor, Query.Construction(floorType, pullSettings));
        }

        /***************************************************/

        internal static ElementProperties ToBHoMBuildingElementProperties(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return Create.ElementProperties(BuildingElementType.Ceiling, Query.Construction(ceilingType, pullSettings));
        }

        /***************************************************/

        internal static ElementProperties ToBHoMBuildingElementProperties(this RoofType roofType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return Create.ElementProperties(BuildingElementType.Roof, Query.Construction(roofType, pullSettings));
        }

        /***************************************************/

        internal static ElementProperties ToBHoMBuildingElementProperties(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            return Create.ElementProperties(aBuildingElementType.Value);
        }

        /***************************************************/
    }
}