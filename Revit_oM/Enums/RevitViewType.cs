/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Enums
{
    /***************************************************/

    [Description("Enumerator defining view type of Revit view. A clone of Autodesk.Revit.DB.ViewType enum.")]
    public enum RevitViewType
    {
        [Description("Undefined/unspecified type of view.")]
        Undefined = 0,
        [Description("Floor plan type of view.")]
        FloorPlan = 1,
        [Description("Reflected ceiling plan type of view.")]
        CeilingPlan = 2,
        [Description("Elevation type of view.")]
        Elevation = 3,
        [Description("3-D type of view.")]
        ThreeD = 4,
        [Description("Schedule type of view.")]
        Schedule = 5,
        [Description("Drawing sheet type of view.")]
        DrawingSheet = 6,
        [Description("The project browser view.")]
        ProjectBrowser = 7,
        [Description("Report type of view.")]
        Report = 8,
        [Description("Drafting type of view.")]
        DraftingView = 10,
        [Description("Legend type of view.")]
        Legend = 11,
        [Description("The MEP system browser view.")]
        SystemBrowser = 12,
        [Description("Structural plan or Engineering plan type of view.")]
        EngineeringPlan = 115,
        [Description("Area plan type of view.")]
        AreaPlan = 116,
        [Description("Cross section type of view.")]
        Section = 117,
        [Description("Detail type of view.")]
        Detail = 118,
        [Description("Cost Report view.")]
        CostReport = 119,
        [Description("Loads Report view.")]
        LoadsReport = 120,
        [Description("Pressure Loss Report view.")]
        PresureLossReport = 121,
        [Description("Column Schedule type of view.")]
        ColumnSchedule = 122,
        [Description("Panel Schedule Report view.")]
        PanelSchedule = 123,
        [Description("Walk-Through type of 3D view.")]
        Walkthrough = 124,
        [Description("Rendering type of view.")]
        Rendering = 125,
        [Description("Systems analysis report view.")]
        SystemsAnalysisReport = 126,
        [Description("Revit's internal type of view.")]
        Internal = 214
    }

    /***************************************************/
}


