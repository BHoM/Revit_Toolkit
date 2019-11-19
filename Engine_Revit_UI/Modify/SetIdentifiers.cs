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
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static IBHoMObject SetIdentifiers(this IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return bHoMObject;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone() as IBHoMObject;

            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            int aWorksetId = WorksetId.InvalidWorksetId.IntegerValue;
            if (element.Document != null && element.Document.IsWorkshared)
            {
                WorksetId aWorksetId_Revit = element.WorksetId;
                if (aWorksetId_Revit != null)
                    aWorksetId = aWorksetId_Revit.IntegerValue;
            }
            aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.WorksetId, aWorksetId);

            Parameter aParameter = null;

            if (element is Family)
            {
                Family aFamily = (Family)element;

                aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.FamilyName, aFamily.Name);

                if (aFamily.FamilyCategory != null)
                    aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.CategoryName, aFamily.FamilyCategory.Name);

                aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.FamilyPlacementTypeName, Query.FamilyPlacementTypeName(aFamily));
            }
            else
            {
                aParameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (aParameter != null)
                    aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.FamilyName, aParameter.AsValueString());

                aParameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                if (aParameter != null)
                    aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.FamilyTypeName, aParameter.AsValueString());

                aParameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
                if (aParameter != null)
                    aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.CategoryName, aParameter.AsValueString());
            }


            return aBHoMObject;
        }

        /***************************************************/
    }
}