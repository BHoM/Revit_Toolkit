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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static bool IUpdate(this Element element, IBHoMObject bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("The element could not be updated because Revit element does not exist.");
                return false;
            }

            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("The element could not be updated because BHoM object does not exist.");
                return false;
            }

            if (element.Pinned)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Element could not be updated because it is pinned. ElementId: {0}", element.Id));
                return false;
            }

            return Update(element as dynamic, bHoMObject as dynamic, settings, setLocationOnUpdate);
        }


        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static bool Update(this Element element, IBHoMObject bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            bool isElement = new ElementIsElementTypeFilter(true).PassesFilter(element);
            if (isElement)
                element.ISetType(bHoMObject, settings);

            element.CopyParameters(bHoMObject, settings);

            if (!string.IsNullOrWhiteSpace(bHoMObject.Name) && element.Name != bHoMObject.Name)
            {
                try
                {
                    element.Name = bHoMObject.Name;
                }
                catch
                {

                }
            }

            if (setLocationOnUpdate && isElement)
                element.ISetLocation(bHoMObject, settings);

            return true;
        }

        /***************************************************/

        public static bool Update(this Material element, BH.oM.Physical.Materials.Material bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            foreach (BH.oM.Physical.Materials.IMaterialProperties property in BH.Engine.Base.Query.FilterByType(bHoMObject.Fragments, typeof(BH.oM.Physical.Materials.IMaterialProperties)))
            {
                element.CopyCharacteristics(property);
            }

            element.CopyParameters(bHoMObject, settings);

            if (!string.IsNullOrWhiteSpace(bHoMObject.Name) && element.Name != bHoMObject.Name)
            {
                try
                {
                    element.Name = bHoMObject.Name;
                }
                catch
                {

                }
            }

            return true;
        }


        /***************************************************/
        /****             Disallowed Types              ****/
        /***************************************************/

        public static bool Update(this Element element, oM.Structure.Elements.Bar bar, RevitSettings settings, bool setLocationOnUpdate)
        {
            bar.ConvertBeforePushError(typeof(oM.Physical.Elements.IFramingElement));
            return false;
        }

        /***************************************************/

        public static bool Update(this Element element, oM.Structure.Elements.Panel panel, RevitSettings settings, bool setLocationOnUpdate)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return false;
        }

        /***************************************************/

        public static bool Update(this Element element, oM.Environment.Elements.Panel panel, RevitSettings settings, bool setLocationOnUpdate)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return false;
        }

        /***************************************************/
    }
}

