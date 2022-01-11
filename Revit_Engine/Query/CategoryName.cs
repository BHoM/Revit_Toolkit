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

using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets Revit category name from BHoMObject.")]
        [Input("bHoMObject", "BHoMObject to be queried.")]
        [Output("categoryName")]
        public static string CategoryName(this IBHoMObject bHoMObject)
        {
            string name = bHoMObject?.GetRevitIdentifiers()?.CategoryName;
            if (string.IsNullOrWhiteSpace(name) && bHoMObject is IInstance)
                name = ((IInstance)bHoMObject).Properties.CategoryName;

            return name;
        }

        /***************************************************/

        [Description("Gets Revit category name from XDocument.")]
        [Input("xDocument", "XDocument from header of Revit family file (*.rfa).")]
        [Output("categoryName")]
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
    }
}



