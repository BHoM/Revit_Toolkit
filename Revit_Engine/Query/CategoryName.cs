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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Revit Category name from RevitFilePreview.")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Output("CategoryName")]
        public static string CategoryName(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return CategoryName(revitFilePreview.XDocument());
        }

        /***************************************************/

        [Description("Gets Revit Category name from XDocument.")]
        [Input("xDocument", "XDocument from Header of Revit Family File (*.rfa)")]
        [Output("CategoryName")]
        public static string CategoryName(this XDocument xDocument)
        {
            if (xDocument == null || xDocument.Root == null || xDocument.Root.Attributes() == null)
                return null;

            List<XAttribute> aAttributeList = xDocument.Root.Attributes().ToList();
            XAttribute aAttribute = aAttributeList.Find(x => x.Name.LocalName == "xmlns");
            if (aAttribute != null)
            {
                XName aName = XName.Get("category", aAttribute.Value);
                List<XElement> aXElementList = xDocument.Root.Elements(aName).ToList();
                if (aXElementList != null)
                {
                    aName = XName.Get("scheme", aAttribute.Value);
                    foreach (XElement aXElement in aXElementList)
                    {
                        XElement aXElement_Scheme = aXElement.Element(aName);
                        if (aXElement_Scheme != null && aXElement_Scheme.Value == "adsk:revit:grouping")
                        {
                            aName = XName.Get("term", aAttribute.Value);
                            XElement aXElement_Term = aXElement.Element(aName);
                            if (aXElement_Term != null)
                                return aXElement_Term.Value;
                        }
                    }
                }
            }

            return null;
        }

        /***************************************************/

        [Description("Gets Revit Category name from BHoMObject.")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("CategoryName")]
        public static string CategoryName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.CategoryName, out aValue))
            {
                if (aValue == null)
                    return null;

                return aValue.ToString();
            }

            return null;
        }

        /***************************************************/

        [Description("Gets Revit Category name from FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("CategoryName")]
        public static string CategoryName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.CategoryName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.CategoryName] as string;
        }

        /***************************************************/
    }
}
