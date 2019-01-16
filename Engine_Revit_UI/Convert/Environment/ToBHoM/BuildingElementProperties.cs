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

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this ElementType elementType, PullSettings pullSettings = null)
        {
            //TODO: dynamic does not work. ToBHoM for WallType not recognized
            //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

            BuildingElementProperties aBuildingElementProperties = null;

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
                aBuildingElementProperties = new BuildingElementProperties();

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this WallType wallType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject<BuildingElementProperties>(wallType.Id.IntegerValue);
            if (aBuildingElementProperties != null)
                return aBuildingElementProperties;

            aBuildingElementProperties = Create.BuildingElementProperties(Query.FamilyTypeFullName(wallType), BuildingElementType.Wall);
            aBuildingElementProperties.Construction = Query.Construction(wallType, pullSettings);
            //aBuildingElementProperties = Create.BuildingElementProperties(wallType.Name, BuildingElementType.Wall);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FloorType floorType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject<BuildingElementProperties>(floorType.Id.IntegerValue);
            if (aBuildingElementProperties != null)
                return aBuildingElementProperties;

            aBuildingElementProperties = Create.BuildingElementProperties(Query.FamilyTypeFullName(floorType), BuildingElementType.Floor);
            aBuildingElementProperties.Construction = Query.Construction(floorType, pullSettings);
            //aBuildingElementProperties = Create.BuildingElementProperties(floorType.Name, BuildingElementType.Floor);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this CeilingType ceilingType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject<BuildingElementProperties>(ceilingType.Id.IntegerValue);
            if (aBuildingElementProperties != null)
                return aBuildingElementProperties;

            aBuildingElementProperties = Create.BuildingElementProperties(Query.FamilyTypeFullName(ceilingType), BuildingElementType.Ceiling);
            aBuildingElementProperties.Construction = Query.Construction(ceilingType, pullSettings);
            //aBuildingElementProperties = Create.BuildingElementProperties(ceilingType.Name, BuildingElementType.Ceiling);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this RoofType roofType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject<BuildingElementProperties>(roofType.Id.IntegerValue);
            if (aBuildingElementProperties != null)
                return aBuildingElementProperties;

            aBuildingElementProperties = Create.BuildingElementProperties(Query.FamilyTypeFullName(roofType), BuildingElementType.Roof);
            aBuildingElementProperties.Construction = Query.Construction(roofType, pullSettings);
            //aBuildingElementProperties = Create.BuildingElementProperties(roofType.Name, BuildingElementType.Roof);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);

            return aBuildingElementProperties;
        }

        /***************************************************/

        internal static BuildingElementProperties ToBHoMBuildingElementProperties(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject<BuildingElementProperties>(familySymbol.Id.IntegerValue);
            if (aBuildingElementProperties != null)
                return aBuildingElementProperties;

            BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
            if (!aBuildingElementType.HasValue)
                aBuildingElementType = BuildingElementType.Undefined;

            aBuildingElementProperties = Create.BuildingElementProperties(Query.FamilyTypeFullName(familySymbol), aBuildingElementType.Value);
            //aBuildingElementProperties = Create.BuildingElementProperties(familySymbol.Name, aBuildingElementType.Value);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, familySymbol) as BuildingElementProperties;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, pullSettings.ConvertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElementProperties;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);

            return aBuildingElementProperties;
        }

        /***************************************************/
    }
}