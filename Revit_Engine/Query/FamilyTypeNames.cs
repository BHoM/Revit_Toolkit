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

            List<string> aFamilyTypeNames = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> aDictionaryList_Category = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(categoryName))
                aDictionaryList_Category = familyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Category = null;
                if (familyLibrary.Dictionary.TryGetValue(categoryName, out aDictionary_Category))
                    aDictionaryList_Category.Add(aDictionary_Category);
            }

            foreach (Dictionary<string, Dictionary<string, string>> aDictionary_Category in aDictionaryList_Category)
            {
                if (string.IsNullOrEmpty(familyName))
                {
                    aFamilyTypeNames.AddRange(aDictionary_Category.Keys);
                }
                else
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> aKeyValuePair_Category in aDictionary_Category)
                        if (aKeyValuePair_Category.Value.ContainsKey(familyName))
                            aFamilyTypeNames.Add(aKeyValuePair_Category.Key);
                }
            }

            return aFamilyTypeNames;
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

            List<XAttribute> aAttributeList = xDocument.Root.Attributes().ToList();
            XAttribute aFirstAttribute = aAttributeList.Find(x => x.Name.LocalName == "A");
            XAttribute aSecondAttribute = aAttributeList.Find(x => x.Name.LocalName == "xmlns");

            if (aFirstAttribute != null && aSecondAttribute != null)
            {
                XName aName = XName.Get("family", aFirstAttribute.Value);
                XElement aXElement = xDocument.Root.Element(aName);
                if (aXElement != null)
                {
                    aName = XName.Get("part", aFirstAttribute.Value);
                    List<XElement> aXElementList = aXElement.Elements(aName).ToList();
                    if (aXElementList != null)
                    {
                        List<string> aResult = new List<string>();
                        aName = XName.Get("title", aSecondAttribute.Value);
                        foreach (XElement XElement_Type in aXElementList)
                        {
                            XElement aXElement_Title = XElement_Type.Element(aName);
                            if (aXElement_Title != null && !string.IsNullOrEmpty(aXElement_Title.Value))
                                aResult.Add(aXElement_Title.Value);
                        }
                        return aResult;
                    }
                }
            }
            return null;
        }

        /***************************************************/


    }
}


