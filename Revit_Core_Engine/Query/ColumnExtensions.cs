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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Calculates the start and end extension of the Revit column.")]
        [Input("familyInstance", "Revit column to extract the extensions from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [MultiOutput(0, "start", "Start extension extracted from the input Revit column.")]
        [MultiOutput(1, "end", "End extension extracted from the input Revit column.")]
        public static Output<double, double> ColumnExtensions(this FamilyInstance familyInstance, RevitSettings settings)
        {
            double startExtension = 0;
            double endExtension = 0;
            if (familyInstance.IsSlantedColumn)
            {
                int attachedBase = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_BASE_ATTACHED_PARAM);
                int attachedTop = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM);

                if (attachedBase == 1 || attachedTop == 1)
                    BH.Engine.Base.Compute.RecordWarning(string.Format("A slanted column is attached at base or top, this may cause wrong length on pull to BHoM. Element Id: {0}", familyInstance.Id.IntegerValue));

                XYZ direction = ((Autodesk.Revit.DB.Line)((LocationCurve)familyInstance.Location).Curve).Direction;
                double angle = direction.AngleTo(XYZ.BasisZ);

                if (attachedBase == 1)
                    startExtension = -familyInstance.LookupParameterDouble(BuiltInParameter.COLUMN_BASE_ATTACHMENT_OFFSET_PARAM);
                else
                {
                    double baseExtensionValue = familyInstance.LookupParameterDouble(BuiltInParameter.SLANTED_COLUMN_BASE_EXTENSION);
                    if (!double.IsNaN(baseExtensionValue) && Math.Abs(baseExtensionValue) > settings.DistanceTolerance)
                    {
                        int baseCutStyle = familyInstance.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_BASE_CUT_STYLE);
                        switch (baseCutStyle)
                        {
                            case 0:
                                startExtension = baseExtensionValue;
                                break;
                            case 1:
                                startExtension = baseExtensionValue / Math.Cos(angle);
                                break;
                            case 2:
                                startExtension = baseExtensionValue / Math.Sin(angle);
                                break;
                        }
                    }
                }

                if (attachedTop == 1)
                    endExtension = -familyInstance.LookupParameterDouble(BuiltInParameter.COLUMN_TOP_ATTACHMENT_OFFSET_PARAM);
                else
                {
                    double topExtensionValue = familyInstance.LookupParameterDouble(BuiltInParameter.SLANTED_COLUMN_TOP_EXTENSION);
                    if (!double.IsNaN(topExtensionValue) && Math.Abs(topExtensionValue) > settings.DistanceTolerance)
                    {
                        int topCutStyle = familyInstance.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_TOP_CUT_STYLE);
                        switch (topCutStyle)
                        {
                            case 0:
                                endExtension = topExtensionValue;
                                break;
                            case 1:
                                endExtension = topExtensionValue / Math.Cos(angle);
                                break;
                            case 2:
                                endExtension = topExtensionValue / Math.Sin(angle);
                                break;
                        }
                    }
                }
            }

            return new Output<double, double> { Item1 = startExtension, Item2 = endExtension };
        }

        /***************************************************/
    }
}

