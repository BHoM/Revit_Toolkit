/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.oM.Base;
using System;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void NullObjectCreateError(Type type)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM {0} is null", type.Name));
        }

        /***************************************************/

        private static void NullDocumentCreateError()
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Document is null. Objects could not be created"));
        }

        /***************************************************/

        private static void NullObjectsCreateError()
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Objects are null."));
        }

        /***************************************************/

        private static void ObjectNotCreatedCreateError(IBHoMObject iBHoMObject)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Revit object could not be created. BHoM object Guid: {0}", iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        private static void ObjectNotMovedWarning(IBHoMObject iBHoMObject)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Revit object could not be moved. Revit element id: {0}, BHoM object Guid: {1}", Query.ElementId( iBHoMObject).ToString(),  iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        private static void DeletePinnedElementError(Element element)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("Could not delete pinned element. Element Id: {0}", element.Id));
        }

        /***************************************************/

        private static void ConvertBeforePushError(IBHoMObject iBHoMObject, Type typeToConvert)
        {
            BH.Engine.Reflection.Compute.RecordError(string.Format("{0} has to be converted to {1} before pushing. BHoM object Guid: {2}", iBHoMObject.GetType().Name, typeToConvert.Name, iBHoMObject.BHoM_Guid));
        }

        /***************************************************/

        
    }
}
