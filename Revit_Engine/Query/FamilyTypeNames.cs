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

using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets Revit Family Type names in FamilyLibrary for given Category and Family Name.")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("categoryName", "Category Name")]
        [Input("familyName", "Family Name")]
        [Output("FamilyTypeNames")]
        public static List<string> FamilyTypeNames(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null)
        {
            if (familyLibrary == null)
                return null;

            List<string> familyTypeNames = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> categoryDictionary = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(categoryName))
                categoryDictionary = familyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> categoryDict = null;
                if (familyLibrary.Dictionary.TryGetValue(categoryName, out categoryDict))
                    categoryDictionary.Add(categoryDict);
            }

            foreach (Dictionary<string, Dictionary<string, string>> catDict in categoryDictionary)
            {
                if (string.IsNullOrEmpty(familyName))
                {
                    familyTypeNames.AddRange(catDict.Keys);
                }
                else
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> kvp in catDict)
                    {
                        if (kvp.Value.ContainsKey(familyName))
                            familyTypeNames.Add(kvp.Key);
                    }
                }
            }

            return familyTypeNames;
        }

        /***************************************************/

        [Description("Gets all Revit Family Type names in RevitFilePreview")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Output("FamilyTypeNames")]
        public static List<string> FamilyTypeNames(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return FamilyTypeNames(revitFilePreview.XDocument());
        }

        /***************************************************/

        [Description("Gets all Revit Family Type names in XDocument")]
        [Input("xDocument", "XDocument from Header of Revit Family File (*.rfa)")]
        [Output("FamilyTypeNames")]
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



