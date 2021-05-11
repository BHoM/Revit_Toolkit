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

using BH.Engine.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        //[Description("Groups and sorts IRequests by their estimated execution time in order to execute fastest first. Order from slowest to fastest: IParameterRequest, IlogicalRequests, others. ")]
        //[Input("requests", "A collection of IRequests to be sorted.")]
        //[Output("sortedRequests")]
        public static ClonedType CloneType(this IBHoMObject sourceRevitObject, string newName)
        {
            if (sourceRevitObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("It is impossible to clone a null Revit object.");
                return null;
            }

            RevitIdentifiers identifiers = sourceRevitObject.FindFragment<RevitIdentifiers>();
            if (identifiers == null)
            {
                BH.Engine.Reflection.Compute.RecordError("The input object is not a valid pulled Revit element.");
                return null;
            }

            ClonedType result = new ClonedType { SourceTypeId = identifiers.FamilyTypeId, Name = newName };
            if (identifiers.ElementId == identifiers.FamilyTypeId)
            {
                RevitParametersToPush parametersToPush = sourceRevitObject.FindFragment<RevitParametersToPush>();
                if (parametersToPush != null)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Parameters to push have been cloned from the source Revit type object.");
                    result.AddFragment(parametersToPush.DeepClone());
                }
            }
            else
                BH.Engine.Reflection.Compute.RecordWarning("The input object is a pulled Revit element, its type has been cloned.");

            return result;
        }

        /***************************************************/
    }
}
