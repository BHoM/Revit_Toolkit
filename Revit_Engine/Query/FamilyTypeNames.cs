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

using BH.oM.Adapters.Revit;
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

        [Description("Gets Revit family type names available in FamilyLibrary for given names of Revit category and family.")]
        [Input("familyLibrary", "FamilyLibrary to be queried.")]
        [Input("categoryName", "Name of Revit category to be sought for.")]
        [Input("familyName", "Name of Revit family to be sought for.")]
        [Output("familyTypeNames")]
        public static List<string> FamilyTypeNames(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null)
        {
            IEnumerable<RevitFilePreview> files = familyLibrary?.Files;
            if (files == null)
                return null;

            if (!string.IsNullOrWhiteSpace(categoryName))
                files = files.Where(x => x.CategoryName == categoryName);

            if (!string.IsNullOrWhiteSpace(familyName))
                files = files.Where(x => x.FamilyName == familyName);

            return files.SelectMany(x => x.FamilyTypeNames).ToList();
        }

        /***************************************************/

        [Description("Gets all Revit family type names in XDocument.")]
        [Input("xDocument", "XDocument from header of Revit family file (*.rfa).")]
        [Output("familyTypeNames")]
        public static List<string> FamilyTypeNames(this XDocument xDocument)
        {
            if (xDocument == null || xDocument.Root == null || xDocument.Root.Attributes() == null)
                return null;

            List<XAttribute> attributes = xDocument.Root.Attributes().ToList();
            XAttribute firstAttribute = attributes.Find(x => x.Name.LocalName == "A");
            XAttribute secondAttribute = attributes.Find(x => x.Name.LocalName == "xmlns");

            if (firstAttribute != null && secondAttribute != null)
            {
                XName name = XName.Get("family", firstAttribute.Value);
                XElement element = xDocument.Root.Element(name);
                if (element != null)
                {
                    name = XName.Get("part", firstAttribute.Value);
                    List<XElement> elements = element.Elements(name).ToList();
                    if (elements != null)
                    {
                        List<string> result = new List<string>();
                        name = XName.Get("title", secondAttribute.Value);
                        foreach (XElement elementType in elements)
                        {
                            XElement elementTitle = elementType.Element(name);
                            if (elementTitle != null && !string.IsNullOrEmpty(elementTitle.Value))
                                result.Add(elementTitle.Value);
                        }
                        return result;
                    }
                }
            }

            return null;
        }

        /***************************************************/
    }
}






