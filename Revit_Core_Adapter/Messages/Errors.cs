/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void NullObjectCreateError(Type type)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM {0} is null", type.Name));
        }

        /***************************************************/

        private static void ObjectNotCreatedError(IBHoMObject iBHoMObject, Exception ex)
        {
            BH.Engine.Reflection.Compute.RecordError($"Revit object could not be created due to the following exception:\n{ex.Message}\nBHoM object Guid: {iBHoMObject.BHoM_Guid}");
        }

        /***************************************************/

        private static void ObjectNotUpdatedError(Element element, IBHoMObject iBHoMObject)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be updated. Revit ElementId: {0} BHoM object Guid: {1}", element.Id, iBHoMObject.BHoM_Guid));
        }

        /***************************************************/
    }
}



