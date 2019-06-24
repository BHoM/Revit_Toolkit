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

namespace BH.oM.Adapters.Revit.Enums
{
    public enum Discipline
    {
        Undefined,
        Environmental,
        Structural,
        Architecture,
        Physical
    }

    public enum RequestType
    {
        Undefined,
        Selection,
        Category,
        Workset,
        ActiveWorkset,
        LogicalAnd,
        LogicalOr,
        ViewTemplate,
        OpenWorksets,
        TypeName,
        Family,
        SelectionSet,
        View,
        Parameter
    }

    //
    // Summary:
    //     An enumerated type listing available view types.
    public enum RevitViewType
    {
        //
        // Summary:
        //     Undefined/unspecified type of view.
        Undefined = 0,
        //
        // Summary:
        //     Floor plan type of view.
        FloorPlan = 1,
        //
        // Summary:
        //     Reflected ceiling plan type of view.
        CeilingPlan = 2,
        //
        // Summary:
        //     Elevation type of view.
        Elevation = 3,
        //
        // Summary:
        //     3-D type of view.
        ThreeD = 4,
        //
        // Summary:
        //     Schedule type of view.
        Schedule = 5,
        //
        // Summary:
        //     Drawing sheet type of view.
        DrawingSheet = 6,
        //
        // Summary:
        //     The project browser view.
        ProjectBrowser = 7,
        //
        // Summary:
        //     Report type of view.
        Report = 8,
        //
        // Summary:
        //     Drafting type of view.
        DraftingView = 10,
        //
        // Summary:
        //     Legend type of view.
        Legend = 11,
        //
        // Summary:
        //     The MEP system browser view.
        SystemBrowser = 12,
        //
        // Summary:
        //     Structural plan or Engineering plan type of view.
        EngineeringPlan = 115,
        //
        // Summary:
        //     Area plan type of view.
        AreaPlan = 116,
        //
        // Summary:
        //     Cross section type of view.
        Section = 117,
        //
        // Summary:
        //     Detail type of view.
        Detail = 118,
        //
        // Summary:
        //     Cost Report view.
        CostReport = 119,
        //
        // Summary:
        //     Loads Report view.
        LoadsReport = 120,
        //
        // Summary:
        //     Pressure Loss Report view.
        PresureLossReport = 121,
        //
        // Summary:
        //     Column Schedule type of view.
        ColumnSchedule = 122,
        //
        // Summary:
        //     Panel Schedule Report view.
        PanelSchedule = 123,
        //
        // Summary:
        //     Walk-Through type of 3D view.
        Walkthrough = 124,
        //
        // Summary:
        //     Rendering type of view.
        Rendering = 125,
        //
        // Summary:
        //     Revit's internal type of view
        //
        // Remarks:
        //     Internal views are not available to API users
        Internal = 214
    }

    public enum AdapterMode
    {
        Delete,
        Replace,
        Update
    }

    public enum TextComparisonType
    {
        Equal,
        NotEqual,
        Contains,
        StartsWith,
        EndsWith
    }

    public enum NumberComparisonType
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        LessOrEqual,
        GreaterOrEqual
    }

}
