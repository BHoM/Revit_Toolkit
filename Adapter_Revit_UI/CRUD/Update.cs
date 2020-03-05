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
using BH.UI.Revit.Engine;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool Update(Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            string tagsParameterName = settings.TagsParameterName;
            
            try
            {
                element.IUpdate(bHoMObject, settings);

                //Assign Tags
                if (!string.IsNullOrEmpty(tagsParameterName))
                    element.SetTags(bHoMObject, tagsParameterName);

                return true;
            }
            catch
            {
                ObjectNotUpdatedError(element, bHoMObject);
                return false;
            }
        }

        /***************************************************/
    }
}