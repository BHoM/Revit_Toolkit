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

using BH.Engine.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Creates a ClonedType object based on a pulled Revit element (extracts its type then) or element type.\n" + 
                     "ClonedType can be pushed to Revit in order to clone the source type.")]
        [Input("sourceRevitObject", "Source Revit object, which type is meant to be cloned.")]
        [Input("newName", "Name to be assigned to the cloned type in Revit.")]
        [Output("clonedType", "BHoM object representing a cloned Revit element type, to be pushed in order to create a new type.")]
        public static ClonedType CloneType(this IBHoMObject sourceRevitObject, string newName)
        {
            if (sourceRevitObject == null)
            {
                BH.Engine.Base.Compute.RecordError("It is impossible to clone a null Revit object.");
                return null;
            }

            RevitIdentifiers identifiers = sourceRevitObject.FindFragment<RevitIdentifiers>();
            if (identifiers == null)
            {
                BH.Engine.Base.Compute.RecordError("The input object is not a valid pulled Revit element.");
                return null;
            }

            ClonedType result = new ClonedType { SourceTypeId = identifiers.FamilyTypeId, Name = newName };
            if (identifiers.ElementId == identifiers.FamilyTypeId)
            {
                RevitParametersToPush parametersToPush = sourceRevitObject.FindFragment<RevitParametersToPush>();
                if (parametersToPush != null)
                {
                    BH.Engine.Base.Compute.RecordWarning("Parameters to push have been cloned from the source Revit type object.");
                    result.AddFragment(parametersToPush.DeepClone());
                }
            }
            else
                BH.Engine.Base.Compute.RecordWarning("The input object is a pulled Revit element, its type has been cloned.");

            return result;
        }

        /***************************************************/
    }
}


