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

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****           Private Event Handlers          ****/
        /***************************************************/

        private static void ControlledApplication_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            bool hasFailure = false;
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            List<FailureMessageAccessor> failureMessageAccessorsList = failuresAccessor.GetFailureMessages().ToList();
            List<ElementId> elementsToDelete = new List<ElementId>();
            foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessorsList)
            {
                try
                {
                    if (failureMessageAccessor.GetSeverity() == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failureMessageAccessor);
                        continue;
                    }
                    else
                    {
                        failuresAccessor.ResolveFailure(failureMessageAccessor);
                        hasFailure = true;
                        continue;
                    }

                }
                catch
                {
                }
            }

            if (elementsToDelete.Count != 0)
                failuresAccessor.DeleteElements(elementsToDelete);

            if (hasFailure)
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);

            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        /***************************************************/
    }
}