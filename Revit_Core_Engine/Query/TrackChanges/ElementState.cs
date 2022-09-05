/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets element state for use in tracking changes.")]
        public static string GetElementState(Element element, ChangeManagerConfig changeManagerConfig)
        {
            string elementState = null;

            List<string> properties = new List<string>();

            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);

            properties.Add(ElementDescription(element)
              + " at " + LocationString(element.Location));
            
            if (!(element is FamilyInstance) && boundingBox != null)
            {
                properties.Add("Box="
                  + BoundingBoxString(boundingBox));

                properties.Add("Vertices="
                  + PointArrayString(GetCanonicVertices(element)));
            }

            if (changeManagerConfig.IsModifications)
            {
                if (changeManagerConfig.Properties.Count == 0)
                {
                    properties.Add("Parameters="
                      + GetPropertiesJson(element.GetOrderedParameters()));
                }
                else
                {
                    properties.Add("Parameters="
                  + GetPropertiesJson(element.GetOrderedParameters().Where(p => changeManagerConfig.Properties.Contains(p.StringValue())).ToList()));
                }
            }
            elementState = string.Join(", ", properties);
            return elementState;
        }
        /***************************************************/

        public static List<Element> GetTrackedElements(Document doc)
        {
            Categories documentCategories = doc.Settings.Categories;

            List<ElementFilter> elementFilters = new List<ElementFilter>();

            foreach (Category category in documentCategories)
            {
                if (CategoryType.Model == category.CategoryType)
                {
                    elementFilters.Add(new ElementCategoryFilter(category.Id));
                }
                if (CategoryType.AnalyticalModel == category.CategoryType)
                {
                    elementFilters.Add(new ElementCategoryFilter(category.Id));
                }
                if (CategoryType.Annotation == category.CategoryType)
                {
                    elementFilters.Add(new ElementCategoryFilter(category.Id));
                }
            }

            ElementFilter isModelCategory
              = new LogicalOrFilter(elementFilters);

            Options opt = new Options();

            return new FilteredElementCollector(doc)
              .WherePasses(isModelCategory).ToList();
        }

        public static List<Element> GetTrackedElements(Document doc, List<BuiltInCategory> categories)
        {
            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(categories);

            return new FilteredElementCollector(doc).WherePasses(elementMulticategoryFilter).ToList();
        }
    }
}

