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
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new Section view in the current Revit file.")]
        [Input("document", "Revit current UI document to be processed.")]
        [Input("boundingBoxXyz", "The BoundingBoxXYZ to fit the section to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamilyType", "View type to be applied on view creation. If left empty, a default section view type will be used.")]
        [Input("viewTemplateId", "View Template Id to be applied in the view. If left empty, default template defined in view type will be applied.")]
        [Input("viewScale", "Scale of the view in format 1:value (1:100 should take input equal to 100). Only applicable if the template does not own the parameter.")]
        [Input("viewDetailLevel", "Detail Level of the view. Only applicable if the template does not own the parameter.")]
        [Output("viewSection", "Section view created based on the inputs.")]
        public static ViewSection ViewSection(this Document document, BoundingBoxXYZ boundingBoxXyz, string viewName = null, ViewFamilyType viewFamilyType = null, ElementId viewTemplateId = null, int viewScale = 0, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Undefined)
        {
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a section view in a null document.");
                return null;
            }

            if (boundingBoxXyz == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a section view based on a null box.");
                return null;
            }

            if (viewFamilyType == null)
                viewFamilyType = Query.ViewFamilyType(document, ViewFamily.Section);

            if (viewFamilyType == null)
            {
                BH.Engine.Base.Compute.RecordError("Section view creation failed: no valid view type could not be found to create it.");
                return null;
            }

            ViewSection result = Autodesk.Revit.DB.ViewSection.CreateSection(document, viewFamilyType.Id, boundingBoxXyz);

            if (viewScale != 0)
                result.SetViewScale(viewScale);

            if (viewDetailLevel != ViewDetailLevel.Undefined)
                result.SetViewDetailLevel(viewDetailLevel);

            if (viewTemplateId != null)
            {
                try
                {
                    result.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
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
                    BH.Engine.Base.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }

            return result;
        }

        /***************************************************/
    }
}



