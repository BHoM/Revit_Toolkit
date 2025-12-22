/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using Autodesk.Revit.UI;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****    Revit side of Revit_Adapter Execute    ****/
        /***************************************************/

        public override Output<List<object>, bool> Execute(IExecuteCommand command, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> result = new Output<List<object>, bool>() { Item1 = null, Item2 = false };
            RevitExecutionConfig config = (actionConfig as RevitExecutionConfig) ?? new RevitExecutionConfig();

            // Suppress warnings
            if (UIControlledApplication != null && config.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            result = IRunCommand(command);

            // Switch of warning suppression
            if (UIControlledApplication != null && config.SuppressFailureMessages)
                UIControlledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return result;
        }

        /***************************************************/

    }
}






