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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
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

            List<XAttribute> attributes = xDocument.Root.Attributes().ToList();
            XAttribute attribute = attributes.Find(x => x.Name.LocalName == "xmlns");
            if (attribute != null)
            {
                XName name = XName.Get("category", attribute.Value);
                List<XElement> elements = xDocument.Root.Elements(name).ToList();
                if (elements != null)
                {
                    name = XName.Get("scheme", attribute.Value);
                    foreach (XElement element in elements)
                    {
                        XElement scheme = element.Element(name);
                        if (scheme != null && scheme.Value == "adsk:revit:grouping")
                        {
                            name = XName.Get("term", attribute.Value);
                            XElement termElement = element.Element(name);
                            if (termElement != null)
                                return termElement.Value;
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

            object value = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.CategoryName, out value))
            {
                if (value == null)
                    return null;

                return value.ToString();
            }

            return null;
        }

        /***************************************************/

        //[Description("Gets Revit Category name from FilterRequest.")]
        //[Input("filterRequest", "FilterRequest")]
        //[Output("CategoryName")]
        //public static string CategoryName(this FilterRequest filterRequest)
        //{
        //    if (filterRequest == null)
        //        return null;

        //    if (!filterRequest.Equalities.ContainsKey(Convert.FilterRequest.CategoryName))
        //        return null;

        //    return filterRequest.Equalities[Convert.FilterRequest.CategoryName] as string;
        //}

        /***************************************************/
    }
}

