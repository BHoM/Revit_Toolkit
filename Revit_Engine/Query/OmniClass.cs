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
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns OmniClass assigned to Revit Family (RevitFilePreview)")]
        [Input("revitFilePreview", "RevitFilePreview")]
        [Output("OmniClass")]
        public static string OmniClass(this RevitFilePreview revitFilePreview)
        {
            if (revitFilePreview == null)
                return null;

            return OmniClass(revitFilePreview.XDocument());
        }

        /***************************************************/

        [Description("Returns OmniClass assigned to Revit Family (RevitFilePreview)")]
        [Input("xDocument", "XDocument from Header of Revit Family File (*.rfa)")]
        [Output("OmniClass")]
        public static string OmniClass(this XDocument xDocument)
        {
            if (xDocument == null || xDocument.Root == null)
                return null;

            List<XElement> aXElementList = xDocument.Root.Elements().ToList();
            if (aXElementList != null && aXElementList.Count > 0)
            {
                XName aName = XName.Get("category", "http://www.w3.org/2005/Atom");
                aXElementList = aXElementList.FindAll(x => x.Name == aName);
                if (aXElementList != null)
                    foreach (XElement aXElement in aXElementList)
                    {
                        List<XElement> aChildXElementList = aXElement.Elements().ToList();
                        aName = XName.Get("scheme", "http://www.w3.org/2005/Atom");

                        if (aChildXElementList != null && aChildXElementList.Find(x => x.Name == aName && x.Value == "std:oc1") != null)
                        {
                            aName = XName.Get("term", "http://www.w3.org/2005/Atom");
                            XElement aChildXElement = aChildXElementList.Find(x => x.Name == aName);
                            if (aChildXElement != null)
                                return aChildXElement.Value;
                        }
                    }
            }

            return null;
        }

        /***************************************************/
    }
}

