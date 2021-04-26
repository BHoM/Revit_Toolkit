/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new Section view in the current Revit file.")]
        [Input("uiDoc", "Revit current UI document to be processed.")]
        [Input("boundingBoxXyz", "The BoundingBoxXYZ to fit the section to.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Output("viewSection", "The new Section view.")]
        public static View ViewSection(this Document document, Autodesk.Revit.DB.BoundingBoxXYZ boundingBoxXyz, string viewName = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            View result = null;

            ViewFamilyType vft = Query.ViewFamilyType(document, ViewFamily.Section);

            result = Autodesk.Revit.DB.ViewSection.CreateSection(document, vft.Id, boundingBoxXyz);
            result.DetailLevel = viewDetailLevel;

            if (viewTemplateId != null)
            {
                try
                {
                    result.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                try
                {
#if (REVIT2018 || REVIT2019)
                    result.ViewName = viewName;
#else
                    result.Name = viewName;
#endif
                }
                catch
                {
                    BH.Engine.Reflection.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }

            return result;
        }

        /***************************************************/
    }
}

