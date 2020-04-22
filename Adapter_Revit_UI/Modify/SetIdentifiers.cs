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

using BH.Adapter.Revit;
using BH.oM.Base;
using BH.UI.Revit.Engine;

namespace BH.UI.Revit.Adapter
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void SetIdentifiers(this IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return;

            bHoMObject.CustomData[RevitAdapter.ElementId] = element.Id.IntegerValue;
            bHoMObject.CustomData[RevitAdapter.AdapterIdName] = element.UniqueId;

            if (element is Family)
            {
                Family family = (Family)element;

                bHoMObject.CustomData[RevitAdapter.FamilyPlacementTypeName] = family.FamilyPlacementTypeName();
                bHoMObject.CustomData[RevitAdapter.FamilyName] = family.Name;
                if (family.FamilyCategory != null)
                    bHoMObject.CustomData[RevitAdapter.CategoryName] = family.FamilyCategory.Name;
            }
            else
            {
                int worksetID = WorksetId.InvalidWorksetId.IntegerValue;
                if (element.Document != null && element.Document.IsWorkshared)
                {
                    WorksetId revitWorksetID = element.WorksetId;
                    if (revitWorksetID != null)
                        worksetID = revitWorksetID.IntegerValue;
                }

                Parameter parameter = null;

                parameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        bHoMObject.CustomData[RevitAdapter.FamilyName] = value;
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        bHoMObject.CustomData[RevitAdapter.FamilyTypeName] = value;
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        bHoMObject.CustomData[RevitAdapter.CategoryName] = value;
                }
            }

        }
        
        /***************************************************/
    }
}
