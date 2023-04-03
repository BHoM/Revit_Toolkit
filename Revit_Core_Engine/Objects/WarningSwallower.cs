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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public class WarningSwallower : IFailuresPreprocessor
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Preprocess the failures, in this case swallow all of them.")]
        [Input("failuresAccessor", "Revit failures accessor object.")]
        [Output("failureProcessingResult", "The result of the failure processing.")]
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessageAccessors = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessors)
            {
                failuresAccessor.DeleteWarning(failureMessageAccessor);
            }

            return FailureProcessingResult.Continue;
        }

        /***************************************************/
    }
}



