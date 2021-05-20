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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static bool SetType(this FamilyInstance element, IFramingElement bHoMObject, RevitSettings settings)
        {
            if (element.TrySetTypeFromString(bHoMObject, settings))
                return true;

            Document doc = element.Document;
            FamilySymbol familySymbol = bHoMObject.Property.ToRevitElementType(doc, bHoMObject.BuiltInCategories(doc), settings);
            if (familySymbol == null)
                familySymbol = bHoMObject.IElementType(doc, settings) as FamilySymbol;

            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(bHoMObject);
                return false;
            }

            return element.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, familySymbol.Id);
        }

        /***************************************************/

        public static bool SetType(this HostObject element, ISurface bHoMObject, RevitSettings settings)
        {
            if (element.TrySetTypeFromString(bHoMObject, settings))
                return true;

            Document doc = element.Document;
            HostObjAttributes hostObjAttr = bHoMObject.Construction.ToRevitElementType(doc, bHoMObject.BuiltInCategories(doc), settings);
            if (hostObjAttr == null)
                hostObjAttr = bHoMObject.IElementType(doc, settings) as HostObjAttributes;

            if (hostObjAttr != null && hostObjAttr.Id.IntegerValue != element.GetTypeId().IntegerValue)
                return element.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, hostObjAttr.Id);

            return false;
        }

        /***************************************************/

        public static bool SetType(this Element element, IInstance instance, RevitSettings settings)
        {
            if (element.TrySetTypeFromString(instance, settings))
                return true;

            ElementType elementType = instance.Properties.ElementType(element.Document, settings);
            if (elementType == null)
                elementType = instance.IElementType(element.Document, settings);

            if (elementType != null)
            {
                try
                {
                    return element.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, elementType.Id);
                }
                catch
                {

                }
            }

            return false;
        }


        /***************************************************/
        /****              Fallback Methods             ****/
        /***************************************************/

        public static bool SetType(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            if (element.TrySetTypeFromString(bHoMObject, settings))
                return true;

            Type type = element.GetType();
            if (type != typeof(Autodesk.Revit.DB.Family) && !typeof(ElementType).IsAssignableFrom(type))
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Element type has not been updated based on the BHoM object due to the lacking convert method. Revit ElementId: {0} BHoM_Guid: {1}", element.Id, bHoMObject.BHoM_Guid));

            return false;
        }
        

        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static bool ISetType(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            return SetType(element as dynamic, bHoMObject as dynamic, settings);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        public static bool TrySetTypeFromString(this Element element, IBHoMObject bHoMObject, RevitSettings settings)
        {
            if (element.Category.Id.IntegerValue < 0)
            {
                RevitParameter param = (bHoMObject.Fragments?.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush)?.Parameters?.FirstOrDefault(x => x.Name == "Type");
                if (param?.Value is string)
                {
                    ElementType et = element.Document.ElementType(null, (string)param.Value, new[] { (BuiltInCategory)element.Category.Id.IntegerValue }, settings);
                    if (et != null)
                    {
                        element.SetParameter(BuiltInParameter.ELEM_TYPE_PARAM, et.Id);
                        return true;
                    }
                    else
                        BH.Engine.Reflection.Compute.RecordWarning($"Element type not updated: type named {param.Value} of category {element.Category.Name} could not be found in the model. ElementId: {element.Id.IntegerValue}");
                }
            }

            return false;
        }

        /***************************************************/
    }
}


