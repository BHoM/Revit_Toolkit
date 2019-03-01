/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Internal methods            ****/
        /***************************************************/

        internal static void NullDocumentError()
        {
            BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit document does not exist.");
        }

        /***************************************************/

        internal static void FamilyPlacementTypeMismatchError(this IBHoMObject iBHoMObject, Family family)
        {
            string aMessage = "Family Placement Type conflict. Family Instance could not be created.";

            if (iBHoMObject != null)
                aMessage = string.Format("{0} BHoM Guid: {1}", aMessage, iBHoMObject.BHoM_Guid);

            if (family != null)
                aMessage = string.Format("{0} Element Id : {1}", aMessage, family.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordError(aMessage);
        }

        /***************************************************/

        internal static void FamilySymbolNotFoundError(this IBHoMObject iBHoMObject)
        {
            string aMessage = "Family symbol has not been found for given BHoM Object.";

            if (iBHoMObject != null)
                aMessage = string.Format("{0} BHoM Guid: {1}", aMessage, iBHoMObject.BHoM_Guid);

            BH.Engine.Reflection.Compute.RecordError(aMessage);
        }

    }
}