﻿/*
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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get the ElementId of all Family Types of certain Families")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("familyIds", "Family ids to search for Family Type ids")]
        [Output("elementIdsOfFamilyTypes", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsOfFamilyTypes(this Document document, IEnumerable<ElementId> familyIds, IEnumerable<ElementId> ids = null)
        {
            if (document == null || familyIds == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();

            foreach(ElementId id in familyIds)
            {
                Family family = document.GetElement(id) as Family;

                if (family == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Couldn't find any Family with the Id " + id.ToString() + ".");
                    continue;
                }

                ISet<ElementId> symbolId = family.GetFamilySymbolIds();

                foreach(ElementId sId in symbolId)
                {
                    result.Add(sId);
                }
            }
            return result; 
        }
        
        [Description("Get the ElementId of all Family Types, with option to narrow search by a certain Family id")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("familyId", "Optional, Family id to search for Family Type ids")]
        [Output("elementIdsOfFamilyTypes", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsOfFamilyTypes(this Document document, ElementId familyId = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if(familyId == null)
            {
                FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
                return collector.WhereElementIsElementType().ToElementIds();
            }
            else
            {
                HashSet<ElementId> result = new HashSet<ElementId>();

                Family family = document.GetElement(familyId) as Family;

                if (family == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Couldn't find any Family with the Id " + familyId.ToString() + ".");
                    return null;
                }

                ISet<ElementId> symbolId = family.GetFamilySymbolIds();

                foreach (ElementId sId in symbolId)
                {
                    result.Add(sId);
                }

                return result;
            }
        }

        /***************************************************/

    }
}