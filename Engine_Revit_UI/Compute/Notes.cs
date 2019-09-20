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
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        public static PullSettings DefaultIfNull(this PullSettings pullSettings)
        {
            if (pullSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordNote("Pull settings are not set. Default settings are used.");
                return PullSettings.Default;
            }

            return pullSettings;
        }

        /***************************************************/

        public static PushSettings DefaultIfNull(this PushSettings pushSettings)
        {
            if (pushSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordNote("Push settings are not set. Default settings are used.");
                return PushSettings.Default;
            }

            return pushSettings;
        }


        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void NonIsotopicStructuralAssetNote(this oM.Physical.Materials.Material material)
        {
            string aMessage = "Revit Structural Asset is Non-Isotopic.";

            if (material != null)
                aMessage = string.Format("{0} BHoM Guid: {1}", aMessage, material.BHoM_Guid);

            BH.Engine.Reflection.Compute.RecordNote(aMessage);
        }

        /***************************************************/

        internal static void MaterialNotInLibraryNote(this Material material)
        {
            string aMessage = "Material could not be found in BHoM Libary.";

            if (material != null)
                aMessage = string.Format("{0} Material Id: {1}", aMessage, material.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordNote(aMessage);
        }

        /***************************************************/

        internal static void MaterialNotInLibraryNote(this Element element)
        {
            string aMessage = "Material could not be found in BHoM Libary.";

            if (element != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, element.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordNote(aMessage);
        }

        /***************************************************/


    }
}