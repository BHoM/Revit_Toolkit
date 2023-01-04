/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Filters ElementIds of elements being members of selection sets, assemblies, systems etc.")]
        [Input("element", "Revit element to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> IElementIdsOfMemberElements(this Element element)
        {
            if (element == null)
                return null;

            return ElementIdsOfMemberElements(element as dynamic);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements being members of selection sets, assemblies, systems etc.")]
        [Input("document", "Revit document to be processed.")]
        [Input("parentId", "Integer value of ElementId of the Revit element to be queried for its member elements.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this Document document, int parentId, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            HashSet<ElementId> result = new HashSet<ElementId>();
            Element element = document.GetElement(new ElementId(parentId));
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordWarning(String.Format("Element under ElementId {0} does not exist in the Revit model.", parentId));
                return result;
            }
            else
                result.UnionWith(element.IElementIdsOfMemberElements());

            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit group.")]
        [Input("group", "Revit group to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this Group group)
        {
            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            IEnumerable<ElementId> memberIds = group.GetMemberIds();
            foreach (ElementId elementId in memberIds)
            {
                Element e = group.Document.GetElement(elementId);
                if (e.Category != null && e.Category.Id.IntegerValue != (int)Autodesk.Revit.DB.BuiltInCategory.OST_WeakDims && e.Category.Id.IntegerValue != (int)Autodesk.Revit.DB.BuiltInCategory.OST_SketchLines && !e.IsAnalytical())
                    elementIds.Add(e.Id);

                if (e is Group)
                    elementIds.UnionWith(e.IElementIdsOfMemberElements());
            }

            return elementIds;
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit assembly.")]
        [Input("assemblyInstance", "Revit assembly to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this AssemblyInstance assemblyInstance)
        {
            return assemblyInstance.GetMemberIds();
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit mechanical system.")]
        [Input("mechanicalSystem", "Revit mechanical system to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this Autodesk.Revit.DB.Mechanical.MechanicalSystem mechanicalSystem)
        {

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            foreach (Element element in mechanicalSystem.DuctNetwork)
            {
                elementIds.Add(element.Id);
            }

            foreach (Element element in mechanicalSystem.Elements)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit piping system.")]
        [Input("pipingSystem", "Revit piping system to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this Autodesk.Revit.DB.Plumbing.PipingSystem pipingSystem)
        {

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            foreach (Element element in pipingSystem.PipingNetwork)
            {
                elementIds.Add(element.Id);
            }

            foreach (Element element in pipingSystem.Elements)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit electrical system.")]
        [Input("electricalSystem", "Revit electrical system to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this Autodesk.Revit.DB.Electrical.ElectricalSystem electricalSystem)
        {

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            foreach (Element element in electricalSystem.Elements)
            {
                elementIds.Add(element.Id);
            }

            return elementIds;
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being members of a given Revit selection set.")]
        [Input("selectionSet", "Revit selection set to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this SelectionFilterElement selectionSet)
        {
            return selectionSet.GetElementIds();
        }

        /***************************************************/

        [Description("Filters ElementIds of elements being nested members of a given Revit family instance.")]
        [Input("familyInstance", "Revit element to be queried for its member elements.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfMemberElements(this FamilyInstance familyInstance)
        {
            // Note this method only returns shared nested families
            return familyInstance.GetSubComponentIds();
        }

        /***************************************************/
    }
}


