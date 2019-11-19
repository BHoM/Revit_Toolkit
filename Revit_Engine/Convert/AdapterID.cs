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

namespace BH.Engine.Adapters.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Properties                         ****/
        /***************************************************/
        
        public const string AdapterId = "Revit_id";
        public const string ElementId = "Revit_elementId";
        public const string WorksetId = "Revit_worksetId";
        public const string SpaceId = "SpaceID"; //FG Change per #191 - ongoing discussion on best use of this still ongoing
        public const string AdjacentSpaceId = "AdjacentSpaceID"; //FG Change per #191 - ongoing discussion on best use of this still ongoing
        public const string FamilyName = "Revit_familyName";
        public const string FamilyTypeName = "Revit_familyTypeName";
        public const string CategoryName = "Revit_categoryName";
        public const string ViewName = "Revit_viewName";
        public const string Edges = "Revit_edges";
        public const string ViewTemplate = "View Template";
        public const string FamilyPlacementTypeName = "Revit_familyPlacementTypeName";

        /***************************************************/
    }
}