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


using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Settings
{
    [Description("General settings that are applicable to all actions performed by the instance of Revit adapter.")]
    public class RevitSettings : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Socket connection settings.")]
        public ConnectionSettings ConnectionSettings { get; set; } = new ConnectionSettings();

        [Description("Revit family load settings. Limited functionality at the moment.")]
        public FamilyLoadSettings FamilyLoadSettings { get; set; } = new FamilyLoadSettings();

        [Description("A collection of relationships between property names of BHoM types and parameter names of correspondent Revit elements.")]
        public MapSettings MapSettings { get; set; } = new MapSettings();
        
        [Description("Name of Revit parameter to be used to store BHoM tags.")]
        public string TagsParameterName { get; set; } = "BHE_Tags";

        [Description("Distance tolerance to be used in geometry processing.")]
        public double DistanceTolerance { get; set; } = BH.oM.Geometry.Tolerance.Distance;

        [Description("Angle tolerance to be used in geometry processing.")]
        public double AngleTolerance { get; set; } = BH.oM.Geometry.Tolerance.Angle;
        
        /***************************************************/
    }
}

