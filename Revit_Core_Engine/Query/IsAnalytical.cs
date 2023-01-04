/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a given Revit element is an analytical element.")]
        [Input("element", "Revit element to be checked whether it is analytical.")]
        [Output("analytical", "True if the input Revit element is analytical, otherwise false.")]
        public static bool IsAnalytical(this Element element)
        {
            if (element == null || element.Category == null)
                return false;

            return AnalyticalCategories.Any(x => element.Category.Id.IntegerValue == (int)x);
        }


        /***************************************************/
        /****            Private Collection             ****/
        /***************************************************/

        private static BuiltInCategory[] AnalyticalCategories = new BuiltInCategory[]
        {
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Lines,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Planes,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalNodes_Points,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalPipeConnectionLineSymbol,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalPipeConnections,
            Autodesk.Revit.DB.BuiltInCategory.OST_AnalyticalRigidLinks,
            Autodesk.Revit.DB.BuiltInCategory.OST_BeamAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_BraceAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalyticalGeometry,
            Autodesk.Revit.DB.BuiltInCategory.OST_ColumnAnalyticalRigidLinks,
            Autodesk.Revit.DB.BuiltInCategory.OST_FloorAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_FloorsAnalyticalGeometry,
            Autodesk.Revit.DB.BuiltInCategory.OST_FootingAnalyticalGeometry,
            Autodesk.Revit.DB.BuiltInCategory.OST_FoundationSlabAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_FramingAnalyticalGeometry,
            Autodesk.Revit.DB.BuiltInCategory.OST_IsolatedFoundationAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_LinksAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_RigidLinksAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_WallAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_WallFoundationAnalytical,
            Autodesk.Revit.DB.BuiltInCategory.OST_WallsAnalyticalGeometry,
        };

        /***************************************************/
    }
}

