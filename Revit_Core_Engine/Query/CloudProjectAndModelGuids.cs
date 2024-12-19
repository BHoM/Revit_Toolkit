/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

        [Description("Returns project and model guids of a model hosted in the cloud. Null if the model is not hosted in the Autodesk cloud.")]
        [Input("document", "Cloud model to query for guids.")]
        [MultiOutput(0, "projectGuid", "Project guid of a model hosted in the cloud.")]
        [MultiOutput(1, "modelGuid", "Model guid of a model hosted in the cloud.")]
        public static Output<Guid, Guid> CloudProjectAndModelGuids(this Document document)
        {
            if (document.IsModelInCloud)
            {
                ModelPath path = document.GetCloudModelPath();
                return new Output<Guid, Guid> { Item1 = path.GetProjectGUID(), Item2 = path.GetModelGUID() };
            }
            else
                return null;
        }

        /***************************************************/
    }
}

